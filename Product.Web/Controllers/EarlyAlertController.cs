using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Product.Bal;
using Product.Dal;
using Product.Dal.Entities;
using Product.Web.Models.Entity;
using Product.Web.Models.PortfolioMonitor;
using Product.Web.Models.Response;


namespace Product.Web.Controllers
{
    public class EarlyAlertController : ControllerBase
    {
        private readonly DBContext _db;
        private readonly IMapper _mapper;
        private readonly UserContextService _userContextService;
        private static readonly HttpClient client = new HttpClient();
        public EarlyAlertController(DBContext db, IMapper mapper, UserContextService userContextService)
        {
            _db = db;
            _mapper = mapper;
            _userContextService = userContextService;
        }

        /// <summary>
        /// Retrieve collection of EarlyAlerts.
        /// </summary>
        /// <remarks>
        /// Authorization: SystemAdmin.
        /// </remarks>
        /// <returns></returns>
        [HttpGet]
        [Route("api/EarlyAlerts")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get()
        {
            return Ok(_db.EarlyAlerts.ToList().OrderBy(x => x.Id));
        }

        /// <summary>
        /// Retrieve collection of entities for a clientId.
        /// </summary>      
        /// <param name="clientId">clientId identifier.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/EarlyAlerts/{clientId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByClient(int clientId)
        {
            return Ok(_db.EarlyAlerts.Where(x => x.ClientId == clientId).ToList().OrderBy(x => x.Id));
        }
       

        /// <summary>
        /// Create an early alerts
        /// </summary>
        /// <remarks>
        /// Authorization: ProfileWrite.
        /// </remarks>
        /// <param name="entity">Entity details.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/EarlyAlerts/Create")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateAlerts([FromBody] EarlyAlerts alerts)
        {
            if (alerts == null)
            {
                return BadRequest(new ActionResponse { Result = 1, Message = "Request cannot be null." });
            }

            // Map to model
            var newAlert = new Product.Dal.Entities.EarlyAlert
            {
                ClientId = alerts.ClientId,
                ClientName = alerts.ClientName,
                Title = alerts.Title,
                Comments = alerts.Comments,
                Metric = alerts.Metric,
            };

            try
            {
                // Add to database
                _db.EarlyAlerts.Add(newAlert);
                await _db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.Message });
            }

            return Ok(new ActionSaveResponse
            {
                Result = 0,
                ModelId = newAlert.Id,
                ModifiedBy = newAlert.ModifiedBy,
                ModifiedDate = newAlert.ModifiedDate
            });
        }

    }
}
