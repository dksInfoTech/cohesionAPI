using System.Linq;
using System.Xml.Linq;
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
using Product.Web.Models.Response;

namespace Product.Web.Controllers;

/// <summary>
/// Defines API methods for a client
/// </summary>
[Route("api/[controller]")]
public class ClientController : ControllerBase
{
    private readonly DBContext _db;
    private readonly IMapper _mapper;
    private readonly UserContextService _userContextService;
    private readonly IRuleSetService _ruleSetService;

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="db"></param>
    /// <param name="mapper"></param>
    /// <param name="userContextService"></param>
    /// <param name="ruleSetService"></param>
    public ClientController(DBContext db, IMapper mapper, UserContextService userContextService, IRuleSetService ruleSetService)
    {
        _db = db;
        _mapper = mapper;
        _userContextService = userContextService;
        _ruleSetService = ruleSetService;
    }

    /// <summary>
    /// Get a client by Id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="v">Optional version number.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Get(int id, int? v = null)
    {
        if (id <= 0)
        {
            return BadRequest("Invalid client id");
        }
        // Current client
        ClientViewModel client = _mapper.Map<ClientViewModel>(await _db.Clients.FindAsync(id));
        try
        {
            if (client != null)
            {
                // Check client exists
                if (client == null || client.IsDeleted)
                {
                    return NotFound(new ActionResponse { Result = 1, Message = "Not found." });
                }

                // Optionally get the historical client version
                if (v != null && client.Version != v)
                {
                    // Find the requested client
                    var history = _db.ClientHistory.FirstOrDefault(x => x.ClientId == id && x.Version == v);

                    // Check client exists
                    if (history == null || history.IsDeleted)
                    {
                        return NotFound(new ActionResponse { Result = 1, Message = "Not found." });
                    }

                    // Map to ActionResponse object
                    client = _mapper.Map<ClientViewModel>(history);
                }

                // Get the prior historical client version for comparison/diff
                if (client.Version > 1)
                {
                    // Find the client
                    var history = _db.ClientHistory.FirstOrDefault(x => x.ClientId == id && x.Version == client.Version - 1);

                    // Check client exists
                    if (history != null && !history.IsDeleted)
                    {
                        // Map to client object for the view to render
                        client.DiffClient = _mapper.Map<Client>(history);
                    }
                }
                var draft = _db.ClientDrafts.FirstOrDefault(x => x.ClientId == id && !x.IsDeleted);
                client.DraftProposalId = draft?.ProposalId;
                client.HasDraft = client.DraftProposalId != null;
                client.ProposalStatus = draft?.Status;
            }

            return Ok(client);
        }
        catch (Exception ex)
        {

            throw ex;
        }
    }


