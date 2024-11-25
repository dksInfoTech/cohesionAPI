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

public class ProposalWorkFlowController : ControllerBase
{
    private readonly DBContext _db;
    private readonly IClientService _clientService;
    private readonly IProposalService _proposalService;
    private readonly UserContextService _userContextService;
    private readonly IRuleSetService _ruleSetService;

    public ProposalWorkFlowController(DBContext db, IClientService clientService, IProposalService proposalService,
        UserContextService userContextService, IRuleSetService ruleSetService)
    {
        _db = db;
        _clientService = clientService;
        _proposalService = proposalService;
        _userContextService = userContextService;
        _ruleSetService = ruleSetService;
    }

    /// <summary>
    /// Finalize the Proposal.
    /// </summary>
    /// <param name="id">proposal identifier.</param>
    /// <param name="proposal">proposal details.</param>
    /// <returns></returns>
    [HttpPost("api/Proposal/{id}/Finalize")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> PostFinalize(int id, [FromBody] ProposalFinalizeRequest proposal) 
    {
        if (proposal == null)
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "Request cannot be null." });
        }

        var editProposal = _db.Proposals.Find(id);
        string userId = _userContextService.GetUserId();

        if (editProposal == null)
        {
            return NotFound(new ActionResponse { Result = 1, Message = "Not found." });
        }

        // Validate the current status
        if (editProposal.Status != ProposalStatus.Pending.ToDescription())
        {
            return BadRequest(new ActionResponse { Result = 1, Message = $"The proposal has been updated to {editProposal.Status}. Please refresh the screen." });
        }

        // Get this user's team member details for this proposal
        var proposalTeamMembers = editProposal.ProposalTeamMembers
                                            .Where(x => x.Role != ProposalRole.Task.ToDescription()); //x.UserId != userId && 

        if (proposalTeamMembers == null)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ActionResponse { Result = 1, Message = "You must be a member of this proposal." });
        }

        var openTasks = editProposal.ProposalTeamMembers.Any(x => x.Role == ProposalRole.Task.ToDescription() && !x.IsFinal);

        if (openTasks)
        {
            return StatusCode(StatusCodes.Status406NotAcceptable, new ActionResponse { Result = 1, Message = "There are pending tasks in the proposal." });
        }

        //check if any pending decision
        var openDecisions = editProposal.ProposalTeamMembers
                                            .Any(x => !string.IsNullOrEmpty(x.Role) && x.Role != ProposalRole.Task.ToDescription()
                                                    && string.IsNullOrEmpty(x.Decision));
        if (openDecisions)
        {
            return StatusCode(StatusCodes.Status406NotAcceptable, new ActionResponse { Result = 1, Message = "There are pending decisions in the proposal." });
        }

        var toBeFinalizedDecisions = proposalTeamMembers.Where(x => !x.IsFinal);

        foreach (var proposalTeamMember in toBeFinalizedDecisions)
        {
            proposalTeamMember.IsFinal = true;
            _db.Entry(proposalTeamMember).Property(p => p.IsFinal).IsModified = true;
        }

        bool finalised = false;

        if (proposal.Decision == ProposalDecision.Rework.ToDescription())
        {
            // Update the proposal status
            editProposal.Status = ProposalStatus.Rework.ToDescription();
            _db.Entry(editProposal).Property(p => p.Status).IsModified = true;

            // Update the draft status if there is a client change
            if (editProposal.IsClientUpdate)
            {
                // Find the draft
                var draft = _db.ClientDrafts.FirstOrDefault(x => x.ClientId == editProposal.ClientId && !x.IsDeleted);

                if (draft != null)
                {
                    _db.ClientDrafts.Attach(draft);
                    draft.Status = ProposalStatus.Rework.ToDescription();
                    _db.Entry(draft).Property(p => p.Status).IsModified = true;
                }
            }
        }
        else
        {
            try
            {
                // Check if the entire proposal can be finalized
                finalised = await _proposalService.Finalize(editProposal, _db);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.Message });
            }
        }
        try
        {
            // Commit changes
            await _db.SaveChangesAsync();
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.GetBaseException().Message });
        }
        return Ok(new ActionSaveResponse
        {
            Result = 0,
            ModelId = editProposal.Id,
            ModifiedBy = editProposal.ModifiedBy,
            ModifiedDate = editProposal.ModifiedDate,
            ModifiedDateString = string.Format("{0:dd-MMM-yyyy HH:mm:ss}", editProposal.ModifiedDate)
        });
    }

    /// <summary>
    /// Change the status to "Pending".
    /// </summary>
    /// <param name="id">proposal identifier.</param>
    /// <returns></returns>
    [HttpPost("api/Proposal/{id}/Status/Pending")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PostStatusPending(int id)
    {
        var proposal = _db.Proposals.Find(id);
        if (proposal == null)
        {
            return NotFound(new ActionResponse { Result = 1, Message = "Not found." });
        }
        if (proposal.ProposalTeamMembers == null || !proposal.ProposalTeamMembers.Any(x => ProposalWorkFlowRole.AllowedRolesForFinalize.Contains(x.Role)))
        {
            return StatusCode(StatusCodes.Status406NotAcceptable, new ActionResponse { Result = 1, Message = "A proposal needs at least one team member for approval, noting or unopposed." });
        }
        // var openExceptions = false;
        IEnumerable<RuleResponse>? outcomes = null;
        var clientDraft = _db.ClientDrafts.FirstOrDefault(x => x.ProposalId == id);
        if (clientDraft != null)
        {
            outcomes = _ruleSetService.Trigger(clientDraft.BasicInformation, "Proposal");
            var failedRules = outcomes.Where(outcome => outcome.Status == "Failed");

            var approvedExceptions = _db.RuleOutcomes.Where(x => x.SourceId == id
            && x.Status == RuleExceptionStatus.Approved);
            if (failedRules?.Count() != approvedExceptions?.Count())
            {
                return Ok(new ActionSaveResponse<IEnumerable<RuleResponse>>
                {
                    Result = 1,
                    Message = "There are open exceptions, hence this Proposal can't proceed.",
                    ModelId = proposal.Id,
                    ModifiedBy = proposal.ModifiedBy,
                    ModifiedDate = proposal.ModifiedDate,
                    ModifiedDateString = string.Format("{0:dd-MMM-yyyy HH:mm:ss}", proposal.ModifiedDate),
                    RuleResponse = outcomes
                });
            }
        }
        // var openExceptions = _db.RuleOutcomes.Any(x => x.SourceId == id
        //     && new[] { RuleExceptionStatus.ForApproval, RuleExceptionStatus.Declined }.Contains(x.Status));

        var openTasks = _db.ProposalTeamMembers.Any(x => x.ProposalId == id && x.Role == ProposalRole.Task.ToDescription() && !x.IsFinal);

        if (openTasks) 
        {
            return StatusCode(StatusCodes.Status406NotAcceptable, new ActionResponse { Result = 1, Message = "There are pending tasks in the proposal." });
        }

        return await UpdateStatus(id, ProposalStatus.Pending.ToDescription(), outcomes);
        // return openExceptions ?
        //     BadRequest(new ActionResponse { Result = 1, Message = "There are open exceptions, hence this Proposal can't proceed." })
        //     : await UpdateStatus(id, ProposalStatus.Pending.ToDescription(), outcomes);
    }

    /// <summary>
    /// Change the status to "Cancelled".
    /// </summary>
    /// <param name="id">proposal identifier.</param>
    /// <returns></returns>
    [HttpPost("api/Proposal/{id}/Status/Cancelled")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PostStatusCancelled(int id)
    {
        return await UpdateStatus(id, ProposalStatus.Cancelled.ToDescription());
    }

    /// <summary>
    /// Change the status to "Rework". Used to "recall" the proposal.
    /// </summary>
    /// <param name="id">proposal identifier.</param>
    /// <returns></returns>
    [HttpPost("api/Proposal/{id}/Status/Rework")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PostStatusRework(int id)
    {
        return await UpdateStatus(id, ProposalStatus.Rework.ToDescription());
    }

    /// <summary>
    /// Withdraw a proposal.
    /// </summary>
    /// <param name="id">proposal identifier.</param>
    /// <returns></returns>
    [HttpPost("api/Proposal/{id}/Status/Withdraw")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PostStatusWithdraw(int id)
    {
        var withdrawProposal = _db.Proposals.Find(id);
        if (withdrawProposal == null)
        {
            return NotFound(new ActionResponse { Result = 1, Message = "Not found." });
        }
        // Remove linked proposal images
        foreach (var i in _db.Images.Where(x => x.ProposalId == id))
        {
            _db.Images.Remove(i);
        }
        // delete client draft
        // delete facility
        // delete facilitydocument
        // delete events dashboard

        // Delete client draft
        await _clientService.DeleteDraft(withdrawProposal.ClientId, proposalId: id);
        _proposalService.DeleteDashboardEvents(withdrawProposal.ClientId, id);

        // Update status to Withdrawn and create client archive if there's a client update
        return await UpdateStatus(id, ProposalStatus.Withdrawn.ToDescription());
    }

    /// <summary>
    /// Update the status of a Proposal (only used for Pending, Cancelled and Rework).
    /// </summary>
    /// <param name="id">proposal identifier.</param>
    /// <param name="status">New status value.</param>
    /// <param name="outcomes">Rule outcome.</param>
    /// <returns></returns>
    private async Task<IActionResult> UpdateStatus(int id, string status, IEnumerable<RuleResponse>? outcomes = null)
    {
        var proposal = _db.Proposals.Find(id);
        string userId = _userContextService.GetUserId();

        if (proposal == null)
        {
            return NotFound(new ActionResponse { Result = 1, Message = "Not found." });
        }

        // Validate the requested status change is allowed
        if (!ProposalWorkFlowStatus.ValidateWorkflow(proposal.Status, status, out string? message))
        {
            return StatusCode(StatusCodes.Status406NotAcceptable, new ActionResponse { Result = 1, Message = message });
        }

        // If the status is Pending then the proposal has been "submitted"
        if (status == ProposalStatus.Pending.ToDescription())
        {
            // Clear all team member decisions if moved from Draft or Rework to Pending, and the IsFinal flag, so that the user can re-execute their decision
            foreach (var p in proposal.ProposalTeamMembers.Where(x => x.Role != ProposalRole.Task.ToDescription()))
            {
                p.Decision = null;
                p.LastDecisionDate = null;
                p.IsFinal = false;
            }

            // Update the submitter details
            proposal.LastContributorId = "system"; //userId
            proposal.LastContributedDate = DateTime.Now;
            _db.Entry(proposal).Property(p => p.LastContributorId).IsModified = true;
            _db.Entry(proposal).Property(p => p.LastContributedDate).IsModified = true;
        }

        // Update the proposal status
        proposal.Status = status;
        _db.Entry(proposal).Property(p => p.Status).IsModified = true;

        // Update the client version number and closed date on the proposal if the proposal is being closed
        if (ProposalWorkFlowStatus.ClosedValues.Contains(status))
        {
            proposal.ClientVersion = _clientService.GetClientVersionNumber(proposal.ClientId);
            _db.Entry(proposal).Property(p => p.ClientVersion).IsModified = true;

            // Update the closing details
            proposal.ClosedDate = DateTime.Now;
            _db.Entry(proposal).Property(p => p.ClosedDate).IsModified = true;
        }

        // Update the draft client if it's linked to this proposal
        if (proposal.IsClientUpdate)
        {
            // The action depends upon the proposal status
            if (status == ProposalStatus.Rework.ToDescription())
            {
                // Update the draft to Rework
                // Find the draft
                var draft = _db.ClientDrafts.FirstOrDefault(x => !x.IsDeleted && x.ClientId == proposal.ClientId && x.ProposalId == id);

                if (draft != null)
                {
                    _db.ClientDrafts.Attach(draft);
                    draft.Status = status;
                    _db.Entry(draft).Property(p => p.Status).IsModified = true;
                }
            }
            else if (status == ProposalStatus.Pending.ToDescription())
            {
                // Update the draft to Pending
                // Find the draft
                var draft = _db.ClientDrafts.FirstOrDefault(x => !x.IsDeleted && x.ClientId == proposal.ClientId && x.ProposalId == id);

                if (draft != null)
                {
                    _db.ClientDrafts.Attach(draft);
                    draft.Status = status;
                    _db.Entry(draft).Property(p => p.Status).IsModified = true;

                }
            }
            else if (status == ProposalStatus.Cancelled.ToDescription())
            {
                // Delete the draft
                await _clientService.DeleteDraft(proposal.ClientId, proposalId: id);
            }
            //else if (status == ProposalStatus.Withdrawn.ToDescription())
            //{
            //    // Archive the client
            //    _clientService.CreateArchive(id);
            //}
        }

        try
        {
            // Commit changes
            await _db.SaveChangesAsync();
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.Message });
        }

        return Ok(new ActionSaveResponse<IEnumerable<RuleResponse>>
        {
            Result = 0,
            ModelId = proposal.Id,
            ModifiedBy = proposal.ModifiedBy,
            ModifiedDate = proposal.ModifiedDate,
            ModifiedDateString = string.Format("{0:dd-MMM-yyyy HH:mm:ss}", proposal.ModifiedDate),
            RuleResponse = outcomes
        });
    }
}
