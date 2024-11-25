using Newtonsoft.Json.Linq;
using Product.Dal.Common.Utils;
using Product.Dal.Entities;
using System.Linq;

namespace Product.Dal.Common.Data.Seed;

public static class ReferenceSeedData
{
    public static List<ReferenceType> GetReferenceTypes()
    {
        var referenceDataTypes = new List<string>
                                    {
                                        "CounterpartyCreditRating", "ClientSegment", "HighRiskStatus", "BusinessUnit",
                                        "Delegationauthority", "Country", "EntityType", "Location", "ControlType", "Priority",
                                        "Group", "RecallReasons", "SectionCategory", "PortfolioMetrics", "Product",
                                        "ProductClass", "LimitTenor", "ProductLimitTenor", "RiskGrade", "FacilityCommitted",
                                        "DocumentStatus", "DocumentClassification", "TenorType", "LimitType", "CadLevel",
                                        "CurrencyList", "Industry", "Sector", "YesNo", "RiskMitigation", "RiskType", "DecisionRationales","AUDExchangeRateTable"
                                    };
        var filteringAllowedKeys = new HashSet<string> { "Location", "Country", "BusinessUnit", "ClientSegment" };
        var clientFilterAllowedKeys = new HashSet<string> { "BusinessUnit", "ClientSegment" };

        var referenceTypes = new List<ReferenceType>();
        foreach (var refKey in referenceDataTypes)
        {
            referenceTypes.Add(new ReferenceType
            {
                RefKey = refKey,
                IsFilteringAllowed = filteringAllowedKeys.Contains(refKey),
                IsClientFilterAllowed = clientFilterAllowedKeys.Contains(refKey)
            });
        }

        return referenceTypes;
    }

    private static int GetRefTypeId(DBContext context, string type)
    {
        return context.ReferenceType.FirstOrDefault(x => x.RefKey == type).Id;
    }

    public static List<ReferenceData> GetReferenceData(DBContext context)
    {
        // Get the list of reference types from the method
        var referenceTypes = GetReferenceTypes();

        // Use LINQ to create the list of ReferenceData objects
        return referenceTypes.Select(refType => new ReferenceData
        {
            RefTypeId = GetRefTypeId(context, refType.RefKey),  // Use RefKey to get RefTypeId
            RefValue = ReadFilteredJsonAsString(refType.RefKey) // Use RefKey to get RefValue
        }).ToList();

    }

    private static string ReadFilteredJsonAsString(string type)
    {
        var jsonString = EmbeddedResourceUtil.ReadAsString("Product.Dal", $"Common.Data.Json.reference-data.json");
        var refData = JObject.Parse(jsonString);
        return refData[type]?.ToString();
    }
    private static string ReadJsonAsString(string fileName)
    {
        return EmbeddedResourceUtil.ReadAsString("Product.Dal", $"Data.Json.{fileName}");
    }
}
