using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Product.Dal.Interfaces;

namespace Product.Dal.Entities;
public class RuleTrigger : IAuditableEntity
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


    [StringLength(300, ErrorMessage = "Max length 300")]
    [Required]
    public string Name { get; set; }


    [StringLength(500, ErrorMessage = "Max length 500")]
    public string? Description { get; set; }


    [Required(ErrorMessage = "Required")]
    [StringLength(100, ErrorMessage = "Max length 100")]
    public string TargetObject { get; set; }


    [StringLength(100, ErrorMessage = "Max length 100")]
    public string TriggerAction { get; set; }


    [Required(ErrorMessage = "Required")]
    public bool Active { get; set; }


    [ForeignKey("RuleQuery")]
    public int? RuleQueryId { get; set; }


    [ForeignKey("Template")]
    public int TemplateId { get; set; }



    [Required(ErrorMessage = "Required")]
    [StringLength(100, ErrorMessage = "Max length 100")]
    //[StringRange(AllowedValues = new[] { RuleCategory.TemplateAssignment, RuleCategory.ExceptionRule, RuleCategory.CustomRule })]
    public string Category { get; set; }

    [JsonIgnore]
    public virtual RuleQuery RuleQuery { get; set; }

    [JsonIgnore]
    public virtual Template Template { get; set; }

}
