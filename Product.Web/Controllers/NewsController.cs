using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Product.Bal;
using Product.Dal;
using Product.Dal.Entities;
using Product.Integration.Interfaces;
using Product.Integration.Models.Data.Response;
using Product.Web.Models.Entity;
using Product.Web.Models.News;
using Product.Web.Models.News.Financial;
using Product.Web.Models.Response;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Reflection.PortableExecutable;
using System.Xml.Linq;


namespace Product.Web.Controllers;

[Route("api/[controller]")]
public class NewsController : ControllerBase
{
    private readonly DBContext _db;
    private readonly IMapper _mapper;
    private readonly UserContextService _userContextService;
    private static readonly HttpClient client = new HttpClient();
    private readonly IIntegrationService _integrationService;

    private static string finnhubAPIKey = "cqsmjv1r01qg43b8vtn0cqsmjv1r01qg43b8vtng";
    public NewsController(DBContext db, IMapper mapper, UserContextService userContextService, IIntegrationService integrationService)
    {
        _db = db;
        _mapper = mapper;
        _userContextService = userContextService;
        _integrationService = integrationService;
    }
    

    [HttpGet]
    [Route("allcompanynews")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AllCompanyNews(string? tickerSymbol = null)
    {
        string toDate = DateTime.Now.ToString("yyyy-MM-dd");
        string fromDate = DateTime.Now.AddYears(-1).ToString("yyyy-MM-dd");

        if(tickerSymbol == null)
        {
            tickerSymbol = "AAPL";
        }

        string url = $"https://finnhub.io/api/v1/company-news?symbol={tickerSymbol}&from={fromDate}&to={toDate}&token={finnhubAPIKey}";

        using (HttpClient client = new HttpClient())
        {
            try
            {
                // Send the GET request
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    // Read and process the response content
                    string content = await response.Content.ReadAsStringAsync();
                    List<NewsResponse> companyResponse = JsonConvert.DeserializeObject<List<NewsResponse>>(content);
                    return Ok(companyResponse);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = "Unauthorized: Check your API key" });
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = response.StatusCode.ToString() });
                }
            }
            catch (Exception ex)
            {
                // Handle any errors that occurred during the request
                return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = ex.Message });
            }

        }
    }

    [HttpGet]
    [Route("allmarketdata")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AllMarketData(string? tickerSymbol = null)
    {
        if (tickerSymbol == null)
        {
            tickerSymbol = "BHP.AX";
        }

        //string url = $"https://www.reuters.com/pf/api/v3/content/fetch/quote-by-rics-v2?query=%7B%22fields%22%3A%22ric%2Ctype%3AricType%2Cname%2Ccurrency%2ClocalName%7Blong%7Bname%2Clang%7D%2Cshort%7Bname%2Clang%7D%7Dprice_closing_or_last_bid%3ApriceClosingOrLastBid%2Cprice_date%3ApriceDate%2Cfiftytwo_wk_high%3AfiftyTwoWeekHigh%2Cfiftytwo_week_high_date%3AfiftytwoWeekHighDate%2Cfiftytwo_wk_low%3AfiftyTwoWeekLow%2Cfiftytwo_week_low_date%3AfiftytwoWeekLowDate%2Cshare_volume_10d%3AshareVolume10D%2Cshare_volume_3m%3AshareVolume3M%2Cmarket_cap%3AmarketCap%2Cbeta%3Abeta%2Cone_day_price%3AoneDayPrice%2Cfive_day_price_return%3AfiveDayPriceReturn%2Cthirteen_wk_price_return%3AthirteenWkPriceReturn%2Ctwenty_six_wk_price_return%3AtwentySixWkPriceReturn%2Cfiftytwo_wk_price_return%3AfiftytwoWkPriceReturn%2Cmtd_price_return%3AmtdPriceReturn%2Cytd_price_return%3AytdPriceReturn%2Cfour_week_rel_sandp500%3AfourWeekRelSandp500%2Cthirteen_week_rel_sandp500%3AthirteenWeekRelSandp500%2Ctwentysix_week_rel_sandp500%3AtwentysixWeekRelSandp500%2Cfiftytwo_week_rel_sandp500%3AfiftytwoWeekRelSandp500%2Cytd_rel_sandp500%3AytdRelSandp500%2Ceps_excl_extra_ttm%3AepsExclExtraTtm%2Ceps_excl_extra_annual%3AepsExclExtraAnnual%2Ceps_normalized_annual%3AepsNormalizedAnnual%2Ceps_basic_excl_extra_annual%3AepsBasicExclExtraAnnual%2Ceps_basic_excl_extra_ttm%3AepsBasicExclExtraTtm%2Ceps_incl_extra_annual%3AepsInclExtraAnnual%2Ceps_incl_extra_ttm%3AepsInclExtraTtm%2Ceps_basic_excl_extra_ttm%3AepsBasicExclExtraTtm%2Crevenue_per_share_annual%3ArevenueShareFy%2Crevenue_per_share_ttm%3ArevenueShareTtm%2Crevenue_annual%3ArevenueAnnual%2Crevenue_ttm%3ArevenueTtm%2Cbook_value_share_annual%3AbookValueShareAnnual%2Cbook_value_share_quarterly%3AbookValueShareQuarterly%2Ctangible_book_annual%3AtangibleBookAnnual%2Ctangible_book_quarterly%3AtangibleBookQuarterly%2Ccash_share_annual%3AcashShareAnnual%2Ccash_share_quarterly%3AcashShareQuarterly%2Ccf_share_annual%3AcfShareAnnual%2Ccf_share_ttm%3AcfShareTtm%2Cdiv_share_annual%3AdivShareAnnual%2Cdiv_share_5y%3AdivShare5Y%2Cdiv_share_ttm%3AdivShareTtm%2Cebitd_share_ttm%3AebitdShareTtm%2Cfcf_annual%3AfcfAnnual%2Cfcf_ttm%3AfcfTtm%2Cfcf_share_ttm%3AfcfShareTtm%2Cpe_excl_extra_annual%3ApeExclExtraAnnual%2Cpe_excl_extra_ttm%3ApeExclExtraTtm%2Cpe_normalized_annual%3ApeNormalizedAnnual%2Cps_annual%3ApsAnnual%2Cps_ttm%3ApsTtm%2Cptbv_annual%3AptbvAnnual%2Cptbv_quarterly%3AptbvQuarterly%2Cpfcf_share_annual%3ApfcfShareAnnual%2Cpcf_share_ttm%3ApcfShareTtm%2Cpfcf_share_ttm%3ApfcfShareTtm%2Cpb_annual%3ApbAnnual%2Cpb_quarterly%3ApbQuarterly%2Cpe_basic_excl_extra_ttm%3ApeBasicExclExtraTtm%2Cpe_excl_extra_high_ttm%3ApeExclExtraHighTtm%2Cpe_excl_low_ttm%3ApeExclLowTtm%2Cpe_incl_extra_ttm%3ApeInclExtraTtm%2Cnet_debt_interim%3AnetDebtInterim%2Cnet_debt_annual%3AnetDebtAnnual%2Cdividend_yield_5y%3AdividendYield5Y%2Cdividend_yield_indicated_annual%3AdividendYieldIndicatedAnnual%2Ccurrent_dividend_yield_ttm%3AcurrentDividendYieldTtm%2Ccurrent_ev_fcf_annual%3AcurrentEvFcfAnnual%2Ccurrent_ev_fcf_ttm%3AcurrentEvFcfTtm%2Ccurrent_ratio_annual%3AcurrentRatioAnnual%2Ccurrent_ratio_quarterly%3AcurrentRatioQuarterly%2Clt_debt_equity_annual%3AltDebtEquityAnnual%2Clt_debt_equity_quarterly%3AltDebtEquityQuarterly%2Cpayout_ratio_annual%3ApayoutRatioAnnual%2Cpayout_ratio_quarterly%3ApayoutRatioTtm%2Cgross_margin_5y%3AgrossMargin5Y%2Cannual_gross_margin%3AannualGrossMargin%2Cgross_margin_ttm%3AgrossMarginTtm%2Cnet_profit_margin_5y%3AnetProfitMargin5Y%2Cnet_margin_growth_5y%3AnetMarginGrowth5Y%2Cnet_profit_margin_annual%3AnetProfitMarginAnnual%2Cnet_profit_margin_ttm%3AnetProfitMarginTtm%2Coperating_margin_5y%3AoperatingMargin5Y%2Coperating_margin_annual%3AoperatingMarginAnnual%2Coperating_margin_ttm%3AoperatingMarginTtm%2Cpretax_margin_5y%3ApretaxMargin5Y%2Cpretax_margin_annual%3ApretaxMarginAnnual%2Cpretax_margin_ttm%3ApretaxMarginTtm%2Cfocf_revenue_5y%3AfocfRevenue5Y%2Cfocf_revenue_ttm%3AfocfRevenueTtm%2Croa_rfy%3AroaRfy%2Croe_ttm%3AroeTtm%2Croe_rfy%3AroeRfy%2Croae_ttm%3AroaeTtm%2Croi_annual%3AroiAnnual%2Croi_ttm%3AroiTtm%2Croaa_5y%3Aroaa5Y%2Croae_5y%3Aroae5Y%2Croi_5y%3Aroi5Y%2Casset_turnover_annual%3AassetTurnoverAnnual%2Casset_turnover_ttm%3AassetTurnoverTtm%2Cinventory_turnover_annual%3AinventoryTurnoverAnnual%2Cinventory_turnover_ttm%3AinventoryTurnoverTtm%2Cnet_income_employee_annual%3AnetIncomeEmployeeAnnual%2Cnet_income_employee_ttm%3AnetIncomeEmployeeTtm%2Creceivables_turnover_annual%3AreceivablesTurnoverAnnual%2Creceivables_turnover_ttm%3AreceivablesTurnoverTtm%2Crevenue_employee_annual%3ArevenueEmployeeAnnual%2Crevenue_employee_ttm%3ArevenueEmployeeTtm%2Crevenue_growth_quarterly_yoy%3ArevenueGrowthQuarterlyYoy%2Crevenue_growth_5y%3ArevenueGrowth5Y%2Ceps_growth_quarterly_yoy%3AepsGrowthQuarterlyYoy%2Ceps_growth_ttm_yoy%3AepsGrowthTtmYoy%2Ceps_growth_5y%3AepsGrowth5Y%2Ceps_growth_3y%3AepsGrowth3Y%2Cdividend_growth_rate_5y%3AdividendGrowthRate5Y%2Crevenue_growth_ttm_yoy%3ArevenueGrowthTtmYoy%2Crevenue_growth_5y%3ArevenueGrowth5Y%2Crevenue_growth_3y%3ArevenueGrowth3Y%2Cbook_value_share_growth_5y%3AbookValueShareGrowth5Y%2Ctbv_cagr_5y%3AtbvCagr5Y%2Ccapital_spending_growth_5y%3AcapitalSpendingGrowth5Y%2Cebitda_cagr_5y%3AebitdaCagr5Y%2Cebitda_interim_cagr_5y%3AebitdaInterimCagr5Y%2Cfocf_cagr_5y%3AfocfCagr5Y%2Ctotal_debt_cagr_5y%3AtotalDebtCagr5Y%2Crevenue_employee_annual%3ArevenueEmployeeAnnual%2Crevenue_employee_ttm%3ArevenueEmployeeTtm%2Crevenue_share_growth_5y%3ArevenueShareGrowth5Y%2Crevenue_annual%3ArevenueAnnual%2Crevenue_ttm%3ArevenueTtm%2Cebitd_annual%3AebitdAnnual%2Cebitd_ttm%3AebitdTtm%2Cebt_annual%3AebtAnnual%2Cebt_ttm%3AebtTtm%2Cnet_income_common_annual%3AnetIncomeCommonAnnual%2Cnet_income_common_ttm%3AnetIncomeCommonTtm%2Cnet_income_common_normalized_annual%3AnetIncomeCommonNormalizedAnnual%2Cebt_normalized_annual%3AebtNormalizedAnnual%2Cquick_ratio_annual%3AquickRatioAnnual%2Cnet_interest_coverage_annual%3AnetInterestCoverageAnnual%2Cnet_interest_coverage_ttm%3AnetInterestCoverageTtm%2Ctotal_debt_equity_annual%3AtotalDebtEquityAnnual%2Ctotal_debt_equity_quarterly%3AtotalDebtEquityQuarterly%2Cdiluted_eps_excl_extra_ttm%3AdilutedEpsExclExtraTtm%22%2C%22retries%22%3A4%2C%22rics%22%3A%22{tickerSymbol}%22%7D&d=216&_website=reuters";

        // alternate 1
         //string url = $"https://www.reuters.com/pf/api/v3/content/fetch/quote-by-rics-v2?query=%7B%22fields%22%3A%22ric%2Ctype%3AricType%2Cname%2Ccurrency%2ClocalName%7Blong%7Bname%2Clang%7D%2Cshort%7Bname%2Clang%7D%7Dprice_closing_or_last_bid%3ApriceClosingOrLastBid%2Cprice_date%3ApriceDate%2Cfiftytwo_wk_high%3AfiftyTwoWeekHigh%2Cfiftytwo_week_high_date%3AfiftytwoWeekHighDate%2Cfiftytwo_wk_low%3AfiftyTwoWeekLow%2Cfiftytwo_week_low_date%3AfiftytwoWeekLowDate%2Cshare_volume_10d%3AshareVolume10D%2Cshare_volume_3m%3AshareVolume3M%2Cmarket_cap%3AmarketCap%2Cbeta%3Abeta%2Cone_day_price%3AoneDayPrice%2Cfive_day_price_return%3AfiveDayPriceReturn%2Cthirteen_wk_price_return%3AthirteenWkPriceReturn%2Ctwenty_six_wk_price_return%3AtwentySixWkPriceReturn%2Cfiftytwo_wk_price_return%3AfiftytwoWkPriceReturn%2Cmtd_price_return%3AmtdPriceReturn%2Cytd_price_return%3AytdPriceReturn%2Cfour_week_rel_sandp500%3AfourWeekRelSandp500%2Cthirteen_week_rel_sandp500%3AthirteenWeekRelSandp500%2Ctwentysix_week_rel_sandp500%3AtwentysixWeekRelSandp500%2Cfiftytwo_week_rel_sandp500%3AfiftytwoWeekRelSandp500%2Cytd_rel_sandp500%3AytdRelSandp500%2Ceps_excl_extra_ttm%3AepsExclExtraTtm%2Ceps_excl_extra_annual%3AepsExclExtraAnnual%2Ceps_normalized_annual%3AepsNormalizedAnnual%2Ceps_basic_excl_extra_annual%3AepsBasicExclExtraAnnual%2Ceps_basic_excl_extra_ttm%3AepsBasicExclExtraTtm%2Ceps_incl_extra_annual%3AepsInclExtraAnnual%2Ceps_incl_extra_ttm%3AepsInclExtraTtm%2Ceps_basic_excl_extra_ttm%3AepsBasicExclExtraTtm%2Crevenue_per_share_annual%3ArevenueShareFy%2Crevenue_per_share_ttm%3ArevenueShareTtm%2Crevenue_annual%3ArevenueAnnual%2Crevenue_ttm%3ArevenueTtm%2Cbook_value_share_annual%3AbookValueShareAnnual%2Cbook_value_share_quarterly%3AbookValueShareQuarterly%2Ctangible_book_annual%3AtangibleBookAnnual%2Ctangible_book_quarterly%3AtangibleBookQuarterly%2Ccash_share_annual%3AcashShareAnnual%2Ccash_share_quarterly%3AcashShareQuarterly%2Ccf_share_annual%3AcfShareAnnual%2Ccf_share_ttm%3AcfShareTtm%2Cdiv_share_annual%3AdivShareAnnual%2Cdiv_share_5y%3AdivShare5Y%2Cdiv_share_ttm%3AdivShareTtm%2Cebitd_share_ttm%3AebitdShareTtm%2Cfcf_annual%3AfcfAnnual%2Cfcf_ttm%3AfcfTtm%2Cfcf_share_ttm%3AfcfShareTtm%2Cpe_excl_extra_annual%3ApeExclExtraAnnual%2Cpe_excl_extra_ttm%3ApeExclExtraTtm%2Cpe_normalized_annual%3ApeNormalizedAnnual%2Cps_annual%3ApsAnnual%2Cps_ttm%3ApsTtm%2Cptbv_annual%3AptbvAnnual%2Cptbv_quarterly%3AptbvQuarterly%2Cpfcf_share_annual%3ApfcfShareAnnual%2Cpcf_share_ttm%3ApcfShareTtm%2Cpfcf_share_ttm%3ApfcfShareTtm%2Cpb_annual%3ApbAnnual%2Cpb_quarterly%3ApbQuarterly%2Cpe_basic_excl_extra_ttm%3ApeBasicExclExtraTtm%2Cpe_excl_extra_high_ttm%3ApeExclExtraHighTtm%2Cpe_excl_low_ttm%3ApeExclLowTtm%2Cpe_incl_extra_ttm%3ApeInclExtraTtm%2Cnet_debt_interim%3AnetDebtInterim%2Cnet_debt_annual%3AnetDebtAnnual%2Cdividend_yield_5y%3AdividendYield5Y%2Cdividend_yield_indicated_annual%3AdividendYieldIndicatedAnnual%2Ccurrent_dividend_yield_ttm%3AcurrentDividendYieldTtm%2Ccurrent_ev_fcf_annual%3AcurrentEvFcfAnnual%2Ccurrent_ev_fcf_ttm%3AcurrentEvFcfTtm%2Ccurrent_ratio_annual%3AcurrentRatioAnnual%2Ccurrent_ratio_quarterly%3AcurrentRatioQuarterly%2Clt_debt_equity_annual%3AltDebtEquityAnnual%2Clt_debt_equity_quarterly%3AltDebtEquityQuarterly%2Cpayout_ratio_annual%3ApayoutRatioAnnual%2Cpayout_ratio_quarterly%3ApayoutRatioTtm%2Cgross_margin_5y%3AgrossMargin5Y%2Cannual_gross_margin%3AannualGrossMargin%2Cgross_margin_ttm%3AgrossMarginTtm%2Cnet_profit_margin_5y%3AnetProfitMargin5Y%2Cnet_margin_growth_5y%3AnetMarginGrowth5Y%2Cnet_profit_margin_annual%3AnetProfitMarginAnnual%2Cnet_profit_margin_ttm%3AnetProfitMarginTtm%2Coperating_margin_5y%3AoperatingMargin5Y%2Coperating_margin_annual%3AoperatingMarginAnnual%2Coperating_margin_ttm%3AoperatingMarginTtm%2Cpretax_margin_5y%3ApretaxMargin5Y%2Cpretax_margin_annual%3ApretaxMarginAnnual%2Cpretax_margin_ttm%3ApretaxMarginTtm%2Cfocf_revenue_5y%3AfocfRevenue5Y%2Cfocf_revenue_ttm%3AfocfRevenueTtm%2Croa_rfy%3AroaRfy%2Croe_ttm%3AroeTtm%2Croe_rfy%3AroeRfy%2Croae_ttm%3AroaeTtm%2Croi_annual%3AroiAnnual%2Croi_ttm%3AroiTtm%2Croaa_5y%3Aroaa5Y%2Croae_5y%3Aroae5Y%2Croi_5y%3Aroi5Y%2Casset_turnover_annual%3AassetTurnoverAnnual%2Casset_turnover_ttm%3AassetTurnoverTtm%2Cinventory_turnover_annual%3AinventoryTurnoverAnnual%2Cinventory_turnover_ttm%3AinventoryTurnoverTtm%2Cnet_income_employee_annual%3AnetIncomeEmployeeAnnual%2Cnet_income_employee_ttm%3AnetIncomeEmployeeTtm%2Creceivables_turnover_annual%3AreceivablesTurnoverAnnual%2Creceivables_turnover_ttm%3AreceivablesTurnoverTtm%2Crevenue_employee_annual%3ArevenueEmployeeAnnual%2Crevenue_employee_ttm%3ArevenueEmployeeTtm%2Crevenue_growth_quarterly_yoy%3ArevenueGrowthQuarterlyYoy%2Crevenue_growth_5y%3ArevenueGrowth5Y%2Ceps_growth_quarterly_yoy%3AepsGrowthQuarterlyYoy%2Ceps_growth_ttm_yoy%3AepsGrowthTtmYoy%2Ceps_growth_5y%3AepsGrowth5Y%2Ceps_growth_3y%3AepsGrowth3Y%2Cdividend_growth_rate_5y%3AdividendGrowthRate5Y%2Crevenue_growth_ttm_yoy%3ArevenueGrowthTtmYoy%2Crevenue_growth_5y%3ArevenueGrowth5Y%2Crevenue_growth_3y%3ArevenueGrowth3Y%2Cbook_value_share_growth_5y%3AbookValueShareGrowth5Y%2Ctbv_cagr_5y%3AtbvCagr5Y%2Ccapital_spending_growth_5y%3AcapitalSpendingGrowth5Y%2Cebitda_cagr_5y%3AebitdaCagr5Y%2Cebitda_interim_cagr_5y%3AebitdaInterimCagr5Y%2Cfocf_cagr_5y%3AfocfCagr5Y%2Ctotal_debt_cagr_5y%3AtotalDebtCagr5Y%2Crevenue_employee_annual%3ArevenueEmployeeAnnual%2Crevenue_employee_ttm%3ArevenueEmployeeTtm%2Crevenue_share_growth_5y%3ArevenueShareGrowth5Y%2Crevenue_annual%3ArevenueAnnual%2Crevenue_ttm%3ArevenueTtm%2Cebitd_annual%3AebitdAnnual%2Cebitd_ttm%3AebitdTtm%2Cebt_annual%3AebtAnnual%2Cebt_ttm%3AebtTtm%2Cnet_income_common_annual%3AnetIncomeCommonAnnual%2Cnet_income_common_ttm%3AnetIncomeCommonTtm%2Cnet_income_common_normalized_annual%3AnetIncomeCommonNormalizedAnnual%2Cebt_normalized_annual%3AebtNormalizedAnnual%2Cquick_ratio_annual%3AquickRatioAnnual%2Cnet_interest_coverage_annual%3AnetInterestCoverageAnnual%2Cnet_interest_coverage_ttm%3AnetInterestCoverageTtm%2Ctotal_debt_equity_annual%3AtotalDebtEquityAnnual%2Ctotal_debt_equity_quarterly%3AtotalDebtEquityQuarterly%2Cdiluted_eps_excl_extra_ttm%3AdilutedEpsExclExtraTtm%22%2C%22retries%22%3A2%2C%22rics%22%3A%22{tickerSymbol}%22%7D&d=213&_website=reuters";

        // alternate 2
        string url = $"https://www.reuters.com/pf/api/v3/content/fetch/quote-by-rics-v2?query=%7B%22fields%22%3A%22ric%2Ctype%3AricType%2Cname%2Ccurrency%2ClocalName%7Blong%7Bname%2Clang%7D%2Cshort%7Bname%2Clang%7D%7Dfundamental_exchange_name%3AfundamentalExchangeName%2Ccompany_name%3AcompanyName%2Cexchange%2Clast%2Cnet_change%3AnetChange%2Cpercent_change%3ApctChange%2Ctime%3AupdatedTimeStamp%2Ccurrency%2Copen%2Cprev_day_close%3ApreviousDayClose%2Cvolume%2Cday_high%3Ahigh%2Cday_low%3Alow%2Cfiftytwo_wk_high%3AfiftyTwoWeekHigh%2Cfiftytwo_wk_low%3AfiftyTwoWeekLow%2Cmarket_cap%3AmarketCap%2Cshare_volume_3m%3AshareVolume3M%2Cbeta%2Ceps_excl_extra_ttm%3AepsBasicExclExtraTtm%2Cpe_excl_extra_ttm%3ApeBasicExclExtraTtm%2Cps_annual%3ApsAnnual%2Cps_ttm%3ApsTtm%2Cpcf_share_ttm%3ApcfShareTtm%2Cpb_annual%3ApbAnnual%2Cpb_quarterly%3ApbQuarterly%2Cdividend_yield_indicated_annual%3AdividendYieldIndicatedAnnual%2Clt_debt_equity_annual%3AltDebtEquityAnnual%2Ctotal_debt_equity_annual%3AtotalDebtEquityAnnual%2Clt_debt_equity_quarterly%3AltDebtEquityQuarterly%2Ctotal_debt_equity_quarterly%3AtotalDebtEquityQuarterly%2Cshares_out%3AsharesOut%2Croe_ttm%3AroeTtm%2Croi_ttm%3AroiTtm%2Csig_devs%3AsigDevs%7Bdevelopment_id%3AdevelopmentId%2Clast_update%3AlastUpdate%2Cheadline%2Cdescription%7D%2Cabout%2Cabout_jp%3AaboutJp%2Cwebsite%2Cstreet_address%3AstreetAddress%2Ccity%2Cstate%2Cpostal_code%3ApostalCode%2Ccountry%2Cphone%7Btype%2Ccountry_phone_code%3AcountryPhoneCode%2Ccity_area_code%3AcityAreaCode%2Cnumber%7D%2Csector%2Cindustry%2Cforward_PE%3AforwardPe%2Cofficers%7Bage%2Cname%2Crank%2Ctitle%2Csince%7D%2Crecommendation%7Bunverified_mean%3AunverifiedMean%2Cpreliminary_mean%3ApreliminaryMean%2Cmean%2Chigh%2Clow%2Cnumber_of_recommendations%3AnumberOfRecommendations%7D%2Ceps_per_year%3AepsPerYear%7Bcurrency%2Cdata%7Bestimate%2Cfiscal_year%3AfiscalYear%2Cvalue%7D%7D%2Crevenue_per_year%3ArevenuePerYear%7Bcurrency%2Cdata%7Bestimate%2Cfiscal_year%3AfiscalYear%2Cvalue%7D%7D%2Cnext_event%3AnextEvent%7Bname%2Ctime%7D%22%2C%22retries%22%3A1%2C%22rics%22%3A%22{tickerSymbol.Trim(',')}%22%7D&d=221&_website=reuters";


        using (HttpClient client = new HttpClient())
        {
            try
            {
                // Send the GET request
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    // Read and process the response content
                    string content = await response.Content.ReadAsStringAsync();
                    FinancialResponse finResponse = JsonConvert.DeserializeObject<FinancialResponse>(content);
                    return Ok(finResponse);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = "Unauthorized: Check your API key" });
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = response.StatusCode.ToString() });
                }
            }
            catch (Exception ex)
            {
                // Handle any errors that occurred during the request
                return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = ex.Message });
            }

        }
    }


    [HttpGet]
    [Route("allfinancialdata")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AllFinancialData(string? tickerSymbol = null)
    {
        if (tickerSymbol == null)
        {
            tickerSymbol = "BHP.AX"; // BHP.AX
        }

       
        //string url = $"https://www.reuters.com/pf/api/v3/content/fetch/quote-by-rics-v2?query=%7B%22fields%22%3A%22ric%2Ctype%3AricType%2Cname%2Ccurrency%2ClocalName%7Blong%7Bname%2Clang%7D%2Cshort%7Bname%2Clang%7D%7DfinancialStatements%7Bincome%7Bannual%7Brevenue%7Blabel%2Citems%7Bdate%2Cvalue%7D%7D%2CnetIncome%7Blabel%2Citems%7Bdate%2Cvalue%7D%7D%2CgrossProfit%7Blabel%2Citems%7Bdate%2Cvalue%7D%7D%7D%2Cinterim%7Brevenue%7Blabel%2Citems%7Bdate%2Cvalue%7D%7D%2CnetIncome%7Blabel%2Citems%7Bdate%2Cvalue%7D%7D%2CgrossProfit%7Blabel%2Citems%7Bdate%2Cvalue%7D%7D%7D%7D%2CbalanceSheet%7Bannual%7BtotalAssets%7Blabel%2Citems%7Bdate%2Cvalue%7D%7D%2CtotalLiabilities%7Blabel%2Citems%7Bdate%2Cvalue%7D%7D%2CtotalDebt%7Blabel%2Citems%7Bdate%2Cvalue%7D%7D%7D%2Cinterim%7BtotalAssets%7Blabel%2Citems%7Bdate%2Cvalue%7D%7D%2CtotalLiabilities%7Blabel%2Citems%7Bdate%2Cvalue%7D%7D%2CtotalDebt%7Blabel%2Citems%7Bdate%2Cvalue%7D%7D%7D%7D%2CcashFlow%7Bannual%7BcashFromOperatingActivities%7Blabel%2Citems%7Bdate%2Cvalue%7D%7D%2CcashFromInvestingActivities%7Blabel%2Citems%7Bdate%2Cvalue%7D%7D%2CfinancingCashFlowItems%7Blabel%2Citems%7Bdate%2Cvalue%7D%7D%7D%2Cinterim%7BcashFromOperatingActivities%7Blabel%2Citems%7Bdate%2Cvalue%7D%7D%2CcashFromInvestingActivities%7Blabel%2Citems%7Bdate%2Cvalue%7D%7D%2CfinancingCashFlowItems%7Blabel%2Citems%7Bdate%2Cvalue%7D%7D%7D%7D%7D%2Ccurrency%22%2C%22retries%22%3A3%2C%22rics%22%3A%22{tickerSymbol}%22%7D&d=216&_website=reuters";
        // alternate
        //string url = $"https://www.reuters.com/pf/api/v3/content/fetch/quote-by-rics-v2?query=%7B%22fields%22%3A%22ric%2Ctype%3AricType%2Cname%2Ccurrency%2ClocalName%7Blong%7Bname%2Clang%7D%2Cshort%7Bname%2Clang%7D%7DfinancialStatements%7Bincome%7Bannual%7B...IncomeStatement%7D%2Cinterim%7B...IncomeStatement%7D%7D%2Cbalance_sheet%3AbalanceSheet%7Bannual%7B...BalanceSheetStatement%7D%2Cinterim%7B...BalanceSheetStatement%7D%7D%2Ccash_flow%3AcashFlow%7Bannual%7B...CashFlowStatement%7D%2Cinterim%7B...CashFlowStatement%7D%7D%7D%22%2C%22retries%22%3A1%2C%22rics%22%3A%22{tickerSymbol}%22%7D&d=213&_website=reuters";

        // alternate2
        string url = $"https://www.reuters.com/pf/api/v3/content/fetch/quote-by-rics-v2?query=%7B%22fields%22%3A%22ric%2Ctype%3AricType%2Cname%2Ccurrency%2ClocalName%7Blong%7Bname%2Clang%7D%2Cshort%7Bname%2Clang%7D%7DfinancialStatements%7Bincome%7Bannual%7Brevenue%7Blabel%2Citems%7Bdate%2Cvalue%7D%7D%2CnetIncome%7Blabel%2Citems%7Bdate%2Cvalue%7D%7D%2CgrossProfit%7Blabel%2Citems%7Bdate%2Cvalue%7D%7D%7D%2Cinterim%7Brevenue%7Blabel%2Citems%7Bdate%2Cvalue%7D%7D%2CnetIncome%7Blabel%2Citems%7Bdate%2Cvalue%7D%7D%2CgrossProfit%7Blabel%2Citems%7Bdate%2Cvalue%7D%7D%7D%7D%2CbalanceSheet%7Bannual%7BtotalAssets%7Blabel%2Citems%7Bdate%2Cvalue%7D%7D%2CtotalLiabilities%7Blabel%2Citems%7Bdate%2Cvalue%7D%7D%2CtotalDebt%7Blabel%2Citems%7Bdate%2Cvalue%7D%7D%7D%2Cinterim%7BtotalAssets%7Blabel%2Citems%7Bdate%2Cvalue%7D%7D%2CtotalLiabilities%7Blabel%2Citems%7Bdate%2Cvalue%7D%7D%2CtotalDebt%7Blabel%2Citems%7Bdate%2Cvalue%7D%7D%7D%7D%2CcashFlow%7Bannual%7BcashFromOperatingActivities%7Blabel%2Citems%7Bdate%2Cvalue%7D%7D%2CcashFromInvestingActivities%7Blabel%2Citems%7Bdate%2Cvalue%7D%7D%2CfinancingCashFlowItems%7Blabel%2Citems%7Bdate%2Cvalue%7D%7D%7D%2Cinterim%7BcashFromOperatingActivities%7Blabel%2Citems%7Bdate%2Cvalue%7D%7D%2CcashFromInvestingActivities%7Blabel%2Citems%7Bdate%2Cvalue%7D%7D%2CfinancingCashFlowItems%7Blabel%2Citems%7Bdate%2Cvalue%7D%7D%7D%7D%7D%2Ccurrency%22%2C%22retries%22%3A1%2C%22rics%22%3A%22{tickerSymbol}%22%7D&d=221&_website=reuters";


        using (HttpClient client = new HttpClient())
        {
            try
            {
                //client.Timeout = TimeSpan.FromMinutes(1);
                // Send the GET request
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    // Read and process the response content
                    string content = await response.Content.ReadAsStringAsync();
                    CompanyFinancialStatements finResponse = JsonConvert.DeserializeObject<CompanyFinancialStatements>(content);
                    return Ok(finResponse);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = "Unauthorized: Check your API key" });
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = response.StatusCode.ToString() });
                }
            }
            catch (Exception ex)
            {
                // Handle any errors that occurred during the request
                return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = ex.Message });
            }

        }
    }

    //[HttpGet]
    //[Route("test")]
    //[ProducesResponseType(StatusCodes.Status200OK)]
    //[ProducesResponseType(StatusCodes.Status403Forbidden)]
    //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
    //public async Task<IActionResult> Test(string? tickerSymbol = null)
    //{
    //    string toDate = DateTime.Now.ToString("yyyy-MM-dd");
    //    string fromDate = DateTime.Now.AddYears(-1).ToString("yyyy-MM-dd");

    //    if (tickerSymbol == null)
    //    {
    //        tickerSymbol = "AAPL";
    //    }

    //    //var response = await _integrationService.ExecuteGetRequestAsync<BloombergResponse>("https://api.bloomberg.com/eap/catalogs/54357/universes/");
    //    //var response = await _integrationService.ExecuteGetRequestAsync<BloombergResponse>("https://api.bloomberg.com/eap/catalogs/54357/content/responses/");
    //    var response = await _integrationService.ExecuteGetRequestAsync<BloombergResponse>("https://api.bloomberg.com/blp/mktnews-content/news/eid/54357?format=xml");

    //    //string url = $"https://finnhub.io/api/v1/company-news?symbol={tickerSymbol}&from={fromDate}&to={toDate}&token={finnhubAPIKey}";

    //    return Ok(response);
    //}
}

