using System;

namespace Product.Bal.Models;

public class SaveFacilityDocumentRequest
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public int ProposalId { get; set; }
    public string FacilityDocumentInfo { get; set; }
    public string? Comments { get; set; }
}
