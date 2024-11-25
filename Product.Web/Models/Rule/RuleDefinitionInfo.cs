namespace Product.Web.Models.Rule;

public class RuleDefinitionInfo
{
    public int Id { get; set; }

    public string TargetModel { get; set; }

    public string TargetKey { get; set; }

    public string Operator { get; set; }

    public string TargetValue { get; set; }

    public string TargetName { get; set; }
}

public class RuleTriggerInfo
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string? Description { get; set; }

    public string TargetObject { get; set; }

    public string TriggerAction { get; set; }

    public bool Active { get; set; }

    public int TemplateId { get; set; }

    public string Category { get; set; }
}

public class CustomRuleDefinitionInfo
{
    public int Id { get; set; }

    public string MethodName { get; set; }

    public string ReturnType { get; set; }

    public IEnumerable<MethodParameter> Parameters { get; set; }
}

public class MethodParameter
{
    public string Type { get; set; }

    public string Parameter { get; set; }

    public bool IsOptional { get; set; }
}

