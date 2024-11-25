using System.Collections.Immutable;
using Product.Dal.Common.Extensions;
using Product.Dal.Enums;

namespace Product.Dal.Common.Workflow;

public static class ProposalWorkFlowRole
{
    public static ImmutableList<string> Values =>
        [
            ProposalRole.ForApproval.ToDescription(),
            ProposalRole.ForNoting.ToDescription(),
            ProposalRole.ForSupport.ToDescription(),
            ProposalRole.ForUnopposed.ToDescription(),
            ProposalRole.ToFinalise.ToDescription(),
            ProposalRole.Task.ToDescription(),
        ];
    public static ImmutableList<string> LookUpValues =>
        [
            ProposalRole.ForApproval.ToDescription(),
            ProposalRole.ForNoting.ToDescription(),
            ProposalRole.ForSupport.ToDescription(),
            ProposalRole.ForUnopposed.ToDescription(),
            ProposalRole.ToFinalise.ToDescription(),
        ];
    public static ImmutableList<string> AllowedRolesForFinalize =>
        [
            ProposalRole.ForApproval.ToDescription(),
            ProposalRole.ForNoting.ToDescription(),
            ProposalRole.ForUnopposed.ToDescription(),
            // ProposalRole.Task.ToDescription(),
        ];
}
