using Newtonsoft.Json;
using Product.Dal.Common.Models;
using Product.Dal.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Product.Dal.Entities;
public class Template : IAuditableEntity, ITemplateDefinition
{
    [Key]
    public int Id { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }


    [StringLength(50, ErrorMessage = "Max length 50")]
    public string CreatedBy { get; set; }


    [StringLength(50, ErrorMessage = "Max length 50")]
    public string? ModifiedBy { get; set; }

    public int Version { get; set; }

    public bool IsDeleted { get; set; }

    [StringLength(100, ErrorMessage = "Max length 100")]
    public virtual string TemplateName { get; set; }

    [StringLength(500, ErrorMessage = "Max length 500")]
    public string Description { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public string TemplateJson { get; set; }

    [NotMapped]
    public TemplateData TemplateData
    {
        get
        {
            if (TemplateJson == null)
            {
                return null;
            }
            else
            {
                return JsonConvert.DeserializeObject<TemplateData>(TemplateJson);
            }
        }
        set
        {
            if (value == null)
            {
                TemplateJson = null;
            }
            else
            {
                TemplateJson = JsonConvert.SerializeObject(value);
            }
        }
    }
}
