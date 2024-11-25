using System.Collections.Immutable;
using System.ComponentModel;

namespace Product.Dal.Enums;

public enum ProposalStatus
{
    [Description("Draft")]
    Draft,
    [Description("Pending")]
    Pending,
    [Description("Closed")]
    Closed,
    [Description("Cancelled")]
    Cancelled,
    [Description("Rework")]
    Rework,
    [Description("Withdrawn")]
    Withdrawn,
}
