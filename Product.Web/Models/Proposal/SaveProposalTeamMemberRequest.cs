namespace Product.Web.Models.Proposal;

public class SaveProposalTeamMemberRequest
{
    public string UserId { get; set; }
    public string? Role { get; set; }
    public string? Decision { get; set; }
    public DateTime? ExpectedDecisionDate { get; set; }
    public string? Comments { get; set; }
    public string? Title { get; set; }
}
