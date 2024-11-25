using Product.Dal.Common.Workflow;

namespace Product.Web.Models.Proposal;

public class ProposalViewModel : Dal.Entities.ProposalDefinition
{
    public new string LastContributorName { get; set; }

    public new string ClientName { get; set; }

    public bool ProposalInProgress => ProposalWorkFlowStatus.OpenValues.Contains(Status);

    public bool ProposalHasClientUpdate { get; set; }
}
