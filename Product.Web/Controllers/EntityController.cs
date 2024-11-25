using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Product.Bal;
using Product.Dal;
using Product.Dal.Entities;
using Product.Web.Models.Entity;
using Product.Web.Models.Response;


namespace Product.Web.Controllers
{
    public class EntityController : ControllerBase
    {
        private readonly DBContext _db;
        private readonly IMapper _mapper;
        private readonly UserContextService _userContextService;
        private static readonly HttpClient client = new HttpClient();
        public EntityController(DBContext db, IMapper mapper, UserContextService userContextService)
        {
            _db = db;
            _mapper = mapper;
            _userContextService = userContextService;
        }

        /// <summary>
        /// Retrieve collection of entities.
        /// </summary>
        /// <remarks>
        /// Authorization: SystemAdmin.
        /// </remarks>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Entity")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get()
        {
            return Ok(_db.Entities.ToList().OrderBy(x => x.Name));
        }

        /// <summary>
        /// Retrieve collection of entities for a clientId.
        /// </summary>      
        /// <param name="clientId">clientId identifier.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Client/{clientId}/Entity")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByClient(int clientId)
        {
            return Ok(_db.Entities.Where(x => x.ClientId == clientId).ToList().OrderBy(x => x.Name));
        }

        /// <summary>
        /// Retrieve collection of entities for a clientId and name search.
        /// </summary>      
        /// <param name="clientId">clientId identifier.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Client/{clientId}/Entity/{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByEntitySearch(int clientId,string name)
        {
            var entityQuery = _db.Entities.Where(x => x.ClientId == clientId).ToList();

            if (!string.IsNullOrEmpty(name))
            {
                // Search within the user name
                entityQuery = entityQuery.Where(x => x.Name.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return Ok(entityQuery);
        }

        /// <summary>
        /// Delete an entity.
        /// </summary>   
        /// <param name="id">Entity identifier.</param>        
        /// <returns></returns>
        [HttpDelete]
        [Route("api/Entity/{id}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            // Lookup the existing entity
            var currentEntity = _db.Entities.Find(id);

            if (currentEntity == null)
            {
                return NotFound(new ActionResponse { Result = 1, Message = "Entity not found." });
            }

            try
            {
                // removed from database
                _db.Entities.Remove(currentEntity);
                await _db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.Message });
            }
            return Ok(new ActionResponse
            {
                Result = 0,
            });
        }


        /// <summary>
        /// Create an entity as a prospect (not CLU linked).
        /// </summary>
        /// <remarks>
        /// Authorization: ProfileWrite.
        /// </remarks>
        /// <param name="entity">Entity details.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Entity/Create")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateProspect([FromBody] EntityCreate entity)
        {
            if (entity == null)
            {
                return BadRequest(new ActionResponse { Result = 1, Message = "Request cannot be null." });
            }

            // if entity name exist will send error to ui
            var duplicateEntity = _db.Entities.FirstOrDefault(x => x.Name == entity.Name);
            if (duplicateEntity != null)
            {
                return StatusCode(StatusCodes.Status406NotAcceptable, new ActionResponse { Result = 1, Message = $"Entity already exist in database. Please create new entity." });
            }

            // Map to model
            var newEntity = new Product.Dal.Entities.Entity
            {
                ClientId = entity.ClientId,
                Name = entity.Name,
                Country = entity.Country,
                SourceSystemName = entity.SourceSystemName,
                SourceId = entity.SourceId,
                EntityType = entity.EntityType,
                PolicyCountry = entity.PolicyCountry,
                PolicyStatus = entity.PolicyStatus,
                IsValidCustomer = entity.IsValidCustomer,
                CCR = entity.CCR,
                SI = entity.SI,
                SourceEntityInformation = entity.SourceEntityInformation,
                Ticker = entity.Ticker,
            };

            try
            {
                // Add to database
                _db.Entities.Add(newEntity);
                await _db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.Message });
            }

            return Ok(new ActionSaveResponse
            {
                Result = 0,
                ModelId = newEntity.Id,
                ModifiedBy = newEntity.ModifiedBy,
                ModifiedDate = newEntity.ModifiedDate
            });
        }


        /// <summary>
        /// Create an entity as a prospect (not CLU linked).
        /// </summary>
        /// <remarks>
        /// Authorization: ProfileWrite.
        /// </remarks>
        /// <param name="entity">Entity details.</param>
        /// <returns></returns>
        [HttpPut]
        [Route("api/Entity/Update/{id}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateEntity(int id, [FromBody] EntityCreate entity)
        {
            if (entity == null)
            {
                return BadRequest(new ActionResponse { Result = 1, Message = "Request cannot be null." });
            }

            // if entity name exist will send error to ui
            var existingEntity = _db.Entities.Find(id);
            if (existingEntity == null)
            {
                return StatusCode(StatusCodes.Status406NotAcceptable, new ActionResponse { Result = 1, Message = $"Entity not found." });
            }

            // Map to model

            existingEntity.ClientId = entity.ClientId;
            existingEntity.Name = entity.Name;
            existingEntity.Country = entity.Country;
            existingEntity.SourceSystemName = entity.SourceSystemName;
            existingEntity.SourceId = entity.SourceId;
            existingEntity.EntityType = entity.EntityType;
            existingEntity.PolicyCountry = entity.PolicyCountry;
            existingEntity.PolicyStatus = entity.PolicyStatus;
            existingEntity.IsValidCustomer = entity.IsValidCustomer;
            existingEntity.CCR = entity.CCR;
            existingEntity.SourceEntityInformation = entity.SourceEntityInformation;
            existingEntity.Ticker = entity.Ticker;
            try
            {
                // update to database

                await _db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.Message });
            }

            return Ok(new ActionSaveResponse
            {
                Result = 0,
                ModelId = existingEntity.Id,
                ModifiedBy = existingEntity.ModifiedBy,
                ModifiedDate = existingEntity.ModifiedDate
            });
        }

        /// <summary>
        /// Search entity by name in GLEIF
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/entity/gleif")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchGLEIFEntity(string name)
        {
            if(name == null) throw new ArgumentNullException("name");

            try
            {
                string url = $"https://api.gleif.org/api/v1/lei-records?page[size]=20&page[number]=1&filter[entity.legalName]={name}";

                // Send the request and get the response
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                // Read the response content
                string responseBody = await response.Content.ReadAsStringAsync();
                Product.Web.Models.Entity.LEIRecordsResponse leiResponse = JsonConvert.DeserializeObject<Product.Web.Models.Entity.LEIRecordsResponse>(responseBody);

                return Ok(leiResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = ex.Message });                
            }
            return Ok(null);
        }

    }
}
