using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Product.Dal;
using Product.Dal.Common.Extensions;
using Product.Dal.Entities;
using Product.Dal.Enums;
using Product.Web.Models.Configuration;
using Product.Web.Models.Response;


namespace Product.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ConfigurationController : ControllerBase
{
    private readonly DBContext _db;
    private readonly IMapper _mapper;

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="db"></param>
    /// <param name="mapper"></param>
    public ConfigurationController(DBContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    [HttpGet("all")]
    public IActionResult GetAllConfiguration()
    {
        // Define the base query with common joins and projections
        var result = _db.Configurations.ToList();

        return Ok(result);
    }

    [HttpGet("{type}")]
    public IActionResult GetConfiguration(string type)
    {
        if (string.IsNullOrEmpty(type)){
            type = ConfigurationType.Threshold.ToDescription();
        }
        // Define the base query with common joins and projections
        var result = _db.Configurations.Where(x => x.ConfigType == type).ToList();

        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Add([FromBody] ConfigurationRequest configurationRequest)
    {
        if (configurationRequest == null)
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "Request is empty." });
        }

        try
        {
            var newConfig = new Configuration
            {
                ConfigType = Enum.Parse<ConfigurationType>(configurationRequest.ConfigType).ToDescription(),
                ConfigInfo = configurationRequest.ConfigInfo,
                ConfigName = configurationRequest.ConfigName,
                IsActive = true
            };

            _db.Configurations.Add(newConfig);

            await _db.SaveChangesAsync();
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.Message });
        }

        return Ok(new ActionResponse { Result = 0 });
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Edit(int id, [FromBody] ConfigurationRequest configurationRequest)
    {
        if (configurationRequest == null)
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "Request is empty." });
        }

        var existingConfig = _db.Configurations.Find(id);
        if (existingConfig == null)
        {
            return NotFound(new ActionResponse { Result = 1, Message = "Config doesn't exist." });
        }

        // Update the changed properties
        existingConfig.ConfigInfo = configurationRequest.ConfigInfo;
        existingConfig.ConfigName = configurationRequest.ConfigName;
        existingConfig.IsActive = configurationRequest.IsActive;

        _db.Entry(existingConfig).State = EntityState.Modified;
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        }

        return Ok(new ActionResponse { Result = 0 });
    }
    
    /// <summary>
    /// this will be used when we want to delete the config
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Remove(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "Id is invalid." });
        }

        var config = _db.Configurations.Find(id);
        if (config == null)
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "Member doesn't exist." });
        }

        _db.Configurations.Remove(config);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.Message });
        }

        return Ok(new ActionResponse { Result = 0 });
    }
}

