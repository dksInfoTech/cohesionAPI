using System.Text.Json;
using CsvHelper;
using System.Globalization;
using Npgsql;
using System.Text;
using Product.Integration.Interfaces;
using CsvHelper.Configuration;
using System.IO.Compression;
using Product.Integration.Models;
using Product.Integration.Models.Data.Response;
using Product.Dal;
using System.Collections.Immutable;

public class BloombergDataService
{
    private readonly string _dbConnectionString;
    private readonly IIntegrationService _bloombergIntegrationService;
    private readonly IntegrationConfig _integrationConfig;
    private ImmutableDictionary<string, string> expectedFiles = ImmutableDictionary.CreateRange(new Dictionary<string, string>
    {
        { "Entity", "RecEntityDataV1" },
        { "Estimate", "RecEstimateData5YearsV1" },
        { "Financial", "RecFinancialData5YearsV2" },
        { "RiskRating", "RecRiskRatingDataV1" }
    });
    private readonly string[] finDataRowExcludeList = { "DL_REQUEST_ID", "DL_REQUEST_NAME", "DL_SNAPSHOT_START_TIME", "DL_SNAPSHOT_TZ", "RC", "IDENTIFIER", "TICKER", "EQY_FUND_YEAR", "FISCAL_YEAR_PERIOD", "EQY_FISCAL_YR_END", "FUND_FISCAL_MONTH", "ISSUER_FINANCIAL_STATEMENT_TYPE", "FILING_STATUS", "CRNCY", "PX_SCALING_FACTOR", "SCALING_FORMAT", "ID_CUSIP", "ID_ISIN", "ID_BB", "ID_BB_UNIQUE", "ID_BB_GLOBAL", "PRIMARY_PERIODICITY", "FUNDAMENTALS_TICKER" };

    public BloombergDataService(IIntegrationService bloombergIntegrationService, IntegrationConfig integrationConfig, string dbConnectionString)
    {
        _bloombergIntegrationService = bloombergIntegrationService;
        _integrationConfig = integrationConfig;
        _dbConnectionString = dbConnectionString;
    }

    private bool ContainsAllFiles(Dictionary<string, string> files)
    {
        return files.Keys.Any(key => expectedFiles.Values.Contains(key));
    }

    public async Task FetchAndUpdateDataAsync()
    {
        var files = await GetLatestFiles();

        if (files.Count == 0 || !ContainsAllFiles(files))
        {
            return;
        }

        var entityCsvFile = files[expectedFiles["Entity"]];
        var entityUrl = $"{_integrationConfig.BloombergConfig.DataApiEndpoint}/eap/catalogs/{_integrationConfig.BloombergConfig.DataLicense}/content/responses/{entityCsvFile}";
        using var entityDataStream = await GetDecompressedCsvStream(entityUrl);

        var financialCsvFile = files[expectedFiles["Financial"]];
        var financialUrl = $"{_integrationConfig.BloombergConfig.DataApiEndpoint}/eap/catalogs/{_integrationConfig.BloombergConfig.DataLicense}/content/responses/{financialCsvFile}";
        using var financialDataStream = await GetDecompressedCsvStream(financialUrl);

        var riskRatingCsvFile = files[expectedFiles["RiskRating"]];
        var riskRatinglUrl = $"{_integrationConfig.BloombergConfig.DataApiEndpoint}/eap/catalogs/{_integrationConfig.BloombergConfig.DataLicense}/content/responses/{riskRatingCsvFile}";
        using var riskRatingDataStream = await GetDecompressedCsvStream(riskRatinglUrl);

        await using var connection = new NpgsqlConnection(_dbConnectionString);
        await connection.OpenAsync();

        try
        {
            var finMetaDataCsvMapping = GetCsvMappings("FinMetaDataMappings.csv");
            var entityCsvMapping = GetCsvMappings("EntityDataMappings.csv");
            var riskRatingCsvMapping = GetCsvMappings("RiskRatingDataMappings.csv");
            var financialCsvMapping = GetCsvMappings("FinancialDataMappings.csv");

            await UpdateTempMetaData(connection, finMetaDataCsvMapping, "TempFinMetaData");
            await UpdateTempData(connection, entityDataStream, entityCsvMapping, "TempEntityData");
            await UpdateTempData(connection, riskRatingDataStream, riskRatingCsvMapping, "TempRiskRatingData");
            await UpdateTempHistoricalFinancials(connection, financialDataStream, financialCsvMapping, "TempFinancialData");

            await MergeData(connection, finMetaDataCsvMapping, "SourceFinancialCode", "TempFinMetaData");
            await MergeData(connection, entityCsvMapping, "SourceEntity", "TempEntityData");
            await MergeData(connection, riskRatingCsvMapping, "SourceRating", "TempRiskRatingData");
            await MergeData(connection, financialCsvMapping, "SourceFinancial", "TempFinancialData");
        }
        catch (Exception e)
        {
            throw;
        }
        finally
        {
            await connection.CloseAsync();
        }
    }

