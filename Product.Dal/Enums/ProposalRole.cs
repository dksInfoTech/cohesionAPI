using System.ComponentModel;

namespace Product.Dal.Enums;

public enum ProposalRole
{
    [Description("Undecided")]
    Undecided,
    [Description("For Approval")]
    ForApproval,
    [Description("For Support")]
    ForSupport,
    [Description("For Noting")]
    ForNoting,
    [Description("For Unopposed")]
    ForUnopposed,
    [Description("To Finalise")]
    ToFinalise,
    [Description("Task")]
    Task,
}