    /// <summary>
    /// Get a client by Id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="v">Optional version number.</param>
    /// <returns></returns>
    [HttpGet("{id}/{proposalId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetClientHistory(int id, int proposalId)
    {
        if (id <= 0 || proposalId <= 0)
        {
            return BadRequest("Invalid client or proposal id");
        }
        // Current client
        ClientViewModel client = _mapper.Map<ClientViewModel>(_db.ClientHistory.FirstOrDefault(x => x.ClientId == id && x.ProposalId == proposalId));

        return Ok(client);
    }

    /// <summary>
    /// Create a new client
    /// </summary>
    /// <param name="name"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("{name}/create")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Create(string name, [FromBody] SaveClientRequest request)
    {
        if (request == null)
        {
            return BadRequest("Request can't be null");
        }
        if (!string.IsNullOrWhiteSpace(name) && _db.Clients.Any(x => x.Name == name))
        {
            return BadRequest("A client with the same name already exists.");
        }

        var newClient = new Client
        {
            Name = name,
            BasicInformation = request.BasicInformation,
            OtherInformation = request.OtherInformation,
            TemplateId = request.TemplateId,
            ImageId = request.ImageId,
        };

        _db.Clients.Add(newClient);

        try
        {
            await _db.SaveChangesAsync();

            var clientUserAccess = new ClientUserAccess
            {
                UserId = _userContextService.GetUserId(),
                ClientId = newClient.Id,
                Admin = true
            };

            _db.ClientUserAccessMapping.Add(clientUserAccess);

            await _db.SaveChangesAsync();

            var newTeamMember = new ClientTeamMember
            {
                ClientUserMapId = clientUserAccess.Id,
                ClientId = newClient.Id,
                ProposalRole = ProposalRole.ToFinalise.ToDescription()
            };

            // Create the team member
            _db.ClientTeamMembers.Add(newTeamMember);
            await _db.SaveChangesAsync();

            return Ok(new ActionSaveResponse
            {
                Result = 0,
                ModelId = newClient.Id,
                ModifiedBy = newClient.ModifiedBy,
                ModifiedDate = newClient.ModifiedDate,
                ModifiedDateString = string.Format("{0:dd-MMM-yyyy HH:mm:ss}", newClient.ModifiedDate),
            });
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        }

    }

    /// <summary>
    /// Update an existing client
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("{id}/edit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Edit(int id, [FromBody] SaveClientRequest request)
    {
        if (request == null)
        {
            return BadRequest("Request can't be null");
        }

        var existingClient = _db.Clients.Find(id);

        if (existingClient == null)
        {
            return BadRequest("Client not found.");
        }

        existingClient.BasicInformation = request.BasicInformation;
        existingClient.OtherInformation = request.OtherInformation;

        try
        {
            await _db.SaveChangesAsync();
            return Ok(existingClient);
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        }
    }

    /// <summary>
    /// Update an existing client
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("{id}/updateimage")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> EditImage(int id, int imageId)
    {
        var existingClient = _db.Clients.Find(id);

        if (existingClient == null)
        {
            return BadRequest("Client not found.");
        }

        existingClient.ImageId = imageId;

        try
        {
            await _db.SaveChangesAsync();
            return Ok(existingClient);
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        }
    }

    /// <summary>
    /// Retrieve the list of team members with explicit access to the client.
    /// </summary>
    /// <param name="id">client identifier.</param>
    /// <returns></returns>
    [HttpGet("{id}/TeamMembers")]
    public new ActionResult ClientTeamMembers(string id)
    {
        var clientIdList = new List<int>();
        // Apply filter if id is not 0
        if (!string.IsNullOrWhiteSpace(id))
        {
            clientIdList = id.Split(',').Select(int.Parse).ToList();
        }

        var teamMembers = from member in _db.ClientTeamMembers
                          join client in _db.Clients on member.ClientId equals client.Id
                          where clientIdList == null || clientIdList.Contains(member.ClientId)
                          select new
                          {
                              member.UserId,
                              Name = member.UserName,
                              Country = member.UserCountry,
                              Image = member.UserImage,
                              Role = member.UserRole,
                              member.ProposalRole,
                              member.ClientId,
                              clientName = client.Name,
                              clientImage = client.Image
                          };

        return Ok(teamMembers.ToList());
    }

    /// <summary>
    /// Grant a user explicit access to the client.
    /// </summary>
    /// <param name="id">client identifier.</param>
    /// <param name="teamMember">User access information.</param>
    /// <returns></returns>
    [HttpPost("{id}/TeamMember")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> AddClientTeamMember(int id, [FromBody] SaveClientTeamMemberRequest teamMember)
    {
        if (teamMember == null || string.IsNullOrWhiteSpace(teamMember.UserId))
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "Request is empty." });
        }

        var userProfile = _db.ClientUserAccessMapping.FirstOrDefault(x => x.UserId == teamMember.UserId && x.ClientId == id);
        var clientUserAccess = new ClientUserAccess
        {
            UserId = teamMember.UserId,
            ClientId = id,
            Admin = true
        };
        if (userProfile == null)
        {
            _db.ClientUserAccessMapping.Add(clientUserAccess);
            await _db.SaveChangesAsync();
        }

        var newTeamMember = new ClientTeamMember
        {
            ClientUserMapId = userProfile != null ? userProfile.Id : clientUserAccess.Id,
            ClientId = id
        };
        //if (!string.IsNullOrWhiteSpace(teamMember.ProposalRole) && teamMember.ProposalRole != ProposalRole.Undecided.ToDescription())
        newTeamMember.ProposalRole = "TeamMember";
        // Create the team member
        _db.ClientTeamMembers.Add(newTeamMember);

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

    /// <summary>
    /// Update a user's explicit access to the client.
    /// </summary>
    /// <param name="id">client identifier.</param>
    /// <param name="teamMember">User access information.</param>
    /// <returns></returns>
    [HttpPut("{id}/TeamMember")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> EditClientTeamMember(int id, [FromBody] SaveClientTeamMemberRequest teamMember)
    {
        if (teamMember == null || string.IsNullOrEmpty(teamMember.UserId))
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "Request is empty" });
        }

        var existingTeamMember = _db.ClientTeamMembers.FirstOrDefault(x => x.ClientUserMap.ClientId == id && x.ClientUserMap.UserId == teamMember.UserId);
        if (existingTeamMember == null)
        {
            return NotFound(new ActionResponse { Result = 1, Message = "Member doesn't exist." });
        }

        // Update the changed properties
        existingTeamMember.ProposalRole = teamMember.ProposalRole;

        _db.Entry<ClientTeamMember>(existingTeamMember).State = EntityState.Modified;
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
    /// Remove a user's explicit access to the client.
    /// </summary>
    /// <param name="id">client identifier.</param>
    /// <param name="teamMemberId">User identifier.</param>
    /// <returns></returns>
    [HttpDelete("{id}/TeamMember")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> RemoveClientTeamMember(int id, [FromBody] string teamMemberId)
    {
        if (string.IsNullOrEmpty(teamMemberId))
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "Client Team Member Id is blank." });
        }

