using System.ComponentModel.DataAnnotations;
using Product.Dal.Interfaces;

namespace Product.Dal.Entities;
public class ReferenceType : IAuditableEntity
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
    public string RefKey { get; set; }

    public bool IsFilteringAllowed { get; set; }

    public bool IsClientFilterAllowed { get; set; }

}
