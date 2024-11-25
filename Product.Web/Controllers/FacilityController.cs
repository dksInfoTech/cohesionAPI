using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
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
using Product.Web.Models.Facility;
using Product.Web.Models.Proposal;
using Product.Web.Models.Response;

namespace Product.Web.Controllers;

public class FacilityController : ControllerBase
{
    private readonly DBContext _db;
    private readonly IFacilityService _facilityService;

    public FacilityController(DBContext db, IFacilityService facilityService)
    {
        _db = db;
        _facilityService = facilityService;

    }

    /// <summary>
    /// Retrieve facilties details.
    /// </summary>
    /// <param name="clientId">client identifier.</param>
    /// <param name="proposalId">Proposal identifier.</param>
    /// <returns></returns>
    [HttpGet("api/Facility/Client/{clientId}/Proposal/{proposalId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GetFacilities(int clientId, int proposalId)
    {
        var facilities = _db.Facilities.Where(s => s.ClientId == clientId && s.IsActive);
        if (facilities.Any(f => f.ProposalId == proposalId))
        {
            facilities = facilities.Where(s => s.ProposalId == proposalId && s.IsDraft);
        }
        else
        {
            facilities = facilities.Where(s => s.IsLatest);
        }

        return Ok(facilities.ToList());
    }

    /// <summary>
    /// Retrieve facilties details.
    /// </summary>
    /// <param name="clientId">client identifier.</param>
    /// <param name="proposalId">Proposal identifier.</param>
    /// <returns></returns>
    [HttpGet("api/LST/Client/{clientId}/Proposal/{proposalId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GetLST(int clientId, int proposalId)
    {
        var proposal = _db.Proposals.Find(proposalId);
        if (proposal == null)
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "Request proposal not available." });
        }

        var facilities = _db.Facilities.Where(s => s.ClientId == clientId && s.IsActive);
        if (facilities.Any(f => f.ProposalId == proposalId))
        {
            facilities = proposal.Status == ProposalStatus.Closed.ToDescription() ? facilities.Where(s => s.ProposalId == proposalId) :
                        facilities.Where(s => s.ProposalId == proposalId && s.IsDraft);
        }
        else
        {
            facilities = facilities.Where(s => s.IsLatest);
        }

        var configResult = _db.Configurations.Where(x => x.ConfigType == ConfigurationType.LSTConfig.ToDescription()).ToList();

        List<Item> lstList = new List<Item>();

        if (configResult.Count() > 0)
        {
            var configList = configResult[0].ConfigInfo;
            var items = JsonConvert.DeserializeObject<List<Item>>(configResult[0].ConfigInfo);


            foreach (var item in items)
            {
                // do a groupby facilities with each item.name
                var groupedFacilities = facilities.ToList().GroupBy(record =>
                                {
                                    var jObject = JObject.Parse(record.FacilityInfo);
                                    return jObject[item.name]?.ToString(); // Grouping by the specified property
                                })
                                .ToList();

                List<LSTItem> lstDataList = new List<LSTItem>();

                // consolidate as per item name to all facility data, below data will  be in loop
                foreach (var grp in groupedFacilities)
                {
                    LSTItem lstData = new LSTItem();
                    lstData.Type = grp.Key;

                    decimal totalApprovedLimit = 0;
                    decimal totalProposedLimit = 0;

                    foreach (var facility in grp)
                    {
                        try
                        {
                            var jObject = JsonConvert.DeserializeObject<LSTFacility>(facility.FacilityInfo);

                            totalApprovedLimit += jObject.ApprovedLimit;
                            totalProposedLimit += jObject.ProposedLimit;

                            lstData.ApprovedLimit = totalApprovedLimit.ToString();
                            lstData.ProposedLimit = totalProposedLimit.ToString();
                            lstData.Currency = jObject.CurrencyList;

                            var filterDateFromTenor = jObject.Tenor.Where(s => s.TenorType != "Date").ToList();

                            foreach (var timeband in filterDateFromTenor)
                            {
                                var existing = lstData.TenorList.FirstOrDefault(t => t.TenorType == timeband.TenorType && t.TenorTimeband == timeband.TenorTimeband);
                                if (existing != null)
                                {
                                    existing.TenorLimit += timeband.TenorLimit;
                                }
                                else
                                {
                                    lstData.TenorList.Add(timeband);
                                }
                            }
                            lstData.TotalLimit = "";

                        }
                        catch (Exception ex)
                        {

                        }
                    }

                    item.LSTData.Add(lstData);
                }
                lstList.Add(item);
            }
        }

        return Ok(lstList);
    }

    /// <summary>
    /// Retrieve facilties doc details.
    /// </summary>
    /// <param name="clientId">client identifier.</param>
    /// <param name="proposalId">Proposal identifier.</param>
    /// <returns></returns>
    [HttpGet("api/FacilityDocument/Client/{clientId}/Proposal/{proposalId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GetFacilityDocuments(int clientId, int proposalId)
    {
        var facilityDocs = _db.FacilityDocuments.Where(s => s.ClientId == clientId && s.IsActive);
        if (facilityDocs.Any(f => f.ProposalId == proposalId))
        {
            facilityDocs = facilityDocs.Where(s => s.ProposalId == proposalId && s.IsDraft);
        }
        else
        {
            facilityDocs = facilityDocs.Where(s => s.IsLatest);
        }

        return Ok(facilityDocs.ToList());
    }

    /// <summary>
    /// Retrieve interchangable limit details.
    /// </summary>
    /// <param name="clientId">client identifier.</param>
    /// <param name="proposalId">Proposal identifier.</param>
    /// <returns></returns>
    [HttpGet("api/Interchangable/Client/{clientId}/Proposal/{proposalId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult Get(int clientId, int proposalId)
    {
        var interchangableLimits = _db.InterchangableLimits.Where(s => s.ClientId == clientId && s.IsActive);
        if (interchangableLimits.Any(f => f.ProposalId == proposalId))
        {
            interchangableLimits = interchangableLimits.Where(s => s.ProposalId == proposalId && s.IsDraft);
        }
        else
        {
            interchangableLimits = interchangableLimits.Where(s => s.IsLatest);
        }

        return Ok(interchangableLimits.ToList());
    }

    /// <summary>
    /// Retrieve All facilties details.
    /// </summary>
    /// <returns></returns>
    [HttpGet("api/Facility/Allfacilities")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GetAllfacilities()
    {
        var facilities = from s in _db.Facilities
                         where s.IsActive && s.IsLatest
                         select new FacilityViewModel
                         {
                             ClientName = s.Client.Name,
                             ClientId = s.Client.Id,
                             ProposalTitle = s.Proposal.Title,
                             ProposalId = s.ProposalId,
                             ProposalStatus = s.Proposal.Status,
                             FacilityInfo = s.FacilityInfo,
                         };


        return Ok(facilities.ToList());
    }

    /// <summary>
    /// Retrieve the facilities for a customer.
    /// </summary>
    /// <param name="clientId">Client identifier.</param>
    /// <returns></returns>
    [HttpGet("api/Facility/ByClient/{clientId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetFacilitiesByClient(int clientId)
    {
        var result = from s in _db.Facilities
                     where s.ClientId == clientId && s.IsLatest && s.IsActive
                     select s;

        return Ok(result);
    }

    /// <summary>
    /// Retrieve the facility documents for a customer.
    /// </summary>
    /// <param name="clientId">Client identifier.</param>
    /// <returns></returns>
    [HttpGet("api/FacilityDocument/ByClient/{clientId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetFacilityDocumentsByClient(int clientId)
    {
        var result = from s in _db.FacilityDocuments
                     where s.ClientId == clientId && s.IsLatest && s.IsActive
                     select s;

        return Ok(result);
    }

    /// <summary>
    /// Retrieve the interchangable limits for a customer.
    /// </summary>
    /// <param name="clientId">Client identifier.</param>
    /// <returns></returns>
    [HttpGet("api/InterchangableLimit/ByClient/{clientId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetInterchangableLimitsByClient(int clientId)
    {
        var result = from s in _db.InterchangableLimits
                     where s.ClientId == clientId && s.IsLatest && s.IsActive
                     select s;

        return Ok(result);
    }

    /// <summary>
    /// delete facility
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("api/Facility/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteFacility(int id)
    {
        var deletefac = _db.Facilities.Find(id);

        if (deletefac == null)
        {
            return NotFound(new ActionResponse { Result = 1, Message = "Not found." });
        }

        _db.Facilities.Remove(deletefac);

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
    /// Create a new facility
    /// </summary>
    /// <param name="facility"></param>
    /// <returns></returns>
    [HttpPost("api/Facility")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SaveFacility([FromBody] SaveFacilityRequest facility)
    {
        if (facility == null)
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "Request cannot be null." });
        }
        Facility newFacility;
        try
        {
            newFacility = await _facilityService.SaveFacilityDraft(facility);
        }

        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.Message });
        }

        return Ok(new ActionSaveResponse
        {
            Result = 0,
            ModelId = newFacility.Id,
            ModifiedBy = newFacility.ModifiedBy,
            ModifiedDate = newFacility.ModifiedDate,
            ModifiedDateString = string.Format("{0:dd-MMM-yyyy HH:mm:ss}", newFacility.ModifiedDate)
        });
    }

    /// <summary>
    /// Update a facility
    /// </summary>
    /// <param name="facility"></param>
    /// <returns></returns>
    [HttpPut("api/Facility/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateFacility(int id, [FromBody] SaveFacilityRequest facility)
    {
        if (facility == null)
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "Request cannot be null." });
        }

        var editFacility = _db.Facilities.Find(id);

        if (editFacility == null)
        {
            return NotFound(new ActionResponse { Result = 1, Message = "Not found." });
        }

        try
        {
            editFacility = await _facilityService.SaveFacilityDraft(facility);
        }

        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.Message });
        }

        return Ok(new ActionSaveResponse
        {
            Result = 0,
            ModelId = editFacility.Id,
            ModifiedBy = editFacility.ModifiedBy,
            ModifiedDate = editFacility.ModifiedDate,
            ModifiedDateString = string.Format("{0:dd-MMM-yyyy HH:mm:ss}", editFacility.ModifiedDate)
        });
    }

    /// <summary>
    /// delete facility doc
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("api/FacilityDocument/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteFacilityDocument(int id)
    {
        var deletefacDoc = _db.FacilityDocuments.Find(id);

        if (deletefacDoc == null)
        {
            return NotFound(new ActionResponse { Result = 1, Message = "Not found." });
        }

        _db.FacilityDocuments.Remove(deletefacDoc);

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
    /// Create a new facility document
    /// </summary>
    /// <param name="facDocument"></param>
    /// <returns></returns>
    [HttpPost("api/FacilityDocument")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SaveFacilityDocument([FromBody] SaveFacilityDocumentRequest facDocument)
    {
        if (facDocument == null)
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "Request cannot be null." });
        }
        FacilityDocument newFacilityDoc;
        try
        {
            newFacilityDoc = await _facilityService.SaveFacilityDocumentDraft(facDocument);
        }

        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.Message });
        }

        return Ok(new ActionSaveResponse
        {
            Result = 0,
            ModelId = newFacilityDoc.Id,
            ModifiedBy = newFacilityDoc.ModifiedBy,
            ModifiedDate = newFacilityDoc.ModifiedDate,
            ModifiedDateString = string.Format("{0:dd-MMM-yyyy HH:mm:ss}", newFacilityDoc.ModifiedDate)
        });
    }

    /// <summary>
    /// Update a facility document
    /// </summary>
    /// <param name="facDocument"></param>
    /// <returns></returns>
    [HttpPut("api/FacilityDocument/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateFacilityDocument(int id, [FromBody] SaveFacilityDocumentRequest facDocument)
    {
        if (facDocument == null)
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "Request cannot be null." });
        }

        var editFacilityDoc = _db.FacilityDocuments.Find(id);

        if (editFacilityDoc == null)
        {
            return NotFound(new ActionResponse { Result = 1, Message = "Not found." });
        }

        try
        {
            editFacilityDoc = await _facilityService.SaveFacilityDocumentDraft(facDocument);
        }

        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.Message });
        }

        return Ok(new ActionSaveResponse
        {
            Result = 0,
            ModelId = editFacilityDoc.Id,
            ModifiedBy = editFacilityDoc.ModifiedBy,
            ModifiedDate = editFacilityDoc.ModifiedDate,
            ModifiedDateString = string.Format("{0:dd-MMM-yyyy HH:mm:ss}", editFacilityDoc.ModifiedDate)
        });
    }

    /// <summary>
    /// delete Interchangable limit
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("api/InterChangableLimit/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteInterChangableLimit(int id)
    {
        var interchangableLimit = _db.InterchangableLimits.Find(id);

        if (interchangableLimit == null)
        {
            return NotFound(new ActionResponse { Result = 1, Message = "Not found." });
        }

        _db.InterchangableLimits.Remove(interchangableLimit);

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
    /// Create a new IL
    /// </summary>
    /// <param name="interchangableLimitRequest"></param>
    /// <returns></returns>
    [HttpPost("api/InterchangableLimit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SaveInterchangableLimit([FromBody] SaveInterchangableLimitRequest interchangableLimitRequest)
    {
        if (interchangableLimitRequest == null)
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "Request cannot be null." });
        }
        InterchangableLimit newIL;
        try
        {
            newIL = await _facilityService.SaveInterchangableLimitsDraft(interchangableLimitRequest);
        }

        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.Message });
        }

        return Ok(new ActionSaveResponse
        {
            Result = 0,
            ModelId = newIL.Id,
            ModifiedBy = newIL.ModifiedBy,
            ModifiedDate = newIL.ModifiedDate,
            ModifiedDateString = string.Format("{0:dd-MMM-yyyy HH:mm:ss}", newIL.ModifiedDate)
        });
    }

    /// <summary>
    /// Update a IL
    /// </summary>
    /// <param name="interchangableLimitRequest"></param>
    /// <returns></returns>
    [HttpPut("api/InterchangableLimit/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateInterchangableLimit(int id, [FromBody] SaveInterchangableLimitRequest interchangableLimitRequest)
    {
        if (interchangableLimitRequest == null)
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "Request cannot be null." });
        }

        var editInterchangableLimitRequest = _db.InterchangableLimits.Find(id);

        if (editInterchangableLimitRequest == null)
        {
            return NotFound(new ActionResponse { Result = 1, Message = "Not found." });
        }

        try
        {
            editInterchangableLimitRequest = await _facilityService.SaveInterchangableLimitsDraft(interchangableLimitRequest);
        }

        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.Message });
        }

        return Ok(new ActionSaveResponse
        {
            Result = 0,
            ModelId = editInterchangableLimitRequest.Id,
            ModifiedBy = editInterchangableLimitRequest.ModifiedBy,
            ModifiedDate = editInterchangableLimitRequest.ModifiedDate,
            ModifiedDateString = string.Format("{0:dd-MMM-yyyy HH:mm:ss}", editInterchangableLimitRequest.ModifiedDate)
        });
    }
}

