using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Product.Dal.Interfaces;

namespace Product.Dal.Entities;
public class RuleQuery : IAuditableEntity
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


    [Required(ErrorMessage = "Required")]
    [ForeignKey("LHSRuleDef")]
    public int LHS { get; set; }


    [StringLength(100, ErrorMessage = "Max length 100")]
    public string? Condition { get; set; }


    [ForeignKey("RHSRuleDef")]
    public int? RHS { get; set; }

    [JsonIgnore]
    public virtual RuleDefinition RHSRuleDef { get; set; }

    [JsonIgnore]
    public virtual RuleDefinition LHSRuleDef { get; set; }
}
