using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Product.Dal.Interfaces;

namespace Product.Dal.Entities;

public abstract class ClientDefinition : IAuditableEntity
{
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public string CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
    public int Version { get; set; }
    [Required(ErrorMessage = "Required")]
    public required string Name { get; set; }
    [Required(ErrorMessage = "Required")]
    public required string BasicInformation { get; set; }
    public string? OtherInformation { get; set; }
    public bool IsDeleted { get; set; }
    [ForeignKey("Image")]
    public int? ImageId { get; set; }
    [ForeignKey("TemplateRef")]
    public int TemplateId { get; set; }
    [JsonIgnore]
    public virtual Template TemplateRef { get; set; }
    [JsonIgnore]
    public virtual Image Image { get; set; }
}