internal class Item
{
    public string name { get; set; }

    public List<LSTItem> LSTData { get; set; } = new List<LSTItem>();
}

internal class LSTItem
{
    public string Type { get; set; }

    public string ApprovedLimit { get; set; }
    public string Currency { get; set; }

    public string ProposedLimit { get; set; }

    public List<Tenor> TenorList { get; set; } = new List<Tenor>();

    public string TotalLimit { get; set; }
}

internal class LSTFacility
{
    public string FacilityName { get; set; }
    public string Product { get; set; }
    public string RiskGrade { get; set; }
    public string CountryOfRisk { get; set; }
    public string FacilityCommitted { get; set; }
    public string Advised { get; set; }
    public string RiskMitigation { get; set; }
    public string CurrencyList { get; set; }
    public string MultiCurrencyList { get; set; }
    public decimal ProposedLimit { get; set; }
    public decimal ApprovedLimit { get; set; }
    public string LimitType { get; set; }
    public DateTime FacilityStartDate { get; set; }
    public string TerminationDate { get; set; }
    public List<Tenor> Tenor { get; set; }
}

internal class Tenor
{
    public string TenorType { get; set; }
    public int? TenorTimeband { get; set; }
    public string TenorEndDate { get; set; }
    public decimal? TenorLimit { get; set; }
    public string TenorCurrency { get; set; }
}