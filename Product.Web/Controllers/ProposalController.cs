using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
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
using Product.Web.Models.Facility;
using Product.Web.Models.Proposal;
using Product.Web.Models.Response;
using System.Reflection;

namespace Product.Web.Controllers;

[Route("api/[controller]")]
public class ProposalController : ControllerBase
{
    private readonly DBContext _db;
    private readonly IClientService _clientService;
    private readonly IProposalService _proposalService;
    private readonly IMapper _mapper;
    private readonly Settings _settings;
    private readonly IRuleSetService _ruleSetService;
    private readonly UserContextService _userContextService;
    private readonly IClientUserAccessMappingService _clientUamService;
    private readonly IFacilityService _facilityService;

    public ProposalController(DBContext db, IClientService clientService, IProposalService proposalService,
        IMapper mapper, IOptions<Settings> settings, IRuleSetService ruleSetService,
        UserContextService userContextService, IClientUserAccessMappingService clientUamService, IFacilityService facilityService)
    {
        _db = db;
        _clientService = clientService;
        _proposalService = proposalService;
        _mapper = mapper;
        _settings = settings.Value;
        _ruleSetService = ruleSetService;
        _userContextService = userContextService;
        _clientUamService = clientUamService;
        _facilityService = facilityService;
    }

    /// <summary>
    /// Retrieve proposal details.
    /// </summary>
    /// <param name="id">Proposal identifier.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult Get(int id)
    {
        var proposal = _proposalService.Get(id);

        if (proposal == null)
        {
            return NotFound(new ActionResponse { Result = 1, Message = "The proposal does not exist." });
        }

        proposal.ProposalEvents = proposal.ProposalEvents.OrderBy(x => x.Order).ToList();

        var proposalModel = _mapper.Map<Proposal, ProposalViewModel>(proposal);

        // Get all the open submissions except current submission to determine state of others that are in progress
        var openProposals = _db.Proposals.Where(x => x.ClientId == proposal.ClientId && x.Id != id && ProposalWorkFlowStatus.OpenValues.Contains(x.Status));
        proposalModel.ProposalHasClientUpdate = openProposals.Any(x => x.IsClientUpdate);

        return Ok(proposalModel);
    }

    /// <summary>
    /// Retrieve the proposals for a customer.
    /// </summary>
    /// <param name="clientId">Client identifier.</param>
    /// <returns></returns>
    [HttpGet("History/ByClient/{clientId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetByClient(int clientId)
    {
        var proposals = from s in _db.Proposals
                        where s.ClientId == clientId
                        select new ProposalHistory
                        {
                            Id = s.Id,
                            CreatedDate = s.CreatedDate,
                            ClientId = s.ClientId,
                            ClientVersion = s.ClientVersion,
                            Status = s.Status,
                            Decision = s.Decision,
                            ProposalInfo = s.ProposalInfo,
                            IsClientUpdate = s.IsClientUpdate,
                            ClosedDate = s.ClosedDate,
                            // LastContributorName = s.LastContributorName ?? s.ModifiedBy,
                            LastContributorId = s.LastContributorId,
                            LastContributedDate = s.LastContributedDate,
                            //ProposalTeamMembers = s.ProposalTeamMembers,
                        };

        var orderedProposals = proposals.ToList()
            .OrderByDescending(x => ProposalWorkFlowStatus.OpenValues.Contains(x.Status)
            ? "B" + x.CreatedDate.ToString("yyyyMMddHHmmss")
            : "A" + x.ClosedDate?.ToString("yyyyMMddHHmmss"));

        return Ok(orderedProposals);
    }

    /// <summary>
    /// Create a new proposal.
    /// </summary>
    /// <remarks>
    /// Notes:<br/>
    /// 1) If linked to a client change, the proposal will be created in the Draft status and a client draft will also be created.<br/>
    /// 2) If not linked to a client change, the proposal will be automatically created in the Pending status.
    /// </remarks>
    /// <param name="proposal">Proposal details.</param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Post([FromBody] SaveProposalRequest proposal)
    {
        if (proposal == null)
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "Request cannot be null." });
        }

        if (!_db.Clients.Any(x => x.Id == proposal.ClientId))
        {
            return NotFound(new ActionResponse { Result = 1, Message = "Client not found." });
        }

