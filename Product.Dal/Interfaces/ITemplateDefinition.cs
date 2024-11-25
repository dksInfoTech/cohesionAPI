using Product.Dal.Common.Models;

namespace Product.Dal.Interfaces;

public interface ITemplateDefinition
{
    string TemplateName { get; set; }
    string Description { get; set; }
    TemplateData TemplateData { get; set; }
}
