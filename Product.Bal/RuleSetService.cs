using System;
using System.Linq.Expressions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Product.Dal;
using Product.Dal.Entities;
using Product.Dal.Enums;
using Product.Dal.Common.Extensions;
using Product.Bal.Models;
using Product.Bal.Interfaces;

namespace Product.Bal;

public class RuleSetService : IRuleSetService
{
    private readonly DBContext _db;

    public RuleSetService(DBContext db)
    {
        _db = db;
    }

    public bool Execute<T>(RuleDefinition r, T t)
    {
        if (Enum.TryParse<RuleOperator>(r.Operator, out var op))
        {
            switch (op)
            {
                case RuleOperator.Contains:
                case RuleOperator.NotContains:
                    return CompileComplexRule<T>(r, t, op);
                default:
                    return CompileRule<T>(r, t)(t);
            }
        }
        return r == null || false;
    }

    public IEnumerable<RuleResponse> Trigger(string actualValueJson, string targetObject, string triggerAction = "save", bool isTemplateAssignment = false)
    {
        var outcome = new List<RuleResponse>();
        try
        {
            var actuals = JsonConvert.DeserializeObject<Dictionary<string, object>>(actualValueJson);
            actuals = new Dictionary<string, object>(actuals.Where(x => x.Value?.GetType() != typeof(JArray)));
            actuals = (from kv in actuals
                       where kv.Value != null
                       select kv).ToDictionary(kv => kv.Key, kv => kv.Value);

            var triggers = _db.RuleTriggers.ToList().Where(r => r.Active && r.TargetObject.Equals(targetObject, StringComparison.OrdinalIgnoreCase) &&
                                                            r.TriggerAction.Equals(triggerAction, StringComparison.OrdinalIgnoreCase));
            if (isTemplateAssignment)
            {
                triggers = triggers.Where(t => t.Category == RuleCategory.TemplateAssignment.ToDescription());
            }
            else
            {
                triggers = triggers.Where(t => t.Category == RuleCategory.ExceptionRule.ToDescription());
            }

            foreach (var trigger in triggers)
            {
                if (trigger != null)
                {
                    var template = _db.Templates.Find(trigger.TemplateId);
                    if (template != null)
                    {
                        string ruleQueryDesc = string.Empty;
                        bool isValid = true;
                        var executionStatus = new Dictionary<int, bool>();
                        var queries = GetRuleDefinitionLinks(trigger.RuleQueryId.Value).ToArray();
                        var model = GetDynamicModel(template, trigger.RuleQuery.LHSRuleDef.TargetModel).CreateInstance(actuals);

                        foreach (var query in queries)
                        {
                            if (!executionStatus.ContainsKey(query.LHS))
                            {
                                var validLHS = isValid = Execute(query.LHSRuleDef, model);
                                executionStatus.Add(query.LHS, validLHS);

                                ruleQueryDesc = $"{query.LHSRuleDef.TargetName} {FormatRuleCondition(query.LHSRuleDef.Operator, query.LHSRuleDef.TargetValue)} {FormatTargetValue(query.LHSRuleDef.TargetValue)}";
                            }

                            if (query.RHS.HasValue && !executionStatus.ContainsKey(query.RHS.Value))
                            {
                                var validRHS = Execute(query.RHSRuleDef, model);
                                isValid = EvaluateCondition(query.Condition)(isValid, validRHS);
                                executionStatus.Add(query.RHS.Value, validRHS);

                                ruleQueryDesc = $"{ruleQueryDesc} {query.Condition} {query.RHSRuleDef.TargetName} {FormatRuleCondition(query.RHSRuleDef.Operator, query.RHSRuleDef.TargetValue)} {FormatTargetValue(query.RHSRuleDef.TargetValue)}";
                            }
                        }

                        outcome.Add(new RuleResponse
                        {
                            Status = isValid ? "Failed" : "Passed",
                            Rule = ruleQueryDesc,
                            TemplateId = trigger.TemplateId
                        });
                    }
                }
            }


            return outcome;
        }
        catch (Exception e)
        {
            throw e;
        }
    }

    public IEnumerable<RuleQuery> GetRuleDefinitionLinks(int parentId)
    {
        // Traverse down the hierarhcy
        string ngsql =
            $@"WITH RECURSIVE RuleQueryParent(""Id"", ""ParentId"", ""ChildId"") AS (
                SELECT ""Id"", ""LHS"" AS ParentId, ""RHS"" AS ChildId
                FROM public.""RuleQuery""
                WHERE ""Id"" = {parentId}
                UNION ALL
                SELECT child.""Id"", child.""LHS"", child.""RHS""
                FROM RuleQueryParent parent
                INNER JOIN public.""RuleQuery"" child ON parent.""ChildId"" = child.""LHS""
                )
                SELECT l.*
                FROM RuleQueryParent h
                INNER JOIN public.""RuleQuery"" l ON h.""Id"" = l.""Id""
                ORDER BY l.""Id"";";

