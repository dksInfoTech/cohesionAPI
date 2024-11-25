using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Product.Bal;
using Product.Bal.Interfaces;
using Product.Bal.Models;
using Product.Dal;
using Product.Dal.Common.Extensions;
using Product.Dal.Common.Workflow;
using Product.Dal.Entities;
using Product.Dal.Enums;
using Product.Web.Models.Client;
using Product.Web.Models.Proposal;
using Product.Web.Models.Response;

namespace Product.Web.Controllers;

public class ProposalEventController : ControllerBase
{
    private readonly DBContext _db;

    public ProposalEventController(DBContext db)
    {
        _db = db;
    }
    /// <summary>
    /// Retrieve collection of proposal events.
    /// </summary>
    /// <param name="id">Proposal identifier.</param>
    /// <returns></returns>
    [HttpGet("api/Proposal/{id}/Event")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GetEvents(int id)
    {
        var proposal = _db.Proposals.Find(id);

        if (proposal == null)
        {
            return NotFound(new ActionResponse { Result = 1, Message = "Not found." });
        }

        var proposalEvents = proposal.ProposalEvents.OrderBy(x => x.Order);

        return Ok(proposalEvents);
    }

    /// <summary>
    /// Retrieve collection of proposal events as per Client.
    /// </summary>
    /// <param name="id">ClientId</param>
    /// <returns></returns>
    [HttpGet("api/Client/{id}/Events")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GetClientEvents(int id)
    {
        IQueryable<ProposalEvent> query;

        if (id == 0)
        {
            // When id is 0, get all proposal events
            query = _db.Proposals
                .SelectMany(p => p.ProposalEvents); // Flatten the collection of events
        }
        else
        {
            // When id is not 0, get proposal events for the specified clientId
            query = _db.Proposals
                .Where(p => p.ClientId == id)
                .SelectMany(p => p.ProposalEvents); // Flatten the collection of events
        }

        // Check if the query result is empty or not found
        var proposalEvents = query
            .OrderBy(e => e.Order) // Sort the events by 'Order'
             .Select(e => new
             {
                 events= e,
                 // Add extra columns here
                 clientId = e.Proposal.ClientId, // Or any calculated value
             }).ToList();

        // if (!proposalEvents.Any())
        // {
        //     // Return a NotFound result if no events are found
        //     return NotFound(new ActionResponse { Result = 1, Message = "Not found." });
        // }

        return Ok(proposalEvents);

    }

    /// <summary>
    /// Retrieve proposal event details.
    /// </summary>
    /// <param name="id">proposal identifier.</param>
    /// <param name="eventId">proposal event identifier.</param>
    /// <returns></returns>
    [HttpGet("api/Proposal/{id}/Event/{eventId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult Get(int id, int eventId)
    {
        var proposal = _db.Proposals.Find(id);
        var proposalEvent = _db.ProposalEvents.Find(eventId);

        if (proposal == null || proposalEvent == null || proposalEvent.ProposalId != proposal.Id)
        {
            return NotFound(new ActionResponse { Result = 1, Message = "Not found." });
        }

        return Ok(proposalEvent);
    }

    /// <summary>
    /// Create a new proposal event.
    /// </summary>
    /// <param name="id">proposal identifier.</param>
    /// <param name="proposalEvent">proposal event details.</param>
    /// <returns></returns>
    [HttpPost("api/Proposal/{id}/Event")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Post(int id, [FromBody] SaveProposalEventRequest proposalEvent)
    {
        if (proposalEvent == null)
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "Request cannot be null." });
        }

        var proposal = _db.Proposals.Find(id);

        if (proposal == null)
        {
            return NotFound(new ActionResponse { Result = 1, Message = "Proposal not found." });
        }

        // Validate the proposal is not already closed
        if (ProposalWorkFlowStatus.ClosedValues.Contains(proposal.Status))
        {
            return StatusCode(StatusCodes.Status406NotAcceptable, new ActionResponse { Result = 1, Message = "Proposal is closed." });
        }

        var newProposalEvent = new ProposalEvent
        {
            ProposalId = id,
            EventDescription = proposalEvent.EventDescription,
            DecisionRationaleInfo = proposalEvent.DecisionRationaleInfo,
            OtherDecisionType = proposalEvent.OtherDecisionType,
            Comments = proposalEvent.Comments,
            Order = proposalEvent.Order
        };

        try
        {
            // Create the proposal event
            _db.ProposalEvents.Add(newProposalEvent);

            // Commit changes
            await _db.SaveChangesAsync();
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.Message });
        }

        return Ok(new ActionSaveResponse
        {
            Result = 0,
            ModelId = newProposalEvent.Id,
            ModifiedBy = newProposalEvent.ModifiedBy,
            ModifiedDate = newProposalEvent.ModifiedDate,
            ModifiedDateString = string.Format("{0:dd-MMM-yyyy HH:mm:ss}", newProposalEvent.ModifiedDate)
        });
    }

