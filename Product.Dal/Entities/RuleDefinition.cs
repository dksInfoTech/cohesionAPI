using System.ComponentModel.DataAnnotations;
using Product.Dal.Interfaces;

namespace Product.Dal.Entities;
public class RuleDefinition : IAuditableEntity
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
    [StringLength(100, ErrorMessage = "Max length 100")]
    public string TargetModel { get; set; }


    [Required(ErrorMessage = "Required")]
    [StringLength(100, ErrorMessage = "Max length 100")]
    public string TargetKey { get; set; }


    [Required(ErrorMessage = "Required")]
    [StringLength(100, ErrorMessage = "Max length 100")]
    public string TargetName { get; set; }


    [Required(ErrorMessage = "Required")]
    [StringLength(50, ErrorMessage = "Max length 50")]
    public string Operator { get; set; }


    [Required(ErrorMessage = "Required")]
    [StringLength(300, ErrorMessage = "Max length 300")]
    public string TargetValue { get; set; }

    public virtual ICollection<RuleTrigger> RuleTriggers { get; set; }
}
