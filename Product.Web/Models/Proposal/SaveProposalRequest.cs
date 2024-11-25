using Product.Web.Models.Facility;

namespace Product.Web.Models.Proposal;

public class SaveProposalRequest
{
    public int ClientId { get; set; }

    public string Status { get; set; }

    public string Title { get; set; }

    public bool IsClientUpdate { get; set; }

    public string Comments { get; set; }

    public string ProposalInfo { get; set; }

    public IEnumerable<string> ModifiedFields { get; set; }

    public string createdBy { get; set; }
}


public class SaveEvents
{
    public int ClientId { get; set; }
    public int ProposalId { get; set; }
    public string Type { get; set; }
    public string EventData { get; set; }

}
