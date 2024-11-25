using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Product.Bal.Models;
using Product.Dal.Entities;

namespace Product.Bal.Interfaces;

    public interface IRuleSetService
    {
        bool Execute<T>(RuleDefinition r, T t);

        IEnumerable<RuleResponse> Trigger(string actualValueJson, string targetObject, string triggerAction = "save", bool isTemplateAssignment = false);

        IEnumerable<RuleQuery> GetRuleDefinitionLinks(int parentId);
    }

