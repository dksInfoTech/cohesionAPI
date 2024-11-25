using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
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
using System.Data;
using System.Xml.Linq;

namespace Product.Web.Controllers;

public class ProposalTeamMemberController : ControllerBase
{
    private readonly DBContext _db;
    private readonly IClientService _clientService;
    private readonly IProposalService _proposalService;
    private readonly UserContextService _userContextService;

    public ProposalTeamMemberController(DBContext db, IClientService clientService, IProposalService proposalService,
        UserContextService userContextService)
    {
        _db = db;
        _clientService = clientService;
        _proposalService = proposalService;
        _userContextService = userContextService;
    }

    /// <summary>
    /// Retrieve collection of proposal team members.
    /// </summary>
    /// <param name="id">Proposal identifier.</param>
    /// <returns></returns>
    [HttpGet("api/Proposal/{id}/TeamMembers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult GetTeamMembers(int id)
    {
        var proposal = _proposalService.Get(id);

        if (proposal == null)
        {
            return NotFound(new ActionResponse { Result = 1, Message = "Not found." });
        }
        var teamMemberData = proposal.ProposalTeamMembers
             .Select(group => new ProposalTeamMemberViewModel
             {
                 Id = group.Id,
                 ProposalId = group.ProposalId,
                 UserId = group.UserId,
                 Title = group.Title,
                 Role = group.Role,
                 Decision = group.Decision,
                 IsFinal = group.IsFinal,
                 Comments = group.Comments,
                 LastDecisionDate = group.LastDecisionDate,
                 ExpectedDecisionDate = group.ExpectedDecisionDate,
                 UserImage = group.User.Image,
                 UserRole = group.User.Role,
                 ClientId = group.Proposal.ClientId,
                 UserName = group.User.FirstName + " " + group.User.LastName,
             })
             .ToList();

        if (!string.IsNullOrEmpty(proposal.Decision) && ProposalWorkFlowStatus.ClosedValues.Contains(proposal.Status))
        {
            return Ok(teamMemberData.Where(s => s.IsFinal));
        }
        return Ok(teamMemberData);
    }

    /// <summary>
    /// Retrieve proposal team member details.
    /// </summary>
    /// <param name="id">proposal identifier.</param>
    /// <param name="teamMemberId">Participant identifier.</param>
    /// <returns></returns>
    [HttpGet("api/Proposal/{id}/TeamMember/{teamMemberId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult Get(int id, int teamMemberId)
    {
        var proposal = _db.Proposals.Find(id);
        var proposalTeamMember = _db.ProposalTeamMembers.Find(teamMemberId);

        if (proposal == null || proposalTeamMember == null || proposalTeamMember.ProposalId != proposal.Id)
        {
            return NotFound(new ActionResponse { Result = 1, Message = "Not found." });
        }

        return Ok(proposalTeamMember);
    }

    /// <summary>
    /// Create a new proposal team member.
    /// </summary>
    /// <param name="id">proposal identifier.</param>
    /// <param name="teamMember">team member details.</param>
    /// <returns></returns>
    [HttpPost("api/Proposal/{id}/TeamMember")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Post(int id, [FromBody] SaveProposalTeamMemberRequest teamMember)
    {
        if (teamMember == null)
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "Request cannot be null." });
        }

        if (string.IsNullOrEmpty(teamMember.UserId))
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "User cannot be null." });
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

        // Validate the user is active
        if (!_db.Users.Any(x => x.Id == teamMember.UserId && x.Active))
        {
            return NotFound(new ActionResponse { Result = 1, Message = "The user does not have access to application." });
        }

        // Validate the team member hasn't already been created
        if (teamMember.Role != ProposalRole.Task.ToDescription()
        && proposal.ProposalTeamMembers.Any(x => x.UserId == teamMember.UserId && x.Role != ProposalRole.Task.ToDescription() && !string.IsNullOrEmpty(x.Role)))
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "The user is already a member of the proposal." });
        }

        // Validate the team member has access to the client
        var hasClientAccess = _db.ClientTeamMembers.Any(x => x.ClientId == proposal.ClientId && x.ClientUserMap.UserId == teamMember.UserId);

        if (!hasClientAccess)
        {
            var clientAccessMap = _db.ClientUserAccessMapping.FirstOrDefault(x => x.ClientId == proposal.ClientId && x.UserId == teamMember.UserId);
            var clientUserAccess = new ClientUserAccess
            {
                UserId = teamMember.UserId,
                ClientId = proposal.ClientId,
                Admin = true
            };
            if (clientAccessMap == null)
            {
                _db.ClientUserAccessMapping.Add(clientUserAccess);
                await _db.SaveChangesAsync();
            }
            var newTeamMember = new ClientTeamMember
            {
                ClientId = proposal.ClientId,
                ClientUserMapId = clientAccessMap != null ? clientAccessMap.Id : clientUserAccess.Id,
                ProposalRole = teamMember.Role
            };
            _db.ClientTeamMembers.Add(newTeamMember);
        }

        // Validate the proposal team member role
        if (!ProposalWorkFlowRole.Values.Contains(teamMember.Role))
        {
            return BadRequest(new ActionResponse { Result = 1, Message = $"Invalid Role (must be: {string.Join(", ", ProposalWorkFlowRole.Values)})." });
        }

        var newProposalTeamMember = new ProposalTeamMember
        {
            ProposalId = id,
            UserId = teamMember.UserId.ToLower(),
            Role = teamMember.Role,
            ExpectedDecisionDate = teamMember.ExpectedDecisionDate,
            Comments = teamMember.Comments,
            Title = teamMember.Title
        };

        try
        {
            _db.ProposalTeamMembers.Add(newProposalTeamMember);

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
            ModelId = newProposalTeamMember.Id,
            ModifiedBy = newProposalTeamMember.ModifiedBy,
            ModifiedDate = newProposalTeamMember.ModifiedDate,
            ModifiedDateString = string.Format("{0:dd-MMM-yyyy HH:mm:ss}", newProposalTeamMember.ModifiedDate)
        });
    }

    /// <summary>
    /// Create a new proposal team member.
    /// </summary>
    /// <param name="id">proposal identifier.</param>
    /// <param name="teamMember">team member details.</param>
    /// <returns></returns>
    [HttpPost("api/Proposal/{id}/CreateTeamMember")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> CreateTeamMember(int id, [FromBody] SaveProposalTeamMemberRequest teamMember)
    {
        if (teamMember == null)
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "Request cannot be null." });
        }

        if (string.IsNullOrEmpty(teamMember.UserId))
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "User cannot be null." });
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

        // Validate the user is active
        if (!_db.Users.Any(x => x.Id == teamMember.UserId && x.Active))
        {
            return NotFound(new ActionResponse { Result = 1, Message = "The user does not have access to application." });
        }

        // Validate the team member hasn't already been created
        if (teamMember.Role != ProposalRole.Task.ToDescription()
        && proposal.ProposalTeamMembers.Any(x => x.UserId == teamMember.UserId && x.Role != ProposalRole.Task.ToDescription()))
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "The user is already a member of the proposal." });
        }

        // Validate the team member has access to the client
        var hasClientAccess = _db.ClientTeamMembers.Any(x => x.ClientId == proposal.ClientId && x.ClientUserMap.UserId == teamMember.UserId);

        if (!hasClientAccess)
        {
            var clientAccessMap = _db.ClientUserAccessMapping.FirstOrDefault(x => x.ClientId == proposal.ClientId && x.UserId == teamMember.UserId);
            var clientUserAccess = new ClientUserAccess
            {
                UserId = teamMember.UserId,
                ClientId = proposal.ClientId,
                Admin = true
            };
            if (clientAccessMap == null)
            {
                _db.ClientUserAccessMapping.Add(clientUserAccess);
                await _db.SaveChangesAsync();
            }
            var newTeamMember = new ClientTeamMember
            {
                ClientId = proposal.ClientId,
                ClientUserMapId = clientAccessMap != null ? clientAccessMap.Id : clientUserAccess.Id,
                ProposalRole = teamMember.Role
            };
            _db.ClientTeamMembers.Add(newTeamMember);
        }

        var newProposalTeamMember = new ProposalTeamMember
        {
            ProposalId = id,
            UserId = teamMember.UserId.ToLower(),
            Role = teamMember.Role,
            ExpectedDecisionDate = teamMember.ExpectedDecisionDate,
            Comments = teamMember.Comments,
            Title = teamMember.Title
        };

        try
        {
            _db.ProposalTeamMembers.Add(newProposalTeamMember);

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
            ModelId = newProposalTeamMember.Id,
            ModifiedBy = newProposalTeamMember.ModifiedBy,
            ModifiedDate = newProposalTeamMember.ModifiedDate,
            ModifiedDateString = string.Format("{0:dd-MMM-yyyy HH:mm:ss}", newProposalTeamMember.ModifiedDate)
        });
    }

    /// <summary>
    /// Update a proposal team member.
    /// </summary>
    /// <param name="id">proposal identifier.</param>
    /// <param name="teamMemberId">team member identifier.</param>
    /// <param name="teamMember">team member details.</param>
    /// <returns></returns>
    [HttpPut("api/Proposal/{id}/TeamMember/{teamMemberId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Put(int id, int teamMemberId, [FromBody] SaveProposalTeamMemberRequest teamMember)
    {
        if (teamMember == null)
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "Request cannot be null." });
        }

        var proposal = _db.Proposals.Find(id);
        var editProposalTeamMember = _db.ProposalTeamMembers.Find(teamMemberId);
        string userId = _userContextService.GetUserId();

        if (editProposalTeamMember == null || proposal == null || editProposalTeamMember.ProposalId != proposal.Id)
        {
            return NotFound(new ActionResponse { Result = 1, Message = "Not found." });
        }

        // Validate the proposal is not already closed
        if (ProposalWorkFlowStatus.ClosedValues.Contains(proposal.Status))
        {
            return StatusCode(StatusCodes.Status406NotAcceptable, new ActionResponse { Result = 1, Message = "Proposal is closed." });
        }

        // Validate the team member has not already finalised (and it's not Draft or Rework status)
        if (editProposalTeamMember.IsFinal && !new[] { ProposalStatus.Draft.ToDescription(), ProposalStatus.Rework.ToDescription() }.Contains(proposal.Status))
        {
            return StatusCode(StatusCodes.Status406NotAcceptable, new ActionResponse { Result = 1, Message = "Cannot update when the decision is finalised." });
        }

        // Validate the proposal team member role
        if (!ProposalWorkFlowRole.Values.Contains(teamMember.Role))
        {
            return BadRequest(new ActionResponse { Result = 1, Message = $"Invalid Role (must be: {string.Join(", ", ProposalWorkFlowRole.Values)})." });
        }

        if (teamMember.UserId != editProposalTeamMember.UserId)
        {
            // Validate the user is active
            if (!_db.Users.Any(x => x.Id == teamMember.UserId && x.Active))
            {
                return NotFound(new ActionResponse { Result = 1, Message = "The user does not have access to application." });
            }

            // Validate the new member isn't already a member on the proposal
            if (teamMember.Role != ProposalRole.Task.ToDescription()
            && proposal.ProposalTeamMembers.Any(x => x.UserId == teamMember.UserId
            && x.Role != ProposalRole.Task.ToDescription()))
            {
                return BadRequest(new ActionResponse { Result = 1, Message = "The user is already a member of the proposal." });
            }

            // Change the user on the team member record, clear the decision
            editProposalTeamMember.UserId = teamMember.UserId.ToLower();
            editProposalTeamMember.Decision = null;
            editProposalTeamMember.LastDecisionDate = null;
        }

        if (new[] { ProposalStatus.Draft.ToDescription(), ProposalStatus.Rework.ToDescription() }.Contains(proposal.Status))
        {
            // Change the details
            editProposalTeamMember.Role = teamMember.Role;
            editProposalTeamMember.ExpectedDecisionDate = teamMember.ExpectedDecisionDate;
            editProposalTeamMember.Title = teamMember.Title;
        }

        editProposalTeamMember.Comments = teamMember.Comments;

        // Only the member can update their decision and comments
        //if (editProposalTeamMember.UserId == userId)
        //{
        // Validate the decision if not null
        // Note the decision is allowed to be null
        if (teamMember.Decision != null && !ProposalWorkFlowDecision.Validate(teamMember.Decision, teamMember.Role, out string message))
        {
            return StatusCode(StatusCodes.Status406NotAcceptable, new ActionResponse { Result = 1, Message = message });
        }

        if (editProposalTeamMember.Decision != teamMember.Decision)
        {
            editProposalTeamMember.Decision = teamMember.Decision;
            editProposalTeamMember.LastDecisionDate = DateTime.Now;
        }
        if (teamMember.Role == ProposalRole.Task.ToDescription() && teamMember.Decision == ProposalDecision.TaskCompleted.ToDescription())
        {
            editProposalTeamMember.IsFinal = true;
        }
        //}
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
            ModelId = editProposalTeamMember.Id,
            ModifiedBy = editProposalTeamMember.ModifiedBy,
            ModifiedDate = editProposalTeamMember.ModifiedDate,
            ModifiedDateString = string.Format("{0:dd-MMM-yyyy HH:mm:ss}", editProposalTeamMember.ModifiedDate)
        });
    }

    /// <summary>
    /// Delete a proposal team member.
    /// </summary>
    /// <param name="id">proposal identifier.</param>
    /// <param name="teamMemberId">team member identifier.</param>
    /// <returns></returns>
    [HttpDelete("api/Proposal/{id}/TeamMember/{teamMemberId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Delete(int id, int teamMemberId)
    {
        var proposal = _db.Proposals.Find(id);
        var deleteProposalTeamMember = _db.ProposalTeamMembers.Find(teamMemberId);

        if (deleteProposalTeamMember == null || proposal == null || deleteProposalTeamMember.ProposalId != proposal.Id)
        {
            return NotFound(new ActionResponse { Result = 1, Message = "Not found." });
        }

        // Validate the proposal is not already closed
        if (ProposalWorkFlowStatus.ClosedValues.Contains(proposal.Status))
        {
            return StatusCode(StatusCodes.Status406NotAcceptable, new ActionResponse { Result = 1, Message = "Proposal is closed." });
        }

        // Validate the member has not already finalised
        if (deleteProposalTeamMember.IsFinal)
        {
            return StatusCode(StatusCodes.Status406NotAcceptable, new ActionResponse { Result = 1, Message = "Cannot delete when the decision is finalised." });
        }

        // Remove submission participant
        _db.ProposalTeamMembers.Remove(deleteProposalTeamMember);

        try
        {
            // Commit changes
            await _db.SaveChangesAsync();
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.Message });
        }

        return Ok(new ActionSaveResponse { Result = 0, ModelId = teamMemberId });
    }
}
