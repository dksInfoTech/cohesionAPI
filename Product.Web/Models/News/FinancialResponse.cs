using Newtonsoft.Json;

namespace Product.Web.Models.News
{
   public class FinancialResponse
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

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("localName")]
        public string LocalName { get; set; }

        [JsonProperty("price_closing_or_last_bid")]
        public string PriceClosingOrLastBid { get; set; }

        [JsonProperty("price_date")]
        public DateTime PriceDate { get; set; }

        [JsonProperty("fiftytwo_wk_high")]
        public double FiftyTwoWkHigh { get; set; }

        [JsonProperty("fiftytwo_week_high_date")]
        public DateTime FiftyTwoWeekHighDate { get; set; }

        [JsonProperty("fiftytwo_wk_low")]
        public double FiftyTwoWkLow { get; set; }

        [JsonProperty("fiftytwo_week_low_date")]
        public DateTime FiftyTwoWeekLowDate { get; set; }

        [JsonProperty("share_volume_10d")]
        public string ShareVolume10d { get; set; }

        [JsonProperty("share_volume_3m")]
        public string ShareVolume3m { get; set; }

        [JsonProperty("market_cap")]
        public string MarketCap { get; set; }

        [JsonProperty("beta")]
        public string Beta { get; set; }

        [JsonProperty("one_day_price")]
        public string OneDayPrice { get; set; }

        [JsonProperty("five_day_price_return")]
        public string FiveDayPriceReturn { get; set; }

        [JsonProperty("thirteen_wk_price_return")]
        public string ThirteenWkPriceReturn { get; set; }

        [JsonProperty("twenty_six_wk_price_return")]
        public string TwentySixWkPriceReturn { get; set; }

        [JsonProperty("fiftytwo_wk_price_return")]
        public string FiftyTwoWkPriceReturn { get; set; }

        [JsonProperty("mtd_price_return")]
        public string MtdPriceReturn { get; set; }

        [JsonProperty("ytd_price_return")]
        public string YtdPriceReturn { get; set; }

        [JsonProperty("four_week_rel_sandp500")]
        public string FourWeekRelSandp500 { get; set; }

        [JsonProperty("thirteen_week_rel_sandp500")]
        public string ThirteenWeekRelSandp500 { get; set; }

        [JsonProperty("twentysix_week_rel_sandp500")]
        public string TwentySixWeekRelSandp500 { get; set; }

        [JsonProperty("fiftytwo_week_rel_sandp500")]
        public string FiftyTwoWeekRelSandp500 { get; set; }

        [JsonProperty("ytd_rel_sandp500")]
        public string YtdRelSandp500 { get; set; }

        [JsonProperty("eps_excl_extra_ttm")]
        public string EpsExclExtraTtm { get; set; }

        [JsonProperty("eps_excl_extra_annual")]
        public string EpsExclExtraAnnual { get; set; }

        [JsonProperty("eps_normalized_annual")]
        public string EpsNormalizedAnnual { get; set; }

        [JsonProperty("eps_basic_excl_extra_annual")]
        public string EpsBasicExclExtraAnnual { get; set; }

        [JsonProperty("eps_basic_excl_extra_ttm")]
        public string EpsBasicExclExtraTtm { get; set; }

        [JsonProperty("eps_incl_extra_annual")]
        public string EpsInclExtraAnnual { get; set; }

        [JsonProperty("eps_incl_extra_ttm")]
        public string EpsInclExtraTtm { get; set; }

        [JsonProperty("revenue_per_share_annual")]
        public string RevenuePerShareAnnual { get; set; }

        [JsonProperty("revenue_per_share_ttm")]
        public string RevenuePerShareTtm { get; set; }

        [JsonProperty("revenue_annual")]
        public string RevenueAnnual { get; set; }

        [JsonProperty("revenue_ttm")]
        public string RevenueTtm { get; set; }

        [JsonProperty("book_value_share_annual")]
        public string BookValueShareAnnual { get; set; }

        [JsonProperty("book_value_share_quarterly")]
        public string BookValueShareQuarterly { get; set; }

        [JsonProperty("tangible_book_annual")]
        public string TangibleBookAnnual { get; set; }

        [JsonProperty("tangible_book_quarterly")]
        public string TangibleBookQuarterly { get; set; }

        [JsonProperty("cash_share_annual")]
        public string CashShareAnnual { get; set; }

        [JsonProperty("cash_share_quarterly")]
        public string CashShareQuarterly { get; set; }

        [JsonProperty("cf_share_annual")]
        public string CfShareAnnual { get; set; }

        [JsonProperty("cf_share_ttm")]
        public string CfShareTtm { get; set; }

        [JsonProperty("div_share_annual")]
        public string DivShareAnnual { get; set; }

        [JsonProperty("div_share_5y")]
        public string DivShare5y { get; set; }

        [JsonProperty("div_share_ttm")]
        public string DivShareTtm { get; set; }

        [JsonProperty("ebitd_share_ttm")]
        public string EbitdShareTtm { get; set; }

        [JsonProperty("fcf_annual")]
        public string FcfAnnual { get; set; }

        [JsonProperty("fcf_ttm")]
        public string FcfTtm { get; set; }

        [JsonProperty("fcf_share_ttm")]
        public string FcfShareTtm { get; set; }

        [JsonProperty("pe_excl_extra_annual")]
        public string PeExclExtraAnnual { get; set; }

        [JsonProperty("pe_excl_extra_ttm")]
        public string PeExclExtraTtm { get; set; }

        [JsonProperty("pe_normalized_annual")]
        public string PeNormalizedAnnual { get; set; }

        [JsonProperty("ps_annual")]
        public string PsAnnual { get; set; }

        [JsonProperty("ps_ttm")]
        public string PsTtm { get; set; }

        [JsonProperty("ptbv_annual")]
        public string PtBVAnnual { get; set; }

        [JsonProperty("ptbv_quarterly")]
        public string PtBVQuarterly { get; set; }

        [JsonProperty("pfcf_share_annual")]
        public string PfcfShareAnnual { get; set; }

        [JsonProperty("pcf_share_ttm")]
        public string PcfShareTtm { get; set; }

        [JsonProperty("pb_annual")]
        public string PbAnnual { get; set; }

        [JsonProperty("pb_quarterly")]
        public string PbQuarterly { get; set; }

        [JsonProperty("pe_basic_excl_extra_ttm")]
        public string PeBasicExclExtraTtm { get; set; }

        [JsonProperty("pe_excl_extra_high_ttm")]
        public string PeExclExtraHighTtm { get; set; }

        [JsonProperty("pe_excl_low_ttm")]
        public string PeExclLowTtm { get; set; }

        [JsonProperty("pe_incl_extra_ttm")]
        public string PeInclExtraTtm { get; set; }

        [JsonProperty("net_debt_interim")]
        public string NetDebtInterim { get; set; }

        [JsonProperty("net_debt_annual")]
        public string NetDebtAnnual { get; set; }

        [JsonProperty("dividend_yield_5y")]
        public string DividendYield5y { get; set; }

        [JsonProperty("dividend_yield_indicated_annual")]
        public string DividendYieldIndicatedAnnual { get; set; }

        [JsonProperty("current_dividend_yield_ttm")]
        public string CurrentDividendYieldTtm { get; set; }

        [JsonProperty("current_ev_fcf_annual")]
        public string CurrentEvFcfAnnual { get; set; }

        [JsonProperty("current_ev_fcf_ttm")]
        public string CurrentEvFcfTtm { get; set; }

        [JsonProperty("current_ratio_annual")]
        public string CurrentRatioAnnual { get; set; }

        [JsonProperty("current_ratio_quarterly")]
        public string CurrentRatioQuarterly { get; set; }

        [JsonProperty("lt_debt_equity_annual")]
        public string LtDebtEquityAnnual { get; set; }

        [JsonProperty("lt_debt_equity_quarterly")]
        public string LtDebtEquityQuarterly { get; set; }

        [JsonProperty("payout_ratio_annual")]
        public string PayoutRatioAnnual { get; set; }

        [JsonProperty("payout_ratio_quarterly")]
        public string PayoutRatioQuarterly { get; set; }

        [JsonProperty("gross_margin_5y")]
        public string GrossMargin5y { get; set; }

        [JsonProperty("annual_gross_margin")]
        public string AnnualGrossMargin { get; set; }

        [JsonProperty("gross_margin_ttm")]
        public string GrossMarginTtm { get; set; }

        [JsonProperty("net_profit_margin_5y")]
        public string NetProfitMargin5y { get; set; }

        [JsonProperty("net_margin_growth_5y")]
        public string NetMarginGrowth5y { get; set; }

        [JsonProperty("net_profit_margin_annual")]
        public string NetProfitMarginAnnual { get; set; }

        [JsonProperty("net_profit_margin_ttm")]
        public string NetProfitMarginTtm { get; set; }

        [JsonProperty("operating_margin_5y")]
        public string OperatingMargin5y { get; set; }

        [JsonProperty("operating_margin_annual")]
        public string OperatingMarginAnnual { get; set; }

        [JsonProperty("operating_margin_ttm")]
        public string OperatingMarginTtm { get; set; }

        [JsonProperty("pretax_margin_5y")]
        public string PretaxMargin5y { get; set; }

        [JsonProperty("pretax_margin_annual")]
        public string PretaxMarginAnnual { get; set; }

        [JsonProperty("pretax_margin_ttm")]
        public string PretaxMarginTtm { get; set; }

        [JsonProperty("focf_revenue_5y")]
        public string FocfRevenue5y { get; set; }

        [JsonProperty("focf_revenue_ttm")]
        public string FocfRevenueTtm { get; set; }

        [JsonProperty("roa_rfy")]
        public string RoaRfy { get; set; }

        [JsonProperty("roe_ttm")]
        public string RoeTtm { get; set; }

        [JsonProperty("roe_rfy")]
        public string RoeRfy { get; set; }

        [JsonProperty("roae_ttm")]
        public string RoaeTtm { get; set; }

        [JsonProperty("roi_annual")]
        public string RoiAnnual { get; set; }

        [JsonProperty("roi_ttm")]
        public string RoiTtm { get; set; }

        [JsonProperty("roaa_5y")]
        public string Roaa5y { get; set; }

        [JsonProperty("roae_5y")]
        public string Roae5y { get; set; }

        [JsonProperty("roi_5y")]
        public string Roi5y { get; set; }

        [JsonProperty("asset_turnover_annual")]
        public string AssetTurnoverAnnual { get; set; }

        [JsonProperty("asset_turnover_ttm")]
        public string AssetTurnoverTtm { get; set; }

        [JsonProperty("inventory_turnover_annual")]
        public string InventoryTurnoverAnnual { get; set; }

        [JsonProperty("inventory_turnover_ttm")]
        public string InventoryTurnoverTtm { get; set; }

        [JsonProperty("net_income_employee_annual")]
        public string NetIncomeEmployeeAnnual { get; set; }

        [JsonProperty("net_income_employee_ttm")]
        public string NetIncomeEmployeeTtm { get; set; }

        [JsonProperty("receivables_turnover_annual")]
        public string ReceivablesTurnoverAnnual { get; set; }

        [JsonProperty("receivables_turnover_ttm")]
        public string ReceivablesTurnoverTtm { get; set; }

        [JsonProperty("revenue_employee_annual")]
        public string RevenueEmployeeAnnual { get; set; }

        [JsonProperty("revenue_employee_ttm")]
        public string RevenueEmployeeTtm { get; set; }

        [JsonProperty("revenue_growth_quarterly_yoy")]
        public string RevenueGrowthQuarterlyYoy { get; set; }

        [JsonProperty("revenue_growth_5y")]
        public string RevenueGrowth5y { get; set; }

        [JsonProperty("eps_growth_quarterly_yoy")]
        public string EpsGrowthQuarterlyYoy { get; set; }

        [JsonProperty("eps_growth_ttm_yoy")]
        public string EpsGrowthTtmYoy { get; set; }

        [JsonProperty("eps_growth_5y")]
        public string EpsGrowth5y { get; set; }

        [JsonProperty("eps_growth_3y")]
        public string EpsGrowth3y { get; set; }

        [JsonProperty("dividend_growth_rate_5y")]
        public string DividendGrowthRate5y { get; set; }

        [JsonProperty("revenue_growth_ttm_yoy")]
        public string RevenueGrowthTtmYoy { get; set; }

        [JsonProperty("revenue_growth_3y")]
        public string RevenueGrowth3y { get; set; }

        [JsonProperty("book_value_share_growth_5y")]
        public string BookValueShareGrowth5y { get; set; }

        [JsonProperty("tbv_cagr_5y")]
        public string TbvCagr5y { get; set; }

        [JsonProperty("capital_spending_growth_5y")]
        public string CapitalSpendingGrowth5y { get; set; }

        [JsonProperty("ebitda_cagr_5y")]
        public string EbitdaCagr5y { get; set; }

        [JsonProperty("ebitda_interim_cagr_5y")]
        public string EbitdaInterimCagr5y { get; set; }

        [JsonProperty("focf_cagr_5y")]
        public string FocfCagr5y { get; set; }

        [JsonProperty("total_debt_cagr_5y")]
        public string TotalDebtCagr5y { get; set; }

        [JsonProperty("revenue_share_growth_5y")]
        public string RevenueShareGrowth5y { get; set; }

        [JsonProperty("ebitd_annual")]
        public string EbitdAnnual { get; set; }

        [JsonProperty("ebitd_ttm")]
        public string EbitdTtm { get; set; }

        [JsonProperty("ebt_annual")]
        public string EbtAnnual { get; set; }

        [JsonProperty("ebt_ttm")]
        public string EbtTtm { get; set; }

        [JsonProperty("net_income_common_annual")]
        public string NetIncomeCommonAnnual { get; set; }

        [JsonProperty("net_income_common_ttm")]
        public string NetIncomeCommonTtm { get; set; }

        [JsonProperty("net_income_common_normalized_annual")]
        public string NetIncomeCommonNormalizedAnnual { get; set; }

        [JsonProperty("ebt_normalized_annual")]
        public string EbtNormalizedAnnual { get; set; }

        [JsonProperty("quick_ratio_annual")]
        public string QuickRatioAnnual { get; set; }

        [JsonProperty("net_interest_coverage_annual")]
        public string NetInterestCoverageAnnual { get; set; }

        [JsonProperty("net_interest_coverage_ttm")]
        public string NetInterestCoverageTtm { get; set; }

        [JsonProperty("total_debt_equity_annual")]
        public string TotalDebtEquityAnnual { get; set; }

        [JsonProperty("total_debt_equity_quarterly")]
        public string TotalDebtEquityQuarterly { get; set; }

        [JsonProperty("diluted_eps_excl_extra_ttm")]
        public string DilutedEpsExclExtraTtm { get; set; }

        [JsonProperty("originalRic")]
        public string OriginalRic { get; set; }
    }
}
