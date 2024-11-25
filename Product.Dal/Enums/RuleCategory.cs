using System.ComponentModel;

namespace Product.Dal.Enums;

public enum RuleCategory
{
    [Description("TemplateAssignment")]
    TemplateAssignment,
    [Description("ExceptionRule")]
    ExceptionRule,
    [Description("CustomRule")]
    CustomRule
}
