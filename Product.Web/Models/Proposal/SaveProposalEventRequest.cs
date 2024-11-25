namespace Product.Web.Models.Proposal;

public class SaveProposalEventRequest
{
    public string EventDescription { get; set; }

    public string? DecisionRationaleInfo { get; set; }

    public string OtherDecisionType { get; set; }

    public string Comments { get; set; }

    public int Order { get; set; }
}
