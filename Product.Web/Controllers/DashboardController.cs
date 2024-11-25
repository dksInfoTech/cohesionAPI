using AutoMapper;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Product.Dal;
using Product.Dal.Common.Workflow;
using Product.Dal.Entities;
using Product.Dapper.Lib;
using Product.Web.Extensions;
using Product.Web.Models.AGGrid;
using Product.Web.Models.Client;
using Product.Web.Models.Dashboard;
using Product.Web.Models.Proposal;
using Product.Web.Models.Response;
using System.Data;
using System.Linq.Dynamic;
using System.Linq.Dynamic.Core;

namespace Product.Web.Controllers;

[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly DBContext _db;
    private readonly IDapperDbContext _dapperContext;
    private readonly IMapper _mapper;
    private static readonly string get_client_and_proposals_count = "public.get_client_and_proposals_count";

    public DashboardController(DBContext db, IDapperDbContext dapperContext, IMapper mapper)
    {
        _db = db;
        _dapperContext = dapperContext;
        _mapper = mapper;
    }

    /// <summary>
    /// Retrieve collection of clients.
    /// </summary>
    /// <param name="agGridQuery">Query parameters.</param>
    /// <param name="q">Optional search string.</param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult AGGridQuery([FromBody] AGGridQuery agGridQuery, string q = null)
    {
        if (agGridQuery == null)
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "Query is null." });
        }
        else if (agGridQuery.EndRow <= agGridQuery.StartRow)
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "Query EndRow <= StartRow." });
        }

        // Change tracking behavour at context level for the home page query
        _db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        var countQuery = _db.Clients.Where(x => !x.IsDeleted);

        // Get the allowed profiles for the current user
        var query = from x in countQuery
                    join image in _db.Images on x.ImageId equals image.Id into imageJoin
                    from image in imageJoin.DefaultIfEmpty()
                    select new ClientDashboard
                    {
                        Id = x.Id,
                        CreatedDate = x.CreatedDate,
                        ModifiedDate = x.ModifiedDate,
                        CreatedBy = x.CreatedBy,
                        ModifiedBy = x.ModifiedBy,
                        Version = x.Version,
                        ClientName = x.Name,
                        BasicInformation = x.BasicInformation,
                        Image = image,
                        //Proposals = x.Proposals
                        //.Select(s => new ProposalDashboard
                        //{
                        //    Id = s.Id,
                        //    Title = s.Title,
                        //    DateOfEvent = s.ClosedDate.HasValue ? s.ClosedDate.Value : DateTime.Now,
                        //    LastContributedDate = s.LastContributedDate,
                        //    // LastContributorName = s.LastContributorName,
                        //    Status = s.Status,
                        //    Decision = s.Decision,
                        //    IsClientUpdate = s.IsClientUpdate,
                        //    ClientId = s.ClientId,
                        //    ClientName = x.Name,
                        //}),
                        HasOpenProposal = x.Proposals.Any(s => ProposalWorkFlowStatus.OpenValues.Contains(s.Status))
                    };


        // Filter the query using the ag-grid parameters
        query = query.ApplyAGGridFilter(agGridQuery);

        // Sort the query using the ag-grid parameters
        if (agGridQuery.SortModel != null && agGridQuery.SortModel.Any())
        {
            query = query.ApplyAGGridSort(agGridQuery);
        }
        else
        {
            query = query.OrderByDescending(x => x.ClientName);
        }

        // Get the total record count
        int totalRecordCount = query.Count();

        // Cap the number of records returned using the ag-grid parameters
        query = query.ApplyAGGridPaging(agGridQuery);

        // Add the total record count to the header
        Response.Headers.Append("X-Total-Count", totalRecordCount.ToString());

        var records = new List<ClientDashboard> { };

        try
        {
            records = query.ToList();
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = "Query error. Please refresh the page." });
        }

        // foreach (var p in records)
        // {
        //     foreach (var s in p.Submissions)
        //     {
        //         var sub = _submissionRepo.Get(s.Id);
        //         s.LastSubmitterName = sub.LastSubmitterName;
        //         s.Participants = sub.SubmissionParticipants;
        //         var events = sub.SubmissionEvents.Select(x => GetEventType(x)).SelectMany(x => x);
        //         s.SubmissionEventTypes = events?.Distinct();
        //     }
        // }

        return Ok(new DashboardTable
        {
            TotalCount = totalRecordCount,
            Collection = records
        });
    }

    /// <summary>
    /// Retrieve collection of tasks. based on client, if ClientId is 0 then it will return whole dataset
    /// </summary>
    /// <returns></returns>
    [HttpGet("AllTasks/{clientIds}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult GetAllTasks(string clientIds)
    {
        // Define the base query
        var query = _db.ProposalTeamMembers
            .Where(s => !string.IsNullOrEmpty(s.Role) && string.IsNullOrEmpty(s.Decision) && !s.IsFinal)
            .ToList()
            .Select(s => new ProposalTeamMemberViewModel
            {
                Id = s.Id,
                ProposalId = s.ProposalId,
                UserId = s.UserId,
                Title = s.Title,
                Role = s.Role,
                Decision = s.Decision,
                LastDecisionDate = s.LastDecisionDate,
                ExpectedDecisionDate = s.ExpectedDecisionDate,
                ClientId = s.Proposal.ClientId,
                ClientName = s.Proposal.ClientName,
                ClientImage = s.Proposal.Client.Image,
                UserImage = s.UserImage,
                AssignedBy = s.ModifiedBy ?? s.CreatedBy,
            });

        var exceptionQuery = _db.RuleOutcomes
        .Join(_db.Proposals, r => r.SourceId, p => p.Id, (r, p) => new { rule = r, proposal = p })
        .Where(s => s.rule.Status == RuleExceptionStatus.ForApproval)
            .ToList()
            .Select(s => new ProposalTeamMemberViewModel
            {
                Id = s.rule.Id,
                ProposalId = s.proposal.Id,
                UserId = s.rule.ApproverName,
                Title = s.rule.Reason,
                Role = "Exception",
                Decision = s.rule.Status,
                LastDecisionDate = s.proposal.LastContributedDate,
                ExpectedDecisionDate = s.rule.DueDate,
                ClientId = s.proposal.ClientId,
                ClientName = s.proposal.ClientName,
                ClientImage = s.proposal.Client.Image,
                UserImage = s.rule.User.Image,
                AssignedBy = s.rule.ModifiedBy ?? s.rule.CreatedBy,
            });

        var unionQuery = query.Union(exceptionQuery);

        // Apply clientId filter if it's not zero
        if (!string.IsNullOrWhiteSpace(clientIds))
        {
            var clientIdList = clientIds.Split(',').Select(int.Parse).ToList();
            if (clientIdList != null)
            {
                unionQuery = unionQuery.Where(s => clientIdList.Contains(s.ClientId)).ToList();
            }
        }

        return Ok(unionQuery);
    }

    /// <summary>
    /// Retrieve collection of tasks. based on client, if ClientId is 0 then it will return whole dataset
    /// </summary>
    /// <returns></returns>
    [HttpGet("AllPortfolio/{clientIds}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult GetAllPortfolio(string clientIds)
    {
        var clients = (from x in _db.Clients
                       join image in _db.Images on x.ImageId equals image.Id into imageJoin
                       from image in imageJoin.DefaultIfEmpty()
                       where !x.IsDeleted
                       select new { x.BasicInformation, x.FacilityDocuments, x.Id, x.Name, ClientImage = image }).ToList();
        // Retrieve clients based on the clientId condition
        if (!string.IsNullOrWhiteSpace(clientIds))
        {
            var clientIdList = clientIds.Split(',').Select(int.Parse).ToList();
            if (clientIdList != null)
            {
                clients = clients.Where(c => clientIdList.Contains(c.Id)).ToList();
            }
        }

        // Initialize the list to hold portfolio data
        var portfolioDataList = clients.Select(client =>
        {
            if (client == null) return null;

            // Parse basic information from JSON
            var jObject = JObject.Parse(client.BasicInformation);

            // Calculate total limit from active facilities
            double totalLimit = client.FacilityDocuments
                .Where(f => f.IsActive && f.IsLatest)
                .Sum(f =>
                {
                    var jFacilityObject = JObject.Parse(f.FacilityDocumentInfo);
                    return Convert.ToDouble(jFacilityObject["DocumentTotalLimit"]);
                });

            // Create and populate PortfolioDashboard object
            return new PortfolioDashboard
            {
                ClientId = client.Id,
                ClientName = client.Name,
                ClientImage = client.ClientImage,
                CreditRating = jObject["CounterpartyCreditRating"]?.ToString(),
                InternalCreditRating = jObject["RiskGrade"]?.ToString(),
                AnnualReviewDate = jObject["AnnualReviewDate"]?.ToString(),
                NextReviewDate = jObject["NextReviewDate"]?.ToString(),
                TotalLimits = totalLimit,
                Ticker = jObject["Ticker"]?.ToString(),
            };
        })
        .Where(portfolioData => portfolioData != null)
        .ToList();

        // Return the result
        return Ok(portfolioDataList);

    }

    /// <summary>
    /// Retrieve count of clients and open proposals.
    /// </summary>
    /// <returns></returns>
    [HttpGet("Status/Count/{clientIds}/v1")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult GetDashbaordStatusCountV1(string? clientIds)
    {
        var totalClients = 0;
        IQueryable<Proposal>? openProposals = null;

        if (!string.IsNullOrWhiteSpace(clientIds) && clientIds != "undefined")
        {
            var clientIdList = clientIds.Split(',').Select(int.Parse).ToList();
            if (clientIdList != null)
            {
                totalClients = _db.Clients.Count(c => !c.IsDeleted && clientIdList.Contains(c.Id));
                openProposals = _db.Proposals.Where(s => clientIdList.Contains(s.ClientId) && ProposalWorkFlowStatus.OpenValues.Contains(s.Status));
            }
        }
        else
        {
            totalClients = _db.Clients.Count(c => !c.IsDeleted);
            openProposals = _db.Proposals.Where(s => ProposalWorkFlowStatus.OpenValues.Contains(s.Status));

        }

        var proposals = openProposals != null ? _mapper.Map<IQueryable<Proposal>, IList<ProposalViewModel>>(openProposals) : null;

        return Ok(new
        {
            TotalClients = totalClients,
            TotalOpenProposals = proposals?.Count,
            TotalFacilities = 0,
            TotalEntities = 0,
            Facilities = (object)null,
            OpenProposals = proposals
        });
    }

    /// <summary>
    /// Retrieve count of clients and open proposals.
    /// </summary>
    /// <returns></returns>
    [HttpGet("Status/Count/{clientIds}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDashbaordStatusCount(string? clientIds)
    {
        try
        {
            var query = $"SELECT * FROM public.get_client_and_proposals_count('{clientIds}');";
            // var parameters = new DynamicParameters();
            // parameters.Add("@client_Ids", clientIds, dbType: DbType.String, direction: ParameterDirection.Input);

            // var data = await _dapperContext.ExecuteStoreProcedure<dynamic>(get_client_and_proposals_count, parameters);

            var data = await _dapperContext.GetAll<dynamic>(query);
            var res = data.Single();

            return Ok(new
            {
                TotalClients = res.total_clients,
                TotalOpenProposals = res.total_open_proposals,
                TotalFacilities = 0,
                TotalEntities = 0,
                Facilities = (object)null,
                OpenProposals = res.open_proposals != null ? JsonConvert.DeserializeObject<IList<ProposalViewModel>>(res.open_proposals) : null
            });
            // return Ok(data);
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = "Error in Get status counts " + e.Message });
        }
    }


    /// <summary>
    /// Retrieve count of clients based on location.
    /// </summary>
    /// <param name="location">cmfLocation</param>
    /// <returns></returns>
    [HttpGet("Search/Clients/cmfLocation")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult SearchClientsByLocation(string? location = null)
    {
        var userQuery = _db.Clients.Where(c => !c.IsDeleted).ToList();

        if (!string.IsNullOrEmpty(location))
        {
            // Search based on filter 
            var filteredData = userQuery
                    .Where(record =>
                    {
                        var jObject = JObject.Parse(record.BasicInformation);
                        return jObject["Location"]?.ToString() == location;
                    }).ToList();

            var clients = filteredData.OrderBy(x => x.Name);

            return Ok(clients);

        }
        return Ok(BadRequest(new ActionResponse { Result = 1, Message = "Please send cmfLocation" }));
    }

    /// <summary>
    /// Retrieve count of clients based on location.
    /// </summary>
    /// <param name="location">cmfLocation</param>
    /// <returns></returns>
    [HttpGet("Search/Clientdomiciles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult GetClientdomiciles(string? clientIds = null)
    {
        var userQuery = new List<Client>();
        var totalCount = 0;

        if (!string.IsNullOrWhiteSpace(clientIds) && clientIds != "undefined")
        {
            var clientIdList = clientIds.Split(',').Select(int.Parse).ToList();
            if (clientIdList != null)
            {
                userQuery = _db.Clients.Where(c => !c.IsDeleted && clientIdList.Contains(c.Id)).ToList();
                totalCount = userQuery.Count();
            }
        }
        else
        {
            userQuery = _db.Clients.Where(c => !c.IsDeleted).ToList();
            totalCount = userQuery.Count();
        }
        // Search based on filter 
        var groupedData = userQuery
         .Select(record => JObject.Parse(record.BasicInformation))
         .GroupBy(jObject => jObject["Location"]?.ToString())
         .Select(group => new
         {
             Location = group.Key,
             NumberOfClients = group.Count(),
             Percentage = (double)group.Count() / totalCount * 100
         })
         .ToList();

        //var clients = groupedData;
        return Ok(groupedData);
    }

    /// <summary>
    /// Retrieve the list of clients.
    /// </summary>   
    /// <param name="q">Optional search string.</param>
    /// <returns></returns>
    [HttpGet("Search/Clients")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult SearchClients(string? q = null)
    {
        var userQuery = _db.Clients.Where(c => !c.IsDeleted).ToList();

        if (!string.IsNullOrEmpty(q))
        {
            // Search within the client name
            userQuery = userQuery.Where(x => x.Name.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // Select the Id and Name only, then convert into KeyValuePair
        var clients = userQuery
            .OrderBy(x => x.Name)
            .ToDictionary(x => x.Id, x => x.Name);

        return Ok(clients);
    }

    /// <summary>
    /// Retrieve collection of tasks. based on client, if ClientId is 0 then it will return whole dataset
    /// </summary>
    /// <returns></returns>
    [HttpGet("AllEvents/{clientIds}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult GetAllEvents(string clientIds)
    {
        // Define the base query
        var query = _db.EventDashboard
            .Where(s => s.IsActive && s.Proposal.Status != "Closed")
            .ToList()
            .Select(s => new
            {
                Id = s.Id,
                ProposalId = s.ProposalId,
                EventType = s.EventType,
                EventInfo = s.EventInfo,
                ClientId = s.Proposal.ClientId,
                ClientName = s.Proposal.ClientName,
                ClientImage = s.Proposal.Client.Image,
            });

        // Apply clientId filter if it's not zero
        if (!string.IsNullOrWhiteSpace(clientIds))
        {
            var clientIdList = clientIds.Split(',').Select(int.Parse).ToList();
            if (clientIdList != null)
            {
                query = query.Where(c => clientIdList.Contains(c.ClientId)).ToList();              
            }
        }

        return Ok(query);
    }


    //private IEnumerable<string> GetEventType(ProposalEvent proposalEvent)
    //{
    //    var events = new List<string>();
    //    if (proposalEvent.IsNewFacility)
    //    {
    //        events.Add("New Facility");
    //    }
    //    if (proposalEvent.IsLimitTenorChange)
    //    {
    //        events.Add("Limit/Tenor Change");
    //    }
    //    if (proposalEvent.IsRiskGradeReviewEvent)
    //    {
    //        events.Add("Risk Grade Review Event");
    //    }
    //    if (proposalEvent.IsConditionsCovenantsSecurity)
    //    {
    //        events.Add("Conditions, Covenants & Security");
    //    }
    //    if (proposalEvent.IsFinancialCovenantMonitoring)
    //    {
    //        events.Add("Financial/Covenant Monitoring");
    //    }
    //    if (proposalEvent.IsCovenantResults)
    //    {
    //        events.Add("Covenant Result");
    //    }
    //    if (proposalEvent.IsOther)
    //    {
    //        events.Add($"Other: {proposalEvent.OtherDecisionType}");
    //    }
    //    if (proposalEvent.IsDcaUtilisation)
    //    {
    //        events.Add("DCA Utilisation");
    //    }
    //    return events;
    //}
}
