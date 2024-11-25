using Product.Dal.Entities;

namespace Product.Web.Models.Rule;
public class RuleTriggerViewModel : RuleTriggerInfo
{
    public RuleTriggerViewModel()
    {
        Queries = new List<RuleQueryViewModel>();
    }

    public string CreatedBy { get; set; }

    public Image? CreatedByImage { get; set; }

    public IList<RuleQueryViewModel> Queries { get; set; }

    public CustomRuleDefinitionInfo CustomRule { get; set; }
}

