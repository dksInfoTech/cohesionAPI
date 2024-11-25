using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Product.Bal;
using Product.Dal;
using Product.Integration.Models.Data.Pub;
using System.Security.Policy;
using System.Text.Json;

namespace Product.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IntegrationController : ControllerBase
    {
        private readonly DBContext _db;
        private readonly IMapper _mapper;
        private readonly UserContextService _userContextService;
        private static readonly HttpClient client = new HttpClient();
        public IntegrationController(DBContext db, IMapper mapper, UserContextService userContextService)
        {
            _db = db;
            _mapper = mapper;
            _userContextService = userContextService;
        }

        /// <summary>
        /// Retrieve collection of Rating.
        /// </summary>
        /// <remarks>
        /// Authorization: SystemAdmin.
        /// </remarks>
        /// <returns></returns>        
        [HttpGet("{ticker}/GetRatings")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRatings(string ticker)
        {
            return Ok(_db.SourceRating
                 .Where(x => x.Ticker == getTickerSymbol(ticker))
                 .ToList());
        }

        /// <summary>
        /// Retrieve collection of Entity Info.
        /// </summary>
        /// <remarks>
        /// Authorization: SystemAdmin.
        /// </remarks>
        /// <returns></returns>        
        [HttpGet("{ticker}/GetEntityInfo")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetEntityInfo(string ticker)
        {
            return Ok(_db.SourceEntity
                 .Where(x => x.Ticker == getTickerSymbol(ticker))
                 .ToList());
        }

        /// <summary>
        /// Retrieve collection of Financial Info
        /// </summary>
        /// <remarks>
        /// Authorization: SystemAdmin.
        /// </remarks>
        /// <returns></returns>        
        [HttpGet("{ticker}/GetFinancialInfo")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFinancialInfo(string ticker)
        {
            var result = from sf in _db.SourceFinancial
                         join sfc in _db.SourceFinancialCode on sf.FinCode equals sfc.FinCode
                         where sf.Ticker == getTickerSymbol(ticker)
                         select new
                         {
                             sf.Ticker,
                             sf.EqyFundYear,
                             sf.FinCode,
                             sf.Currency,
                             sf.Scaling,
                             sf.FinValue,
                             sfc.Title
                         };

            return Ok(result.ToList());
        }


        private string getTickerSymbol(string ticker)
        {
            //return ticker.Substring(0, ticker.IndexOf(' '));
            int spaceIndex = ticker.IndexOf(' ');
            if (spaceIndex != -1)
            {
                return ticker.Substring(0, spaceIndex);
            }
            else
            {
                // Handle the case where there's no space (return the whole string or any default behavior)
                return ticker;
            }
        }



    }

    
}
