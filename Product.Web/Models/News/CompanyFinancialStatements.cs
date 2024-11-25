using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Product.Web.Models.News.Financial
{
    public class CompanyFinancialStatements
    {
        [JsonProperty("statusCode")]
        public Dictionary<string, int> StatusCode { get; set; }

        [JsonProperty("message")]
        public Dictionary<string, string> Message { get; set; }

        [JsonProperty("result")]
        public Result Result { get; set; }

        [JsonProperty("_id")]
        public string Id { get; set; }
    }

    public class Result
    {
        [JsonProperty("ts")]
        public Dictionary<string, long> Ts { get; set; }

        [JsonProperty("market_data")]
        public List<MarketData> MarketData { get; set; }
    }

    public class MarketData
    {
        [JsonProperty("ric")]
        public string Ric { get; set; }

        [JsonProperty("retryInSec")]
        public int RetryInSec { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("localName")]
        public string LocalName { get; set; }

        [JsonProperty("financialStatements")]
        public FinancialStatements FinancialStatements { get; set; }

        [JsonProperty("originalRic")]
        public string OriginalRic { get; set; }
    }

    public class FinancialStatements
    {
        [JsonProperty("income")]
        public IncomeStatement Income { get; set; }

        [JsonProperty("balanceSheet")]
        public BalanceSheet BalanceSheet { get; set; }

        [JsonProperty("cashFlow")]
        public CashFlow CashFlow { get; set; }
    }

    public class IncomeStatement
    {
        [JsonProperty("annual")]
        public AnnualData Annual { get; set; }

        [JsonProperty("interim")]
        public InterimData Interim { get; set; }
    }

    public class BalanceSheet
    {
        [JsonProperty("annual")]
        public AnnualData Annual { get; set; }

        [JsonProperty("interim")]
        public InterimData Interim { get; set; }
    }

    public class CashFlow
    {
        [JsonProperty("annual")]
        public AnnualData Annual { get; set; }

        [JsonProperty("interim")]
        public InterimData Interim { get; set; }
    }

    public class AnnualData
    {
        [JsonProperty("totalAssets")]
        public FinancialItem TotalAssets { get; set; }

        [JsonProperty("totalLiabilities")]
        public FinancialItem TotalLiabilities { get; set; }

        [JsonProperty("totalDebt")]
        public FinancialItem TotalDebt { get; set; }

        [JsonProperty("cashFromOperatingActivities")]
        public FinancialItem CashFromOperatingActivities { get; set; }

        [JsonProperty("cashFromInvestingActivities")]
        public FinancialItem CashFromInvestingActivities { get; set; }

        [JsonProperty("financingCashFlowItems")]
        public FinancialItem FinancingCashFlowItems { get; set; }

        [JsonProperty("revenue")]
        public FinancialItem Revenue { get; set; }

        [JsonProperty("netIncome")]
        public FinancialItem NetIncome { get; set; }

        [JsonProperty("grossProfit")]
        public FinancialItem GrossProfit { get; set; }
    }

    public class InterimData
    {
        [JsonProperty("totalAssets")]
        public FinancialItem TotalAssets { get; set; }

        [JsonProperty("totalLiabilities")]
        public FinancialItem TotalLiabilities { get; set; }

        [JsonProperty("totalDebt")]
        public FinancialItem TotalDebt { get; set; }

        [JsonProperty("cashFromOperatingActivities")]
        public FinancialItem CashFromOperatingActivities { get; set; }

        [JsonProperty("cashFromInvestingActivities")]
        public FinancialItem CashFromInvestingActivities { get; set; }

        [JsonProperty("financingCashFlowItems")]
        public FinancialItem FinancingCashFlowItems { get; set; }

        [JsonProperty("revenue")]
        public FinancialItem Revenue { get; set; }

        [JsonProperty("netIncome")]
        public FinancialItem NetIncome { get; set; }

        [JsonProperty("grossProfit")]
        public FinancialItem GrossProfit { get; set; }
    }

    public class FinancialItem
    {
        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("items")]
        public List<FinancialItemDetail> Items { get; set; }
    }

    public class FinancialItemDetail
    {
        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

}
