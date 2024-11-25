using System;

namespace Product.Bal.Models;

public class SaveFacilityRequest
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public int ProposalId { get; set; }
    public int? FacilityDocumentId { get; set; }
    public int? InterchangableLimitId { get; set; }
    public string FacilityInfo { get; set; }
    public string? Comments { get; set; }
}
