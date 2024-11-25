using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Product.Dal;
using Product.Dal.Entities;
using Product.Financial;
using Product.Financial.Models;
using Product.Integration.Constants;
using Product.Integration.Models.Data.Pub;
using Product.Web.Models.Configuration;
using Product.Web.Models.Financial;
using Product.Web.Models.Response;
using System.Text.Json;

namespace Product.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FinancialController : ControllerBase
    {
        private readonly DBContext _db;
        private readonly BloombergDataService _bloombergDataService;
        private readonly FdeConfig _fdeConfig;
        private readonly ILogger<FinancialController> _logger;

        public FinancialController(BloombergDataService bloombergDataService, DBContext db, IOptions<FdeConfig> fdeConfig, ILogger<FinancialController> logger)
        {
            _bloombergDataService = bloombergDataService;
            _db = db;
            _fdeConfig = fdeConfig.Value;
            _logger = logger;
        }

        [HttpGet("TriggerDataSync")]
        public async Task<IActionResult> TriggerDataSync()
        {
            await _bloombergDataService.FetchAndUpdateDataAsync();
            return Ok("Job Completed.");
        }


        [HttpGet("Get/MetaData")]
        public async Task<IActionResult> GetFinMetaData()
        {
            try
            {
                var metaData = _db.FinCode
                    .Select(x => new
                    {
                        StatementType = x.FinStatementType.ToString(),
                        FinCode = x.Code,
                        FinTitle = x.FinTitle,
                        Group = x.FinCodeGroup != null ? x.FinCodeGroup.GroupTitle : null,
                        GroupSortOrder = x.FinCodeGroup != null ? x.FinCodeGroup.SortOrder : (int?)null,
                        SortOrder = x.SortOrder,
                        Style = x.Style,
                        IsCalc = x.IsCalc
                    })
                    .ToList() // Execute the query and bring the data to memory
                    .GroupBy(x => x.StatementType) // Group by StatementType
                    .ToDictionary(
                        g => g.Key, // The key is the StatementType
                        g => g.Select(item => new
                        {
                            item.FinCode,
                            item.FinTitle,
                            item.Group,
                            item.GroupSortOrder,
                            item.SortOrder,
                            item.Style,
                            item.IsCalc
                        }).ToList() // The value is a list of items for that StatementType
                    );
                
                return Ok(metaData);

            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.Message });
            }
        }

        [HttpGet("Get/Jobs")]
        public async Task<IActionResult> GetExtractJobs()
        {
            try
            {
                var metaData = _db.FinancialExtractJob
                    .Select(x => new
                    {
                        x.EntityId,
                        ClientId = x.Entity.ClientId,
                        EntityName = x.Entity.Name,
                        ClientName = x.Entity.Client.Name,
                        x.JobId,
                        x.FileName,
                        Stage = x.Stage.ToString(),
                        Status = x.Status.ToString(),
                        x.IsCompleted,
                        SubmittedAt = x.SubmittedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                        FinishedAt = x.FinishedAt.HasValue ? x.FinishedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : null
                    })
                    .ToList();

                return Ok(metaData);

            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.Message });
            }
        }

        [HttpPost("Calculate")]
        public async Task<IActionResult> GetCalculatedData([FromBody] Dictionary<string, Dictionary<string, Dictionary<string, FinancialItem>>> inputData)
        {
            var calcTotals = await _db.FinCode.Where(fc => fc.IsCalc == true).ToListAsync();
            var isTotals = calcTotals.Where(ct => ct.FinStatementType == Dal.Enums.FinStatementType.IS).OrderBy(ct => ct.SortOrder);
            var bsTotals = calcTotals.Where(ct => ct.FinStatementType == Dal.Enums.FinStatementType.BS).OrderBy(ct => ct.SortOrder);

            var formulas = new List<FinancialFormula>();

            foreach (var formula in calcTotals)
            {
                formulas.Add(new FinancialFormula { Identifier = formula.Code, Expression = formula.Formula });
            }

            var evaluator = new FormulaEvaluatorService(formulas);

            foreach (var statementType in inputData.Keys)
            {
                var yearData = inputData[statementType];
                foreach (var statementYear in yearData.Keys)
                {
                    var statementData = yearData[statementYear];

                    var inputs = new Dictionary<string, decimal?>();
                    foreach (FinancialItem item in statementData.Values)
                    {
                        inputs.Add(item.FinCode, item.FinValue);
                    }

                    if (statementType.Equals("BS"))
                    {
                        foreach (var bsCalcItem in bsTotals)
                        {
                            decimal newValue = evaluator.Evaluate(bsCalcItem.Code, inputs);
                            inputs[bsCalcItem.Code] = newValue;
                            UpdateFinValue(inputData, statementType, statementYear, bsCalcItem.Code, newValue);
                        }
                    }

                    if (statementType.Equals("IS"))
                    {
                        foreach (var isCalcItem in isTotals)
                        {
                            decimal newValue = evaluator.Evaluate(isCalcItem.Code, inputs);
                            inputs[isCalcItem.Code] = newValue;
                            UpdateFinValue(inputData, statementType, statementYear, isCalcItem.Code, newValue);
                        }
                    }
                }
            }

            return Ok(inputData);
        }

        private void UpdateFinValue(Dictionary<string, Dictionary<string, Dictionary<string, FinancialItem>>> groupedData, string statementType, string year, string finCode, decimal value)
        {
            // Check and update FinValue if the keys exist
            if (groupedData.TryGetValue(statementType, out var yearData) &&
                yearData.TryGetValue(year, out var finCodeData) &&
                finCodeData.TryGetValue(finCode, out var item))
            {
                // Update the FinValue directly
                if (item.FinValue != value)
                {
                    item.FinValue = value;
                    item.IsEdited = true;
                }
            }
            else
            {
                _logger.LogInformation("Item not found for updating. ST: {ST}, Year: {Year}, FinCode: {FinCode}", statementType, year, finCode);
            }
        }

        [HttpPut("UpdateFinItems")]
        public async Task<IActionResult> UpdateFinancialData([FromBody] Dictionary<string, Dictionary<string, Dictionary<string, FinancialItem>>> inputData)
        {
            try
            {
                foreach (var statementType in inputData.Keys)
                {
                    var yearData = inputData[statementType];
                    foreach (var statementYear in yearData.Keys)
                    {
                        var statementData = yearData[statementYear];

                        //var inputs = new Dictionary<string, decimal?>();
                        foreach (FinancialItem item in statementData.Values)
                        {
                            bool isEdited = item.IsEdited ?? false;

                            if (isEdited)
                            {
                                var finItem = await _db.Financial.FirstOrDefaultAsync(f => f.Id == item.Id);
                                if (finItem != null)
                                {
                                    finItem.FinValue = item.FinValue;
                                    _db.Financial.Update(finItem);
                                }
                            }
                        }
                    }
                }

                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save financial data.");
                return StatusCode(500, new ActionResponse
                {
                    Result = 1,
                    Message = "An error occurred while updating financials."
                });
            }

            return Ok(new ActionResponse { Result = 0, Message = "Financials updated." });
        }

        [HttpGet("Get/{entityId}")]
        public async Task<IActionResult> GetFinancialData(int entityId)
        {

            try
            {
                var financialData = _db.Financial
                    .Where(f => f.FinancialStatement.EntityId == entityId)
                    .Include(f => f.FinancialStatement)
                    .Include(f => f.Code)
                    .Select(f => new
                    {
                        Id = f.Id,
                        Year = f.FinancialStatement.FinancialYear,
                        Type = f.Code.FinStatementType.ToString(), // Convert enum to string
                        FinCode = f.FinCode,
                        FinTitle = f.Code.FinTitle,
                        FinValue = f.FinValue
                    })
                    .ToList();

                // Build a nested dictionary for quick lookups
                Dictionary<string, Dictionary<string, Dictionary<string, FinancialItem>>> groupedData = new Dictionary<string, Dictionary<string, Dictionary<string, FinancialItem>>>();

                foreach (var typeGroup in financialData.GroupBy(f => f.Type))
                {
                    if (!groupedData.ContainsKey(typeGroup.Key))
                    {
                        groupedData[typeGroup.Key] = typeGroup
                            .GroupBy(item => item.Year)
                            .ToDictionary(
                                yearGroup => yearGroup.Key.ToString(),
                                yearGroup =>
                                {
                                    var finCodeDict = new Dictionary<string, FinancialItem>();
                                    foreach (var item in yearGroup)
                                    {
                                        if (!finCodeDict.ContainsKey(item.FinCode))
                                        {
                                            finCodeDict[item.FinCode] = new FinancialItem
                                            {
                                                Id = item.Id,
                                                FinCode = item.FinCode,
                                                Type = item.Type,
                                                Year = item.Year,
                                                FinTitle = item.FinTitle,
                                                FinValue = item.FinValue,
                                                IsEdited = false
                                            };
                                        }
                                    }
                                    return finCodeDict;
                                }
                            );
                    }
                    else
                    {
                        // Handle duplicate parent-level keys (e.g., log them or merge data)
                        Console.WriteLine($"Duplicate parent type found: {typeGroup.Key}");
                        // Optionally merge or skip
                    }
                }

                return Ok(groupedData);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.Message });
            }
        }

        /// <summary>
        /// Submit a file for extraction
        /// </summary>
        /// <param name="file">The file to upload</param>
        /// <param name="entityId">The entity ID for the request</param>
        /// <returns></returns>
        [RequestSizeLimit(33_554_432)] // Set to 32MB
        [HttpPost("SubmitForExtraction")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SubmitForExtraction(IFormFile file, string entityId)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            if (!await IsValidPdf(file))
            {
                return BadRequest("Not a valid PDF file.");
            }

            var originalFileName = file.FileName;
            Guid jobId = Guid.NewGuid();
            var directoryPath = Path.Combine(_fdeConfig.DocumentPath, jobId.ToString());
            var filePath = Path.Combine(directoryPath, _fdeConfig.DocumentName);

            Directory.CreateDirectory(directoryPath);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var dateTime = DateTime.UtcNow;
            var localDateTime = dateTime.ToLocalTime();

            // TO BE CHNAGED: Temporarily using a dummy entity for testing
            //var entityIdOverride = await CreateDummyEntity();

            var newJob = new FinancialExtractJob
            {
                EntityId = int.Parse(entityId),
                FileName = originalFileName,
                IsCompleted = false,
                Status = Dal.Enums.FdeStatus.InProgress,
                SubmittedAt = localDateTime,
                Stage = Dal.Enums.FdeStage.Submitted,
                JobId = jobId.ToString(),
                SubmittedBy = "system"
            };

            _db.FinancialExtractJob.Add(newJob);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.Message });
            }

            var jobStatus = new FinancialExtractJobStatus
            {
                ExtractJobId = newJob.Id,
                Stage = Dal.Enums.FdeStage.Submitted,
                Status = Dal.Enums.FdeStatus.Success,
                UpdatedAt = localDateTime
            };

            _db.FinancialExtractJobStatus.Add(jobStatus);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.Message });
            }

            var request = new FinDataExtract
            {
                EntityID = entityId.ToString(),
                FilePath = filePath,
                JobId = jobId.ToString()
            };

            ZeroMqPubService.PublishMessage(IntegrationConstants.MqTopicPubExtract, JsonSerializer.Serialize(request));

            return Ok(new ActionResponse { Result = 0, Message = "Request submitted." });
        }

        // Temporary method to insert a dummy entity for testing purposes
        private async Task<int> CreateDummyEntity()
        {
            var newEntity = new Product.Dal.Entities.Entity
            {
                ClientId = 2,
                Name = "CSL",
                Country = "AU",
                SourceSystemName = "TEST",
                SourceId = "1",
                EntityType = "TEST",
                PolicyCountry = "AU",
                PolicyStatus = "TEST",
                IsValidCustomer = true,
                CCR = "1",
                SI = "1",
                SourceEntityInformation = "",
                Ticker = "CSL"
            };

            try
            {
                // Add to database
                _db.Entities.Add(newEntity);
                await _db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return 0;
            }

            return newEntity.Id;
        }

        private async Task<bool> IsValidPdf(IFormFile file)
        {
            if (file == null || file.Length < 10) // Check if file exists and has minimal size
                return false;

            // Read the first few bytes to check for PDF header
            byte[] header = new byte[5];
            using (var stream = file.OpenReadStream())
            {
                await stream.ReadAsync(header, 0, header.Length);

                // Check if header starts with "%PDF-"
                string headerText = System.Text.Encoding.ASCII.GetString(header);
                if (headerText != "%PDF-")
                    return false;

                // Check for PDF end-of-file marker "%%EOF"
                // Read the last 1024 bytes to search for the EOF marker
                long footerLength = Math.Min(1024, stream.Length);
                byte[] footer = new byte[footerLength];
                stream.Seek(-footerLength, SeekOrigin.End);
                await stream.ReadAsync(footer, 0, footer.Length);

                string footerText = System.Text.Encoding.ASCII.GetString(footer);
                if (!footerText.Contains("%%EOF"))
                    return false;
            }

            // If both checks pass, the file is likely a valid PDF
            return true;
        }
    }
    
}