    /// <summary>
    /// Update a proposal event.
    /// </summary>
    /// <param name="id">proposal identifier.</param>
    /// <param name="eventId">proposal event identifier.</param>
    /// <param name="proposalEvent">proposal event details.</param>
    /// <returns></returns>
    [HttpPut("api/Proposal/{id}/Event/{eventId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Put(int id, int eventId, [FromBody] SaveProposalEventRequest proposalEvent)
    {
        if (proposalEvent == null)
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "Request cannot be null." });
        }

        var proposal = _db.Proposals.Find(id);
        var editProposalEvent = _db.ProposalEvents.Find(eventId);

        if (editProposalEvent == null || proposal == null || editProposalEvent.ProposalId != proposal.Id)
        {
            return NotFound(new ActionResponse { Result = 1, Message = "This proposal/event required no longer exists." });
        }

        // Validate the proposal is not already closed
        if (ProposalWorkFlowStatus.ClosedValues.Contains(proposal.Status))
        {
            return StatusCode(StatusCodes.Status406NotAcceptable, new ActionResponse { Result = 1, Message = "Proposal is closed." });
        }

        editProposalEvent.EventDescription = proposalEvent.EventDescription;
        editProposalEvent.DecisionRationaleInfo = proposalEvent.DecisionRationaleInfo;        
        editProposalEvent.OtherDecisionType = proposalEvent.OtherDecisionType;
        editProposalEvent.Comments = proposalEvent.Comments;
        editProposalEvent.Order = proposalEvent.Order;

        try
        {
            // Commit changes
            await _db.SaveChangesAsync();
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.Message });
        }

        return Ok(new ActionSaveResponse
        {
            Result = 0,
            ModelId = editProposalEvent.Id,
            ModifiedBy = editProposalEvent.ModifiedBy,
            ModifiedDate = editProposalEvent.ModifiedDate,
            ModifiedDateString = string.Format("{0:dd-MMM-yyyy HH:mm:ss}", editProposalEvent.ModifiedDate)
        });
    }

    /// <summary>
    /// Delete a proposal event.
    /// </summary>
    /// <param name="id">proposal identifier.</param>
    /// <param name="eventId">proposal event identifier.</param>
    /// <returns></returns>
    [HttpDelete("api/Proposal/{id}/Event/{eventId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Delete(int id, int eventId)
    {
        var proposal = _db.Proposals.Find(id);
        var deleteProposalEvent = _db.ProposalEvents.Find(eventId);

        if (deleteProposalEvent == null || proposal == null || deleteProposalEvent.ProposalId != proposal.Id)
        {
            return NotFound(new ActionResponse { Result = 1, Message = "Not found." });
        }

        // Validate the proposal is not already closed
        if (ProposalWorkFlowStatus.ClosedValues.Contains(proposal.Status))
        {
            return StatusCode(StatusCodes.Status406NotAcceptable, new ActionResponse { Result = 1, Message = "Proposal is closed." });
        }

        // Remove proposal event
        _db.ProposalEvents.Remove(deleteProposalEvent);

        try
        {
            // Commit changes
            await _db.SaveChangesAsync();
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.Message });
        }

        return Ok(new ActionSaveResponse { Result = 0, ModelId = eventId });
    }
}
