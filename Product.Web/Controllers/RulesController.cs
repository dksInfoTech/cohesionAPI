using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Product.Bal.Interfaces;
using Product.Dal;
using Product.Web.Models.Response;
using Product.Dal.Entities;
using Product.Web.Models.Rule;

namespace Product.Web.Controllers;
public class RulesController : ControllerBase
{
	private readonly DBContext _db;
	private readonly ITemplateService _templateService;
	private readonly IRuleSetService _ruleSetService;


	public RulesController(DBContext db, ITemplateService templateService, IRuleSetService ruleSetService)
	{
		_db = db;
		_templateService = templateService;
		_ruleSetService = ruleSetService;

	}

	/// <summary>
	/// Retrieve rule details.
	/// </summary>
	/// <remarks>
	/// Authorization: Only system admin.
	/// </remarks>
	/// <returns></returns>
	[HttpGet]
	[Route("api/Rules/All")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public ActionResult Get()
	{
		try
		{
			var rules = _db.Rules.Select(x => new
			{
				x.Id,
				x.TargetModel,
				x.TargetName,
				x.TargetKey,
				x.TargetValue,
				x.Operator
			});

			return Ok(rules);

		}
		catch (Exception e)
		{
			return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.Message });
		}
	}

	/// <summary>
	/// Retrieve rule details.
	/// </summary>
	/// <remarks>
	/// Authorization: Only system admin.
	/// </remarks>
	/// <returns></returns>
	[HttpGet]
	[Route("api/Rule/Triggers")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public ActionResult GetTriggers()
	{
		try
		{
			var response = new List<RuleTriggerViewModel>();
			var triggers = _db.RuleTriggers.OrderByDescending(o => o.CreatedBy);
			foreach (var item in triggers)
			{
				var trigger = new RuleTriggerViewModel
				{
					Id = item.Id,
					Name = item.Name,
					Description = item.Description,
					TargetObject = item.TargetObject,
					TriggerAction = item.TriggerAction,
					Active = item.Active,
					CreatedBy = "system",
					CreatedByImage = null,
				};
				response.Add(trigger);
			}

			return Ok(response);

		}
		catch (Exception e)
		{
			return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.Message });
		}
	}

	/// <summary>
	/// Retrieve rule details by id.
	/// </summary>
	/// <remarks>
	/// Authorization: Only system admin.
	/// </remarks>
	/// <returns></returns>
	[HttpGet]
	[Route("api/Rule/{id}/Trigger")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public ActionResult GetTrigger(int id)
	{
		try
		{
			var trigger = _db.RuleTriggers.Find(id);

			if (trigger == null)
			{
				return BadRequest(new ActionResponse { Result = 1, Message = "Trigger not found." });
			}

			var response = new RuleTriggerViewModel
			{
				Id = trigger.Id,
				Name = trigger.Name,
				Description = trigger.Description,
				TargetObject = trigger.TargetObject,
				TriggerAction = trigger.TriggerAction,
				Active = trigger.Active,
				CreatedBy = "system",
				CreatedByImage = null,
				TemplateId = trigger.TemplateId,
				Category = trigger.Category,
			};

			if (trigger.RuleQueryId.HasValue)
			{
				var queries = _ruleSetService.GetRuleDefinitionLinks(trigger.RuleQueryId.Value);
				var includedQueries = new List<int>();

				foreach (var query in queries)
				{
					if (!includedQueries.Contains(query.LHS))
					{
						includedQueries.Add(query.LHS);
						response.Queries.Add(new RuleQueryViewModel
						{
							Id = query.Id,
							Condition = query.Condition,
							Rule = MapRuleDefinition(query.LHSRuleDef)
						});
					}
					if (query.RHS.HasValue && !includedQueries.Contains(query.RHS.Value))
					{
						includedQueries.Add(query.RHS.Value);
						response.Queries.Add(new RuleQueryViewModel
						{
							Id = query.Id,
							Condition = query.Condition,
							Rule = MapRuleDefinition(query.RHSRuleDef)
						});
					}
				}
			}

			return Ok(response);

		}
		catch (Exception e)
		{
			return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.Message });
		}
	}

	/// <summary>
	/// Map RuleDefinition to RuleDefinitionInfo
	/// </summary>
	/// <param name="query"></param>
	/// <returns></returns>
	private RuleDefinitionInfo MapRuleDefinition(RuleDefinition rule)
	{
		return new RuleDefinitionInfo
		{
			Id = rule.Id,
			Operator = rule.Operator,
			TargetKey = rule.TargetKey,
			TargetModel = rule.TargetModel,
			TargetName = rule.TargetName,
			TargetValue = rule.TargetValue
		};
	}

	/// <summary>
	/// Create a new rule
	/// </summary>
	/// <returns></returns>
	[HttpPost]
	[Route("api/Rule/Create")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult> Post([FromBody] RuleRequest rule)
	{
		try
		{
			var queryIds = new List<int>();

			if (rule.Trigger == null || string.IsNullOrWhiteSpace(rule.Trigger.Name))
			{
				return BadRequest(new ActionResponse { Result = 1, Message = "Trigger not attached." });
			}

			if (rule.Queries == null || rule.Queries.Count() == 0)
			{
				return BadRequest(new ActionResponse { Result = 1, Message = "Atleast one query required." });
			}

			foreach (var query in rule.Queries)
			{
				if (query.LHS == null)
				{
					return BadRequest(new ActionResponse { Result = 1, Message = "Invalid query details." });
				}

				if (query.LHS == query.RHS)
				{
					return BadRequest(new ActionResponse { Result = 1, Message = "LHS and RHS can't be same." });
				}

				var existingLHSRule = _db.Rules.Where(r => r.TargetKey == query.LHS.TargetKey);
				var newLHSRule = new RuleDefinition();
				if (!existingLHSRule.Any())
				{
					newLHSRule.TargetKey = query.LHS.TargetKey;
					newLHSRule.Operator = query.LHS.Operator;
					newLHSRule.TargetValue = query.LHS.TargetValue;
					newLHSRule.TargetModel = query.LHS.TargetModel;
					newLHSRule.TargetName = query.LHS.TargetName;

					_db.Rules.Add(newLHSRule);
				}

				var newRHSRule = new RuleDefinition();
				if (query.RHS != null)
				{
					newRHSRule.TargetKey = query.RHS.TargetKey;
					newRHSRule.Operator = query.RHS.Operator;
					newRHSRule.TargetValue = query.RHS.TargetValue;
					newRHSRule.TargetModel = query.RHS.TargetModel;
					newRHSRule.TargetName = query.RHS.TargetName;

					_db.Rules.Add(newRHSRule);
				}

				await _db.SaveChangesAsync();

				var newQuery = new RuleQuery();
				newQuery.LHS = !existingLHSRule.Any() ? newLHSRule.Id : existingLHSRule.First().Id;
				if (newRHSRule.Id > 0)
				{
					newQuery.Condition = query.Condition;
					newQuery.RHS = newRHSRule.Id;
				}

				_db.RuleQueries.Add(newQuery);
				await _db.SaveChangesAsync();

				queryIds.Add(newQuery.Id);

			}

			var trigger = new RuleTrigger
			{
				Name = rule.Trigger.Name,
				Description = rule.Trigger.Description,
				TargetObject = rule.Trigger.TargetObject,
				TriggerAction = rule.Trigger.TriggerAction,
				RuleQueryId = queryIds.FirstOrDefault(),
				Active = rule.Trigger.Active,
				TemplateId = rule.Trigger.TemplateId,
				Category = rule.Trigger.Category
			};
			_db.RuleTriggers.Add(trigger);
			await _db.SaveChangesAsync();

			return Ok(new ActionSaveResponse
			{
				Result = 0,
				ModelId = trigger.Id,
				ModifiedBy = trigger.ModifiedBy,
				ModifiedDate = trigger.ModifiedDate,
				ModifiedDateString = string.Format("{0:dd-MMM-yyyy HH:mm:ss}", trigger.ModifiedDate)
			});
		}
		catch (Exception e)
		{
			return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.Message });
		}
	}

	/// <summary>
	/// Create a new rule
	/// </summary>
	/// <returns></returns>
	[HttpPut]
	[Route("api/Rule/{id}/Update")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult> Put(int id, [FromBody] RuleRequest rule)
	{
		try
		{
			var trigger = _db.RuleTriggers.Find(id);
			if (trigger == null)
			{
				return BadRequest(new ActionResponse { Result = 1, Message = "Trigger not found." });
			}

			trigger.Active = rule.Trigger.Active;
			trigger.Name = rule.Trigger.Name;
			trigger.Description = rule.Trigger.Description;
			trigger.TargetObject = rule.Trigger.TargetObject;
			trigger.TriggerAction = rule.Trigger.TriggerAction;
			trigger.TemplateId = rule.Trigger.TemplateId;
			trigger.Category = rule.Trigger.Category;

			foreach (var query in rule.Queries)
			{
				//var updateLHS = _db.Rules.Find(query.LHS.Id);

				var newLHSRule = new RuleDefinition();
				var newRHSRule = new RuleDefinition();
               

                var updateLHS = _db.Rules.FirstOrDefault(r => r.TargetKey == query.LHS.TargetKey);

				if (updateLHS == null)
				{
					newLHSRule.TargetKey = query.LHS.TargetKey;
					newLHSRule.Operator = query.LHS.Operator;
					newLHSRule.TargetValue = query.LHS.TargetValue;
					newLHSRule.TargetModel = query.LHS.TargetModel;
					newLHSRule.TargetName = query.LHS.TargetName;

					_db.Rules.Add(newLHSRule);
				}
				else
				{
					updateLHS.Operator = query.LHS.Operator;
					updateLHS.TargetValue = query.LHS.TargetValue;
				}
                RuleDefinition updateRHS = null;
                if (query.RHS != null)
				{
                    updateRHS = _db.Rules.FirstOrDefault(r => r.TargetKey == query.RHS.TargetKey);
                    //var existingRHSRule = _db.Rules.FirstOrDefault(r => r.TargetKey == query.RHS.TargetKey);
                    if (updateRHS == null)
					{
						newRHSRule.TargetKey = query.RHS.TargetKey;
						newRHSRule.Operator = query.RHS.Operator;
						newRHSRule.TargetValue = query.RHS.TargetValue;
						newRHSRule.TargetModel = query.RHS.TargetModel;
						newRHSRule.TargetName = query.RHS.TargetName;

						_db.Rules.Add(newRHSRule);
					}
					else
					{
						updateRHS.Operator = query.RHS.Operator;
						updateRHS.TargetValue = query.RHS.TargetValue;
					}
				}

				await _db.SaveChangesAsync();

				if (updateLHS == null && updateRHS == null)
				{
					var newQuery = new RuleQuery();
					newQuery.LHS = newLHSRule.Id;
					if (newRHSRule.Id > 0)
					{
						newQuery.Condition = query.Condition;
						newQuery.RHS = newRHSRule.Id;
					}

					_db.RuleQueries.Add(newQuery);
				}
				else if (updateLHS != null && updateRHS == null)
				{
					var existingQuery = _db.RuleQueries.FirstOrDefault(q => q.LHS == updateLHS.Id);
					if (existingQuery != null)
					{
						existingQuery.Condition = query.Condition;
						existingQuery.RHS = newLHSRule.Id;
						query.RHS.Id = newLHSRule.Id;
					}
					else
					{
						var newQuery = new RuleQuery();
						newQuery.LHS = updateLHS.Id;
						if (newRHSRule.Id > 0)
						{
							newQuery.Condition = query.Condition;
							newQuery.RHS = newRHSRule.Id;
							_db.RuleQueries.Add(newQuery);
						}
					}
				}
			}

			await _db.SaveChangesAsync();

			return Ok(new ActionSaveResponse
			{
				Result = 0,
				ModelId = trigger.Id,
				ModifiedBy = trigger.ModifiedBy,
				ModifiedDate = trigger.ModifiedDate,
				ModifiedDateString = string.Format("{0:dd-MMM-yyyy HH:mm:ss}", trigger.ModifiedDate)
			});
		}
		catch (Exception e)
		{
			return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.Message });
		}
	}

	/// <summary>
	/// Delete a rule and associated triggers
	/// </summary>
	/// <returns></returns>
	[HttpPut]
	[Route("api/Rule/{id}/Status/{active}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult> ChangeStatus(int id, bool active)
	{
		try
		{
			var trigger = _db.RuleTriggers.Find(id);
			if (trigger == null)
			{
				return BadRequest(new ActionResponse { Result = 1, Message = "Trigger not found." });
			}

			trigger.Active = active;

			await _db.SaveChangesAsync();

			return Ok(new ActionSaveResponse { Result = 0, ModelId = id });
		}
		catch (Exception e)
		{
			return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.GetBaseException().Message });
		}
	}

	/// <summary>
	/// Delete a rule and associated triggers
	/// </summary>
	/// <returns></returns>
	[HttpDelete]
	[Route("api/Rule/{id}/Remove")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult> Delete(int id)
	{
		try
		{
			var trigger = _db.RuleTriggers.Find(id);
			if (trigger == null)
			{
				return BadRequest(new ActionResponse { Result = 1, Message = "Trigger not found." });
			}
			if (trigger.RuleQueryId == null)
			{
				return BadRequest(new ActionResponse { Result = 1, Message = "Invalid query attached to the trigger." });
			}
			var queries = _ruleSetService.GetRuleDefinitionLinks(trigger.RuleQueryId.Value).ToArray();

			_db.RuleTriggers.Remove(trigger);
			await _db.SaveChangesAsync();

			var rules = new List<RuleDefinition>();
			foreach (var query in queries)
			{
				if (!rules.Any(x => x.Id == query.LHS))
				{
					rules.Add(query.LHSRuleDef);
				}

				if (query.RHS.HasValue && !rules.Any(x => x.Id == query.RHS.Value))
				{
					rules.Add(query.RHSRuleDef);
				}
				_db.RuleQueries.Remove(query);
			}
			await _db.SaveChangesAsync();

			foreach (var rule in rules)
			{
				_db.Rules.Remove(rule);
			}
			await _db.SaveChangesAsync();

			return Ok(new ActionSaveResponse { Result = 0, ModelId = id });
		}
		catch (Exception e)
		{
			return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.GetBaseException().Message });
		}
	}

	/// <summary>
	/// Get exception approval details by source id 
	/// </summary>
	/// <param name="sourceId"></param>
	/// <returns></returns>
	[HttpGet]
	[Route("api/Rule/{sourceId}/Exception")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public ActionResult GetExceptionDetails(int sourceId)
	{
		try
		{
			var openExceptions = _db.RuleOutcomes.Where(x => x.SourceId == sourceId).ToList();
			if (openExceptions == null)
			{
				return Ok();
			}
			var exceptions = new List<RuleExceptionResponse>();
			foreach (var openException in openExceptions)
			{
				exceptions.Add(new RuleExceptionResponse
				{
					Id = openException.Id,
					SourceId = openException.SourceId,
					Status = openException.Status,
					ApproverName = openException.ApproverName,
					Approver = openException.Approver,
					ApproverImage = openException.User.Image,
					Reason = openException.Reason,
					DueDate = openException.DueDate,
					Requestor = openException.ModifiedBy ?? openException.CreatedBy,
				});
			}

			return Ok(exceptions);
		}
		catch (Exception e)
		{
			return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.Message });
		}
	}

	/// <summary>
	/// Create a new exception approval request
	/// </summary>
	/// <param name="request"></param>
	/// <returns></returns>
	[HttpPost]
	[Route("api/Rule/Exception/Request")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult> CreateExceptionRequest([FromBody] RuleExceptionRequest request)
	{
		try
		{
			if (request == null)
			{
				return BadRequest(new ActionResponse { Result = 1, Message = $"Invalid request!" });
			}

			var openExceptions = _db.RuleOutcomes.Any(x => x.Reason == request.Reason && x.SourceId == request.SourceId && x.Status == RuleExceptionStatus.ForApproval);
			if (openExceptions)
			{
				return BadRequest(new ActionResponse { Result = 1, Message = $"There is an open exception for source {request.SourceId}." });
			}

			var outcome = new RuleOutcome
			{
				Approver = request.Approver,
				DueDate = request.DueDate,
				SourceId = request.SourceId,
				Status = RuleExceptionStatus.ForApproval,
				Reason = request.Reason
			};
			_db.RuleOutcomes.Add(outcome);
			await _db.SaveChangesAsync();

			return Ok(new ActionSaveResponse
			{
				Result = 0,
				ModelId = outcome.Id,
				ModifiedBy = outcome.ModifiedBy,
				ModifiedDate = outcome.ModifiedDate,
				ModifiedDateString = string.Format("{0:dd-MMM-yyyy HH:mm:ss}", outcome.ModifiedDate)
			});

		}
		catch (Exception e)
		{
			return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.GetBaseException().Message });
		}

	}

	/// <summary>
	/// Resolve a request.
	/// </summary>
	/// <param name="id"></param>
	/// <param name="request"></param>
	/// <returns></returns>
	[HttpPut]
	[Route("api/Rule/{id}/Exception/Update")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult> UpdateExceptionRequest(int id, [FromBody] RuleExceptionRequest request)
	{
		try
		{
			if (request == null)
			{
				return BadRequest(new ActionResponse { Result = 1, Message = $"Invalid request!" });
			}

			var outcome = _db.RuleOutcomes.Find(id);
			if (outcome == null)
			{
				return NotFound(new ActionResponse { Result = 1, Message = $"Exception request not found." });
			}
			outcome.Approver = request.Approver;
			outcome.DueDate = request.DueDate;
			if (!string.IsNullOrWhiteSpace(request.Status) && request.Status != RuleExceptionStatus.ForApproval)
			{
				outcome.Status = request.Status;
			}

			await _db.SaveChangesAsync();

			return Ok(new ActionSaveResponse { Result = 0, ModelId = id });

		}
		catch (Exception e)
		{
			return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.GetBaseException().Message });
		}
	}
}