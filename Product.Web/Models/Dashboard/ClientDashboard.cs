using Product.Dal.Entities;

namespace Product.Web.Models.Dashboard;

public class ClientDashboard
{
    public required int Id { get; set; }

    public required DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public required string CreatedBy { get; set; }

    public string? ModifiedBy { get; set; }

    public required int Version { get; set; }

    public required string ClientName { get; set; }

    public required string BasicInformation { get; set; }

    public DateTime? SortDate { get; set; }

    public Image Image { get; set; }

    public bool HasOpenProposal { get; set; }

    public ProposalDashboard? OpenProposal { get; set; }

    public IEnumerable<ProposalDashboard> Proposals { get; set; }
}