        return _db.RuleQueries.FromSqlRaw(ngsql).ToList();
    }

    private Expression BuildExpression<T>(RuleDefinition r, T t, ParameterExpression param)
    {
        var left = Expression.Property(Expression.Convert(param, t.GetType()), r.TargetKey);
        var tProp = t.GetType().GetProperty(r.TargetKey).PropertyType;
        // checking for valid .NET operator
        if (ExpressionType.TryParse(r.Operator.ToString(), out ExpressionType tBinary))
        {
            var right = Expression.Constant(Convert.ChangeType(r.TargetValue, tProp));
            // use a binary operation, e.g. 'Equal' -> 'x == y'/ 'x > y'
            return Expression.MakeBinary(tBinary, left, right);
        }
        else
        {
            var method = tProp.GetMethod(r.Operator.ToString());
            var tParam = method.GetParameters()[0].ParameterType;
            var right = Expression.Constant(Convert.ChangeType(r.TargetValue, tParam));
            // use a method call, e.g. 'Contains' -> 'list.Contains(x)'
            return Expression.Call(left, method, right);
        }
    }

    private Func<T, bool> CompileRule<T>(RuleDefinition r, T t)
    {
        var param = Expression.Parameter(typeof(T), "x");
        var expr = BuildExpression<T>(r, t, param);
        return Expression.Lambda<Func<T, bool>>(expr, param).Compile();
    }

    private bool CompileComplexRule<T>(RuleDefinition r, T t, RuleOperator op)
    {
        var token = JToken.Parse(r.TargetValue);

        if (token is JArray)
        {
            var right = JsonConvert.DeserializeObject<IEnumerable<string>>(r.TargetValue);
            var left = t.GetType().GetProperty(r.TargetKey).GetValue(t);

            return op == RuleOperator.Contains ? right.Any(x => x == left.ToString()) : !right.Any(x => x == left.ToString());
        }
        else
        {
            var right = JsonConvert.DeserializeObject<string>(r.TargetValue);
            var left = t.GetType().GetProperty(r.TargetKey).GetValue(t);

            return op == RuleOperator.Contains
            ? left.ToString().Contains(right, StringComparison.OrdinalIgnoreCase)
            : !left.ToString().Contains(right, StringComparison.OrdinalIgnoreCase);
        }

    }

    private DynamicClass GetDynamicModel(Template template, string targetModel)
    {
        var properties = new Dictionary<string, Type>();

        var modifiedSection = template.TemplateData.Sections.FirstOrDefault(x => x.Group == targetModel);
        foreach (var field in modifiedSection.Fields.Where(f => f.Type != "custom"))
        {
            var propertyName = field.Type == "dropdown" ? field.LookupKey : field.Key;
            properties.Add(field.Key, TypeUtil.GetTemplateFieldType(field.Type));
        }
        return new DynamicClass(properties);
    }

    private Func<bool, bool, bool> EvaluateCondition(string condition)
    {
        Func<bool, bool, bool> evalCondition = default;
        if (ExpressionType.TryParse(condition, out ExpressionType tBinary))
        {
            switch (tBinary)
            {
                case ExpressionType.And:
                    evalCondition = (x, y) => (x && y);
                    break;
                case ExpressionType.Or:
                    evalCondition = (x, y) => (x || y);
                    break;
            }
        }
        return evalCondition;
    }

    private string FormatTargetValue(string value)
    {
        if (value.IsValidJson())
        {
            var token = JToken.Parse(value);
            return token is JArray ? string.Join(',', JsonConvert.DeserializeObject<IEnumerable<string>>(value)) : JsonConvert.DeserializeObject<string>(value);
        }
        else
        {
            return value;
        }
    }

    private string FormatRuleCondition(string rOperator, string value)
    {
        var isDateType = DateTime.TryParse(value, out var date);
        var isTextType = value.IsValidJson() && JToken.Parse(value).Type == JTokenType.String;

        switch (rOperator)
        {
            case "Equal":
                return "is";
            case "NotEqual":
                return "is not";
            case "GreaterThan":
                return isDateType ? "is before" : "is above";
            case "LessThan":
                return isDateType ? "is after" : "is below";
            case "Contains":
                return isTextType ? "contains" : "in";
            case "NotContains":
                return isTextType ? "does not contain" : "not in";
            default:
                return rOperator;
        }
    }
}
