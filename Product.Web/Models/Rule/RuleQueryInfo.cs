namespace Product.Web.Models.Rule;
public class RuleQueryInfo
{
    public int Id { get; set; }

    public RuleDefinitionInfo LHS { get; set; }

    public string? Condition { get; set; }

    public RuleDefinitionInfo? RHS { get; set; }
}

public class RuleRequest
{
    public IEnumerable<RuleQueryInfo> Queries { get; set; }

    public RuleTriggerInfo Trigger { get; set; }
}

public class CustomRuleRequest
{
    public CustomRuleDefinitionInfo Rule { get; set; }

    public RuleTriggerInfo Trigger { get; set; }
}

