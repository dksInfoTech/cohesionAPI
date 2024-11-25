using System.Dynamic;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Product.Dal;
using Product.Dal.Common.Models;
using Product.Dal.Common.Utils;
using Product.Dal.Common.Workflow;
using Product.Web.Models.Configuration;
using Product.Web.Models.Lookup;
using Product.Web.Models.Response;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Product.Web.Controllers;

public class LookupController : ControllerBase
{
    private readonly DBContext _db;
    private readonly Settings _settings;
    public LookupController(DBContext db, IOptions<Settings> settings)
    {
        _db = db;
        _settings = settings.Value;
    }
    /// <summary>
    /// List of Reference Data.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("api/RefData/All")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult ReferenceData()
    {
        try
        {
            var result = new List<ReferenceDataViewModel>();
            var refDataList = _db.ReferenceData.OrderBy(o => o.RefTypeId).ToList();
            foreach (var r in refDataList)
            {
                switch (r.RefType.RefKey)
                {
                    case "PortfolioMetrics":
                    case "Group":
                    case "ProductLimitTenor":
                    case "LimitTenor":
                    case "Industry":
                    case "Sector":
                    case "DecisionRationales":
                    case "AUDExchangeRateTable":
                        result.Add(new ReferenceDataViewModel
                        {
                            RefDataId = r.Id,
                            RefTypeId = r.RefTypeId,
                            RefKey = r.RefType.RefKey,
                            IsFilteringAllowed = r.RefType.IsFilteringAllowed,
                            RefValue = JsonConvert.DeserializeObject<IList<ExpandoObject>>(r.RefValue)
                        }); break;
                    default:
                        result.Add(new ReferenceDataViewModel
                        {
                            RefDataId = r.Id,
                            RefTypeId = r.RefTypeId,
                            RefKey = r.RefType.RefKey,
                            IsFilteringAllowed = r.RefType.IsFilteringAllowed,
                            RefValue = JsonConvert.DeserializeObject<IEnumerable<string>>(r.RefValue)
                        }); break;
                }
            }
            result.Add(new ReferenceDataViewModel
            {
                RefKey = "ProposalRoles",
                RefValue = ProposalWorkFlowRole.LookUpValues
            });
            result.Add(new ReferenceDataViewModel
            {
                RefKey = "ProposalDecisions",
                RefValue = ProposalWorkFlowDecision.LookUpValues
            });
            result.Add(new ReferenceDataViewModel
            {
                RefKey = "ProposalStatuses",
                RefValue = ProposalWorkFlowStatus.Values
            });
            return Ok(result);
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.Message });
        }
    }

    [HttpPut("api/RefData/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Edit(int id, [FromBody] ReferenceDataViewModel refData)
    {
        if (refData == null)
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "Request is empty." });
        }

        var existingData = _db.ReferenceData.Find(id);
        if (existingData == null)
        {
            return NotFound(new ActionResponse { Result = 1, Message = "ref data doesn't exist." });
        }
        try
        {
            string result = JsonSerializer.Serialize(refData.RefValue);
            existingData.RefValue = result;

            _db.Entry(existingData).State = EntityState.Modified;

            await _db.SaveChangesAsync();
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        }

        return Ok(new ActionResponse { Result = 0 });
    }
}