        var teamMember = _db.ClientTeamMembers.FirstOrDefault(x => x.ClientUserMap.ClientId == id && x.ClientUserMap.UserId == teamMemberId);
        if (teamMember == null)
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "Member doesn't exist." });
        }

        _db.ClientTeamMembers.Remove(teamMember);

        _db.ClientUserAccessMapping.RemoveRange(_db.ClientUserAccessMapping.Where(x => x.ClientId == id && x.UserId == teamMemberId));

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
    /// <summary>
    /// Retrieve the list of members without access to the client.
    /// </summary>   
    /// <param name="id">client identifier.</param>
    /// <param name="includeAll">enabled search through all client members.</param>
    /// <param name="q">Optional search string.</param>
    /// <param name="proposalId">Optional proposal id.</param>
    /// <returns></returns>
    [HttpGet("{id}/TeamMember/Search/{includeAll}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult SearchTeamMember(int id, bool includeAll, string? q = null, int? proposalId = null)
    {
        // Only allowed active users that are not already linked to the client
        var userQuery = _db.ClientUserAccessMapping.Where(c => c.ClientId == id && (c.Write || c.Approve || c.Admin)).ToList();

        if (!string.IsNullOrEmpty(q))
        {
            // Search within the user name
            userQuery = userQuery.Where(x => x.User.Name.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (userQuery == null || !userQuery.Any())
        {
            return GetUser(q);
        }

        if (!includeAll && proposalId.HasValue)
        {
            var existingTeamMembers = _db.ProposalTeamMembers
                                        .Where(x => x.Proposal.ClientId == id
                                        && x.ProposalId == proposalId.Value
                                        && x.Proposal.Status != ProposalStatus.Closed.ToDescription())
                                        .Select(s => s.UserId).ToList();
            userQuery = userQuery.Where(u => !existingTeamMembers.Contains(u.UserId)).ToList();
        }

        // Select the Id and Name only, then convert into KeyValuePair
        var users = userQuery
            .Select(x => new
            {
                Id = x.UserId,
                x.User.Name,
                x.User.Image
            })
            .OrderBy(x => x.Name)
            .Take(100)
            .ToList()
            .Select(x => new KeyValuePair<string, object[]>(x.Id, [x.Name, x.Image]));

        return Ok(users);
    }

    private ActionResult GetUser(string? q = null)
    {
        var userQuery = _db.Users.Where(x => x.Active).ToList();

        if (!string.IsNullOrEmpty(q))
        {
            // Search within the user name
            userQuery = userQuery.Where(x => x.Name.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // Select the Id and Name only, then convert into KeyValuePair
        var users = userQuery
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Image
            })
            .OrderBy(x => x.Name)
            .Take(100)
            .ToList()
            .Select(x => new KeyValuePair<string, object[]>(x.Id, [x.Name, x.Image]));

        return Ok(users);
    }

    /// <summary>
    /// Update a draft client.
    /// </summary>
    /// <param name="id">client identifier.</param>
    /// <param name="client"></param>
    /// <returns></returns>
    [HttpPut("{id}/Draft/Edit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Put(int id, [FromBody] SaveClientRequest client)
    {
        if (client == null)
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "Request cannot be null." });
        }

        var existingClientDraft = _db.ClientDrafts.FirstOrDefault(x => x.ClientId == id && !x.IsDeleted);

        if (existingClientDraft == null)
        {
            return NotFound(new ActionResponse { Result = 1, Message = $"Draft not found ({id})" });
        }
        // IEnumerable<RuleResponse>? outcome = null;
        existingClientDraft.BasicInformation = client.BasicInformation;
        existingClientDraft.OtherInformation = client.OtherInformation;
        if (ProposalWorkFlowStatus.Values.Any(x => x.Equals(client.ProposalStatus)) && !string.IsNullOrWhiteSpace(client.ProposalStatus))
        {
            existingClientDraft.Status = client.ProposalStatus;
        }

        try
        {
            await _db.SaveChangesAsync();
            // outcome = _ruleSetService.Trigger(existingClientDraft.BasicInformation, "Proposal");
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.Message });
        }

        return Ok(new ActionSaveResponse
        {
            Result = 0,
            ModelId = existingClientDraft.ClientId,
            ModifiedBy = existingClientDraft.ModifiedBy,
            ModifiedDate = existingClientDraft.ModifiedDate,
            ModifiedDateString = string.Format("{0:dd-MMM-yyyy HH:mm:ss}", existingClientDraft.ModifiedDate),
        });
    }

    /// <summary>
    /// Update an existing EntityHierarchy or create new
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("CreateEntityHierarchy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> CreateEntityHierarchy([FromBody] SaveEntityHierarchy request)
    {
        if (request == null)
        {
            return BadRequest("Request can't be null");
        }

        try
        {
            if (request.Id == 0)
            {
                // create new record
                var newHierarchy = new EntityHierarchy
                {
                    ClientId = request.ClientId,
                    EntityHierarchyInfo = request.EntityHierarchyInfo,
                    Type = request.Type,
                    IsLatest = true,
                    IsActive = true,
                    ChildIds = request.ChildIds
                };

                _db.EntityHierarchy.Add(newHierarchy);
                await _db.SaveChangesAsync();
                return Ok(newHierarchy);
            }
            else
            {
                // update
                var existingEntity = _db.EntityHierarchy.Find(request.Id);

                existingEntity.ClientId = request.ClientId;
                existingEntity.EntityHierarchyInfo = request.EntityHierarchyInfo;
                existingEntity.Type = request.Type;
                existingEntity.IsLatest = true;
                existingEntity.IsActive = true;
                existingEntity.ChildIds = request.ChildIds;

                await _db.SaveChangesAsync();
                return Ok(existingEntity);

            }
        }

        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        }
    }

    /// <summary>
    /// get an existing EntityHierarchy
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("GetEntityHierarchy/{clientId}")]
    public IActionResult GetEntityHierarchy(int clientId)
    {
        if (clientId == 0)
        {
            return BadRequest("Invalid client id");
        }

        // Find the requested client
        var result = _db.EntityHierarchy.Where(x => (x.ClientId == clientId || x.ChildIds.Contains(clientId.ToString())) && x.IsActive).ToList();

        return Ok(result);
    }
}