        //var loggedInUser = _userContextService.GetUserId();

        //var hasProfileAccess = _db.ClientTeamMembers.Any(x => x.ClientUserMap.ClientId == proposal.ClientId && x.ClientUserMap.UserId == loggedInUser);
        //if (!hasProfileAccess)
        //{
        //    var clientUserAccessMap = _db.ClientUserAccessMapping.FirstOrDefault(x => x.ClientId == proposal.ClientId && x.UserId == loggedInUser);
        //    if (clientUserAccessMap != null)
        //    {
        //        var newTeamMember = new ClientTeamMember
        //        {
        //            ClientUserMapId = clientUserAccessMap.Id,
        //            ClientId = proposal.ClientId

        //        };

        //        _db.ClientTeamMembers.Add(newTeamMember);
        //    }
        //}

        // var isFirstVersion = _db.Proposals.Count(x => x.ClientId == proposal.ClientId) == 0;
        // if (!isFirstVersion)
        // {
        //     proposal.IsClientUpdate = _db.Proposals.Any(x => x.IsClientUpdate
        //                                 && x.ClientId == proposal.ClientId
        //                                 && ProposalWorkFlowStatus.OpenValues.Contains(x.Status));
        // }
        proposal.IsClientUpdate = true;

        var newProposal = new Proposal
        {
            Status = ProposalStatus.Draft.ToDescription(),
            ClientId = proposal.ClientId,
            ProposalInfo = proposal.ProposalInfo,
            IsClientUpdate = proposal.IsClientUpdate,
            Comments = proposal.createdBy,
            CreatedBy =proposal.createdBy,
        };