    private async Task<Dictionary<string, string>> GetLatestFiles()
    {
        // Bloomberg timezone for file generation is in Tokyo timezone. Currently we generate the files at midnight everyday.
        var tokyoTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
        var tokyoTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tokyoTimeZone);
        var date = DateTime.Now.ToString("yyyy-MM-dd");

        if (tokyoTime.TimeOfDay >= TimeSpan.FromHours(0) && tokyoTime.TimeOfDay <= TimeSpan.FromHours(2.5))
        {
            date = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
        }

        var files = new Dictionary<string, string>();
        var fileListUrl = $"{_integrationConfig.BloombergConfig.DataApiEndpoint}/eap/catalogs/{_integrationConfig.BloombergConfig.DataLicense}/content/responses/?snapshotStartDateTime={date}T00:00:00";
        var filesResponse = await _bloombergIntegrationService.ExecuteGetRequestAsync(fileListUrl);

        try
        {
            var content = await filesResponse.Content.ReadAsStringAsync();
            // Deserialize JSON response into TResponse (your custom class)
            var data = JsonSerializer.Deserialize<BBFileResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true  // Handle case insensitivity in JSON keys
            });

            if (data != null)
            {
                foreach (var item in data.Contains)
                {
                    if (!files.ContainsKey(item.Metadata.DL_REQUEST_NAME))
                    {
                        files.Add(item.Metadata.DL_REQUEST_NAME, item.Key);
                    }
                }
            }
        }
        catch (JsonException ex)
        {
            // Handle any deserialization issues
            throw new InvalidOperationException($"Failed to deserialize response into {typeof(TResponse).Name}: {ex.Message}", ex);
        }

        return files;
    }

    private async Task UpdateTempMetaData(NpgsqlConnection connection, IEnumerable<dynamic> csvMappings, string tempTableName)
    {
        var metaDataUrl = $"{_integrationConfig.BloombergConfig.DataApiEndpoint}/eap/catalogs/{_integrationConfig.BloombergConfig.DataLicense}/requests/{_integrationConfig.BloombergConfig.FinMetaDataIdentifier}/fieldList/?pageSize={_integrationConfig.BloombergConfig.FinMetaDataPageSize}&page=1";
        var metaDataResponse = await _bloombergIntegrationService.ExecuteGetRequestAsync(metaDataUrl);

        try
        {
            var content = await metaDataResponse.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<BBFinMetaDataResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true  // Handle case insensitivity in JSON keys
            });

            if (data != null)
            {
                await CreateTempTable(connection, tempTableName, csvMappings);

                var columns = string.Join(", ", csvMappings.Select(c => $"\"{c.TEMP_COLUMN}\""));
                using var writer = connection.BeginTextImport($"COPY \"{tempTableName}\" ({columns}) FROM STDIN (FORMAT CSV)");

                foreach (var item in data.Contains)
                {
                    await writer.WriteLineAsync($"{item.Mnemonic},\"{item.Title}\"");
                }
            }
        }
        catch (JsonException ex)
        {
            // Handle any deserialization issues
            throw new InvalidOperationException($"Failed to deserialize response into {typeof(TResponse).Name}: {ex.Message}", ex);
        }
    }

    private async Task CreateTempTable(NpgsqlConnection connection, string tableName, IEnumerable<dynamic> csvMappings) 
    {
        // Drop the table if it exists
        var dropIfExists = $"DROP TABLE IF EXISTS \"{tableName}\";";
        using var dropCmd = new NpgsqlCommand(dropIfExists, connection);
        await dropCmd.ExecuteNonQueryAsync();

        // Create the table with dynamic columns
        var sqlBuilderTable = new StringBuilder($"CREATE TABLE \"{tableName}\" (");
        bool isFirst = true;
        foreach (var column in csvMappings)
        {
            if (!isFirst) sqlBuilderTable.Append(", ");
            sqlBuilderTable.Append($"\"{column.TEMP_COLUMN}\" {column.TEMP_DATA_TYPE}");
            isFirst = false;
        }
        sqlBuilderTable.Append(");");

        var sql = sqlBuilderTable.ToString();

        using var createCmd = new NpgsqlCommand(sqlBuilderTable.ToString(), connection);
        await createCmd.ExecuteNonQueryAsync();
    }

    private async Task InsertTempData(NpgsqlConnection connection, string tableName, IEnumerable<dynamic> csvMappings, IEnumerable<dynamic> csvData)
    {
        await CreateTempTable(connection, tableName, csvMappings);

        // Prepare the COPY command
        var columns = string.Join(", ", csvMappings.Select(c => $"\"{c.TEMP_COLUMN}\""));
        using var writer = connection.BeginTextImport($"COPY \"{tableName}\" ({columns}) FROM STDIN (FORMAT CSV, QUOTE '\"')");

        // Insert data into the table
        foreach (var record in csvData)
        {
            var values = csvMappings.Select(c =>
            {
                var value = GetFieldValue(record, c.TEMP_COLUMN);
                return string.IsNullOrEmpty(value) ? "" : $"\"{value}\"";  // Empty string for NULLs
            });

            string row = string.Join(",", values);
            await writer.WriteLineAsync(row);
        }
    }

    private async Task UpdateTempData(NpgsqlConnection connection, GZipStream csvStream, IEnumerable<dynamic> csvMappings, string tempTableName)
    {
        using var reader = new StreamReader(csvStream);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            PrepareHeaderForMatch = args => args.Header.Trim(),
            HeaderValidated = null, // Ignore header mismatches
            MissingFieldFound = null, // Ignore missing fields
            Quote = '"',
            Delimiter = ","
        });

        var csvData = csv.GetRecords<dynamic>().ToList();

        await InsertTempData(connection, tempTableName, csvMappings, csvData);
    }

    private async Task UpdateTempHistoricalFinancials(NpgsqlConnection connection, GZipStream csvStream, IEnumerable<dynamic> csvMappings, string tempTableName)
    {
        await CreateTempTable(connection, tempTableName, csvMappings);

        using var reader = new StreamReader(csvStream);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            PrepareHeaderForMatch = args => args.Header.Trim(),
            HeaderValidated = null, // Ignore header mismatches
            MissingFieldFound = null, // Ignore missing fields
            Quote = '"',
            Delimiter = ","
        });
        var csvData = csv.GetRecords<dynamic>().ToList();
        var headers = csv.HeaderRecord;

        // Prepare COPY command
        var columns = string.Join(", ", csvMappings.Select(c => $"\"{c.TEMP_COLUMN}\""));
        using var writer = connection.BeginTextImport($"COPY \"{tempTableName}\" ({columns}) FROM STDIN (FORMAT CSV)");

        // Read data and write each column as a row
        foreach (var record in csvData)
        {
            foreach (var header in headers)
            {
                if (!finDataRowExcludeList.Contains(header))
                {
                    string value = GetFieldValue(record, header);
                    await writer.WriteLineAsync($"{GetFieldValue(record, "TICKER")}," +
                        $"{GetFieldValue(record, "EQY_FUND_YEAR")}," +
                        $"{GetFieldValue(record, "CRNCY")}," +
                        $"{GetFieldValue(record, "PX_SCALING_FACTOR")}," +
                        $"{header},{value}");
                }
            }
        }
    }

    private async Task<bool> MergeData(NpgsqlConnection connection, IEnumerable<dynamic> csvMappings, string destinationTable, string tempTable)
    {
        var upsertSql = GenerateUpsertSql(destinationTable, tempTable, csvMappings);
        using var createCmd = new NpgsqlCommand(upsertSql, connection);
        var result = await createCmd.ExecuteNonQueryAsync();
        return result > 0;
    }

    private async Task<GZipStream> GetDecompressedCsvStream(string apiUrl)
    {
        var response = await _bloombergIntegrationService.ExecuteGetRequestAsync(apiUrl);
        var stream = await response.Content.ReadAsStreamAsync();
        return new GZipStream(stream, CompressionMode.Decompress);
    }

    private IEnumerable<dynamic> GetCsvMappings(string fileName)
    {
        string filePath = Path.Combine(AppContext.BaseDirectory, "Resources", fileName);
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            PrepareHeaderForMatch = args => args.Header.Trim(),  // Trim headers
            MissingFieldFound = null  // Ignore missing fields
        });

        return csv.GetRecords<dynamic>().ToList();
    }

    // Helper method to safely access fields from a dynamic object
    private static string GetFieldValue(dynamic record, string fieldName)
    {
        var dictionary = record as IDictionary<string, object>;
        if (dictionary != null && dictionary.ContainsKey(fieldName))
        {
            var value = dictionary[fieldName]?.ToString();
            return string.IsNullOrWhiteSpace(value) ? "" : value;
        }
        return "";
    }

    private static string GenerateUpsertSql(string sourceTable, string tempTable, IEnumerable<dynamic> csvMappings)
    {
        // Ensure column names are properly quoted
        var keyColumns = csvMappings
            .Where(c => c.MAPPED_COLUMN_TYPE == "KEY")
            .Select(c => $"\"{c.MAPPED_COLUMN}\"");

        var insertCols = csvMappings
            .Select(c => $"\"{c.MAPPED_COLUMN}\"");

        var dataColumns = csvMappings
            .Where(c => c.MAPPED_COLUMN_TYPE == "DATA")
            .Select(c => $"\"{c.MAPPED_COLUMN}\"");

        // Prepare the column lists for the insert and select parts
        var insertColumns = string.Join(", ", insertCols);
        var selectColumns = string.Join(", ", csvMappings.Select(c => $"\"{c.TEMP_COLUMN}\""));

        // Build the SQL parts
        var insertPart = $"INSERT INTO \"{ sourceTable}\" ({insertColumns}) SELECT { selectColumns} FROM \"{tempTable}\" ";

        var conflictColumns = string.Join(", ", keyColumns);
        var updateColumns = string.Join(",\n    ", dataColumns.Select(c => $"{c} = EXCLUDED.{c}"));

        var conflictPart = $@"
                    ON CONFLICT ({conflictColumns}) DO UPDATE
                    SET {updateColumns};";

        // Combine and return the final SQL string
        return $"{insertPart}\n{conflictPart}";
    }
}
