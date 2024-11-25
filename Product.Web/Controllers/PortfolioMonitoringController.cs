using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Product.Bal;
using Product.Bal.Interfaces;
using Product.Bal.Models;
using Product.Dal;
using Product.Dal.Common.Extensions;
using Product.Dal.Common.Workflow;
using Product.Dal.Entities;
using Product.Dal.Enums;
using Product.Web.Models.Client;
using Product.Web.Models.PortfolioMonitor;
using Product.Web.Models.Response;

namespace Product.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PortfolioMonitoringController : ControllerBase
{
    private readonly DBContext _db;
    private readonly IMapper _mapper;
    private readonly UserContextService _userContextService;

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="db"></param>
    /// <param name="mapper"></param>
    /// <param name="userContextService"></param>
    public PortfolioMonitoringController(DBContext db, IMapper mapper, UserContextService userContextService)
    {
        _db = db;
        _mapper = mapper;
        _userContextService = userContextService;
    }

    [HttpGet("PortfolioFilters")]
    public IActionResult GetPortfolioFilters()
    {
        // Define the base query with common joins and projections
        var result = _db.PortfolioMonitors.Select(x=> new
                        {
                            x.Id,
                            x.ClientIds,
                            x.FilterId,
                            x.Description,
                            x.Title,
                            x.MonitorType,
                        }).ToList();

        return Ok(result);
    }

    [HttpGet("{filterId}/PortfolioFilters")]
    public IActionResult GetPortfolioFiltersByFilterId(int filterId)
    {
        // Define the base query with common joins and projections
        var result = _db.PortfolioMonitors.Where(y => y.FilterId == filterId).Select(x => new
        {
                            x.Id,
                            x.ClientIds,
                            x.FilterId,
                            x.Description,
                            x.Title,
                            x.MonitorType,
                        }).ToList();

        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> AddPortfolioFilter([FromBody] PortfolioFilterRequest portfolioFilter)
    {
        if (portfolioFilter == null || string.IsNullOrWhiteSpace(portfolioFilter.ClientIds))
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "Request is empty." });
        }

        var newFilter = new PortfolioMonitor
        {
            ClientIds = portfolioFilter.ClientIds,
            Description = portfolioFilter.Description,
            Title = portfolioFilter.Title,
            MonitorType = portfolioFilter.MonitorType,
            FilterId = portfolioFilter.FilterId,
        };
        
        _db.PortfolioMonitors.Add(newFilter);

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

    [HttpPut("{id}/Filter")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Edit(int id, [FromBody] PortfolioFilterRequest portfolioFilter)
    {
        if (portfolioFilter == null || string.IsNullOrWhiteSpace(portfolioFilter.ClientIds))
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "Request is empty." });
        }

        var existingFilter = _db.PortfolioMonitors.Find(id);
        if (existingFilter == null)
        {
            return NotFound(new ActionResponse { Result = 1, Message = "Filter doesn't exist." });
        }

        // Update the changed properties
        existingFilter.ClientIds = portfolioFilter.ClientIds;
        existingFilter.Description = portfolioFilter.Description;
        existingFilter.Title = portfolioFilter.Title;
        existingFilter.MonitorType = portfolioFilter.MonitorType;

        _db.Entry(existingFilter).State = EntityState.Modified;
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
    
    [HttpDelete("{id}/Filter")]
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

        var filter = _db.PortfolioMonitors.Find(id);
        if (filter == null)
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "Member doesn't exist." });
        }

        _db.PortfolioMonitors.Remove(filter);

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