        try
        {
            _db.Proposals.Add(newProposal);
            await _db.SaveChangesAsync();

            // Create a new client draft linked to this proposal
            if (proposal.IsClientUpdate)
            {
                var actionResult = NewClientDraft(proposal.ClientId, newProposal.Id, newProposal.Status);

                if (!(actionResult is OkResult))
                {
                    return actionResult;
                }
                await _db.SaveChangesAsync();

                await _facilityService.CloneLatestFacilityDocument(proposal.ClientId, newProposal.Id);
                await _facilityService.CloneLatestFacility(proposal.ClientId, newProposal.Id);
                await _facilityService.CloneLatestInterchangableLimit(proposal.ClientId, newProposal.Id);
            }
        }

        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.Message });
        }

        return Ok(new ActionSaveResponse
        {
            Result = 0,
            ModelId = newProposal.Id,
            ModifiedBy = newProposal.ModifiedBy,
            ModifiedDate = newProposal.ModifiedDate,
            ModifiedDateString = string.Format("{0:dd-MMM-yyyy HH:mm:ss}", newProposal.ModifiedDate)
        });
    }

    /// <summary>
    /// Update a proposal.
    /// </summary>
    /// <remarks>
    /// Note: Will also create/delete a client draft if linked/unlinked to the proposal.
    /// </remarks>
    /// <param name="id">Proposal identifier.</param>
    /// <param name="proposal">Proposal details.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Put(int id, [FromBody] SaveProposalRequest proposal)
    {
        if (proposal == null)
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "Request cannot be null." });
        }

        var editProposal = _db.Proposals.Find(id);

        if (editProposal == null)
        {
            return NotFound(new ActionResponse { Result = 1, Message = "Not found." });
        }

        if (ProposalWorkFlowStatus.ClosedValues.Contains(editProposal.Status))
        {
            return StatusCode(StatusCodes.Status406NotAcceptable, new ActionResponse { Result = 1, Message = "Can't update a closed proposal." });
        }

        // Get all the open proposals except current proposal to determine state of others that are in progress
        var openProposals = _db.Proposals.Where(x => x.ClientId == proposal.ClientId && x.Id != id && ProposalWorkFlowStatus.OpenValues.Contains(x.Status));
        var proposalHasProfileUpdate = openProposals.Any(x => x.IsClientUpdate);

        // Only one proposal can have a client change at a time
        if (proposal.IsClientUpdate && !editProposal.IsClientUpdate)
        {
            if (proposalHasProfileUpdate)
            {
                return BadRequest(new ActionResponse { Result = 1, Message = "The client can't be changed while it is being edited in another proposal." });
            }

            // Create a new client draft linked to this proposal (if it doesn't already exist)
            var actionResult = NewClientDraft(editProposal.ClientId, editProposal.Id, editProposal.Status);

            if (!(actionResult is OkResult))
            {
                return actionResult;
            }

        }
        else if (!proposal.IsClientUpdate && editProposal.IsClientUpdate)
        {
            // Delete the draft
            await _clientService.DeleteDraft(editProposal.ClientId, proposalId: id);

        }

        editProposal.IsClientUpdate = proposal.IsClientUpdate;
        editProposal.ProposalInfo = proposal.ProposalInfo;
        editProposal.Title = proposal.Title;
        

        try
        {
            // Commit changes
            await _db.SaveChangesAsync();
            // var outcome = _ruleSetService.Trigger(editProposal.ProposalInfo, "Proposal");
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.Message });
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
    /// Delete a Proposal.
    /// </summary>
    /// <remarks>
    /// Note: Will also delete a client draft if linked to the proposal.
    /// </remarks>
    /// <param name="id">Proposal identifier.</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleteProposal = _db.Proposals.Find(id);

        if (deleteProposal == null)
        {
            return NotFound(new ActionResponse { Result = 1, Message = "Not found." });
        }

        if (ProposalWorkFlowStatus.ClosedValues.Contains(deleteProposal.Status))
        {
            return StatusCode(StatusCodes.Status406NotAcceptable, new ActionResponse { Result = 1, Message = "Cannot delete a closed proposal." });
        }

        // Remove linked proposal events
        foreach (var ev in _db.ProposalEvents.Where(x => x.ProposalId == id))
        {
            _db.ProposalEvents.Remove(ev);
        }

        // Remove linked proposal team members
        foreach (var p in _db.ProposalTeamMembers.Where(x => x.ProposalId == id))
        {
            _db.ProposalTeamMembers.Remove(p);
        }

        // Remove linked proposal images
        foreach (var i in _db.Images.Where(x => x.ProposalId == id))
        {
            _db.Images.Remove(i);
        }

        // Remove proposal
        _db.Proposals.Remove(deleteProposal);

        // Delete the draft if linked to a profile update
        if (deleteProposal.IsClientUpdate)
        {
            await _clientService.DeleteDraft(deleteProposal.ClientId, proposalId: id);
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

        return Ok(new ActionSaveResponse { Result = 0, ModelId = id });
    }

    /// <summary>
    /// Retrieve the client associated with the proposal.
    /// </summary>
    /// <param name="id">Proposal identifier.</param>
    /// <returns></returns>
    [HttpGet]
    [Route("{id}/Client")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Client(int id)
    {
        // Get the proposal
        var proposal = _db.Proposals.Find(id);

        if (proposal == null)
        {
            return NotFound(new ActionResponse { Result = 1, Message = "Not found." });
        }

        // Get the client linked to the proposal
        var baseProfile = _proposalService.GetClient(proposal.Id);

        // Check client exists
        if (baseProfile == null || baseProfile.IsDeleted)
        {
            return NotFound(new ActionResponse { Result = 1, Message = "Client not found." });
        }

        // Map to the view model
        var profile = _mapper.Map<ClientViewModel>(baseProfile);

        // Get the diff client for the proposal (default to an empty client)
        var diffProfile = _proposalService.GetDiffClient(proposal.Id) ?? new Client { Name = string.Empty, BasicInformation = string.Empty };
        profile.DiffClient = diffProfile is Client ? diffProfile as Client : _mapper.Map<Client>(diffProfile);

        profile.DraftProposalId = _db.ClientDrafts.FirstOrDefault(x => x.ClientId == proposal.ClientId && !x.IsDeleted)?.ProposalId;
        profile.HasDraft = profile.DraftProposalId != null;

        var outcome = _ruleSetService.Trigger(profile.BasicInformation, "Proposal");

        return Ok(new ActionResponse<ClientViewModel, IEnumerable<RuleResponse>> { Data = profile, RuleResponse = outcome });
    }

    /// <summary>
    /// Retrieve the list of users that are not team members but are allowed to be.
    /// </summary>
    /// <param name="id">Proposal identifier.</param>
    /// <param name="q">Optional search string.</param>
    /// <returns></returns>
    [HttpGet("{id}/OtherUsers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult OtherUsers(int id, string q = null)
    {
        IEnumerable<KeyValuePair<string, string>> users;

        var proposal = _db.Proposals.Find(id);

        if (proposal == null)
        {
            return NotFound(new ActionResponse { Result = 1, Message = "Proposal not found." });
        }

        var currentParticipants = proposal.ProposalTeamMembers.Select(x => x.UserId).ToList();

        // Only allowed active users that are not already participants
        IQueryable<User> userQuery = from u in _clientUamService.GetAllowedUsers(proposal.ClientId)
                                     where u.Active
                                     select u;

        if (!string.IsNullOrEmpty(q))
        {
            // Search within the user name
            userQuery = userQuery.Where(x => x.Name.Contains(q));
        }

        // Select the Id and Name only, then convert into KeyValuePair
        users = userQuery
            .Select(x => new
            {
                x.Id,
                x.Name
            })
            .OrderBy(x => x.Name)
            .Take(500)
            .ToList()
            .Select(x => new KeyValuePair<string, string>(x.Id, $"{x.Name} ({x.Id.LanId()})"));

        return Ok(users);
    }

    /// <summary>
    /// Update a proposal.
    /// </summary>
    /// <remarks>
    /// Note: Will also create/delete a client draft if linked/unlinked to the proposal.
    /// </remarks>
    /// <param name="id">Proposal identifier.</param>
    /// <param name="proposal">Proposal details.</param>
    /// <returns></returns>
    [HttpPut("saveevents")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Put([FromBody] SaveEvents eventData)
    {
        if (!_db.Clients.Any(x => x.Id == eventData.ClientId))
        {
            return NotFound(new ActionResponse { Result = 1, Message = "Client not found." });
        }

        if (!_db.Proposals.Any(x => x.Id == eventData.ProposalId))
        {
            return NotFound(new ActionResponse { Result = 1, Message = "Proposal not found." });
        }

        var existingEvents = _db.EventDashboard.FirstOrDefault(x => x.ProposalId == eventData.ProposalId && x.ClientId == eventData.ClientId && x.EventType == eventData.Type);

        if (existingEvents != null)
        {
            // update data
            existingEvents.EventInfo = eventData.EventData;
            existingEvents.IsActive = true;
        }
        else
        {
            // create new data
            var newEventDashboard = new EventDashboard
            {
                ClientId = eventData.ClientId,
                ProposalId = eventData.ProposalId,
                EventInfo = eventData.EventData,
                EventType = eventData.Type,
                IsActive = true,
            };
            _db.EventDashboard.Add(newEventDashboard);
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

        return Ok(new ActionSaveResponse
        {
            Result = 0,
        });
    }

    /// <summary>
    /// Create a new draft client for the proposal.
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="proposalId"></param>
    /// <param name="proposalStatus"></param>
    /// <returns></returns>
    private IActionResult NewClientDraft(int clientId, int proposalId, string proposalStatus)
    {
        // Find the draft client
        var draftClient = _db.ClientDrafts.FirstOrDefault(x => x.ClientId == clientId && !x.IsDeleted);

        // Create the draft if it doesn't exist
        if (draftClient == null)
        {
            // Get the current profile
            var approvedProfile = _db.Clients.Find(clientId);

            // Check profile exists
            if (approvedProfile == null || approvedProfile.IsDeleted)
            {
                return NotFound(new ActionResponse { Result = 1, Message = "Client not found." });
            }

            // Add a new draft
            draftClient = _mapper.Map<ClientDraft>(approvedProfile);
            draftClient.ClientId = clientId;
            draftClient.Status = proposalStatus;   // Set the status to match the proposal status
            draftClient.ProposalId = proposalId; // Add the proposal Id to the draft
            _db.ClientDrafts.Add(draftClient);
        }
        else
        {
            // Add the submission Id to the draft
            draftClient.ProposalId = proposalId;
            _db.Entry(draftClient).Property(p => p.ProposalId).IsModified = true;
        }

        return Ok();
    }
}
