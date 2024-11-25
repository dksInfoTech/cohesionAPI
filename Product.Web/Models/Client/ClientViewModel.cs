using Product.Dal.Entities;

namespace Product.Web.Models.Client;

public class ClientViewModel : Dal.Entities.Client
{
    /// <summary>
    /// The Client for "diff" comparison.
    /// </summary>
    public Dal.Entities.Client DiffClient { get; set; }

    /// <summary>
    /// The client has an active draft.
    /// </summary>
    public bool HasDraft { get; set; }

    /// <summary>
    /// The proposal Id of the active draft.
    /// </summary>
    public int? DraftProposalId { get; set; }

    public string? ProposalStatus { get; set; }

    public ICollection<Dal.Entities.Entity> Entities { get; set; }

    public ICollection<Dal.Entities.Facility> Facilities { get; set; }
}
