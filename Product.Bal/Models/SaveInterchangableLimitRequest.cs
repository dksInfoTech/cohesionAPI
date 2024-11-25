using System;

namespace Product.Bal.Models;

public class SaveInterchangableLimitRequest
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public int ProposalId { get; set; }
    public string InterchangableLimitInfo { get; set; }
    public string? Comments { get; set; }
    public int? FacilityDocumentId { get; set; }
}
