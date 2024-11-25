using Microsoft.AspNetCore.Mvc;
using Product.Bal.Interfaces;
using Product.Dal;
using Product.Dal.Common.Extensions;
using Product.Dal.Entities;
using Product.Dal.Enums;
using Product.Dal.Interfaces;
using Product.Web.Models.Response;
using Product.Web.Models.Template;

namespace Product.Web.Controllers;

[Route("api/[controller]")]
public class TemplateController : ControllerBase
{
    private readonly DBContext _db;
    private readonly ITemplateService _templateService;
    private readonly IRuleSetService _ruleSetService;

    public TemplateController(DBContext db, ITemplateService templateService, IRuleSetService ruleSetService)
    {
        _db = db;
        _templateService = templateService;
        _ruleSetService = ruleSetService;
    }

    /// <summary>
    /// Get the metadata associated with a client.
    /// </summary>
    /// <param name="proposalId">Optional proposal identifier.</param>
    /// <param name="clientId">Optional client identifier.</param>
    /// <param name="clientVersion">Optional client version.</param>
    /// <returns></returns>
    [HttpGet]
    [Route("Client")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult GetClientTemplateMetadata(int? proposalId = null, int? clientId = null, int? clientVersion = null)
    {
        Client client = null;
        ITemplateDefinition metadata;

        // Get the required client based on the proposal Id or client Id and version
        if (proposalId.HasValue)
        {
            var proposal = _db.Proposals.Find(proposalId);

            if (proposal != null)
            {
                client = _db.Clients.Find(proposal.ClientId);
            }
        }
        else if (clientId.HasValue)
        {
            client = _db.Clients.Find(clientId.Value);
        }
        else
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "A proposal Id or client Id is required." });
        }

        // Check client exists
        if (client == null)
        {
            return NotFound(new ActionResponse { Result = 1, Message = "Not found." });
        }

        try
        {
            var outcomes = _ruleSetService.Trigger(client.BasicInformation, "Proposal", isTemplateAssignment: true);
            var passedRule = outcomes.FirstOrDefault(o => o.Status == "Failed");
            // Get the specific metadata for the profile
            metadata = _templateService.Get(client, passedRule != null ? passedRule?.TemplateId : (int?)null);
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.Message });
        }

        return Ok(metadata?.TemplateData);
    }

    /// <summary>
    /// Retrieve template details.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("All")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult GetAll(bool applyRuleFilter = false)
    {
        try
        {
            var templates = _db.Templates
                .Select(s => new TemplateViewModel
                {
                    Id = s.Id,
                    TemplateName = s.TemplateName,
                    TemplateData = s.TemplateData,
                    Version = s.Version,
                    Description = s.Description
                });

            if (applyRuleFilter)
            {
                var templateIds = _db.RuleTriggers
                                    .Where(x => x.Category == RuleCategory.TemplateAssignment.ToDescription())
                                    .Select(t => t.TemplateId);
                templates = templates.Where(t => !templateIds.Contains(t.Id));
            }

            return Ok(templates);
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.Message });
        }
    }
}
