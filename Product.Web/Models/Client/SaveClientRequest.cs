namespace Product.Web.Models.Client;

public class SaveClientRequest
{
    public int? ImageId { get; set; }
    public string BasicInformation { get; set; }
    public string OtherInformation { get; set; }
    public int TemplateId { get; set; }
    public string? ProposalStatus { get; set; }
}

public class SaveEntityHierarchy
{
    public int? Id { get; set; }
    public string EntityHierarchyInfo { get; set; }
    public int ClientId { get; set; }   
    public bool IsActive { get; set; }
    public bool IsLatest { get; set; }
    public string Type { get; set; } // Legal or Client
    public string ChildIds { get; set; }
}
