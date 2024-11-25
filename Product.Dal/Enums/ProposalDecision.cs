using System.ComponentModel;

namespace Product.Dal.Enums;

public enum ProposalDecision
{
    [Description("Approved")]
    Approved,
    [Description("Noted")]
    Noted,
    [Description("Unopposed")]
    Unopposed,
    [Description("Supported")]
    Supported,
    [Description("Finalised")]
    Finalised,
    [Description("Declined")]
    Declined,
    [Description("Rework")]
    Rework,
    [Description("Completed")]
    TaskCompleted,
}
