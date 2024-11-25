using Product.Dal.Common.Models;
using Product.Dal.Interfaces;

namespace Product.Web.Models.Template;

public class TemplateViewModel : ITemplateDefinition
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public string CreatedBy { get; set; }
    public string ModifiedBy { get; set; }
    public int Version { get; set; }
    public bool IsDeleted { get; set; }
    public string TemplateName { get; set; }
    public string Description { get; set; }
    public TemplateData TemplateData { get; set; }
    public List<string> UnassignedFields { get; set; }
}
