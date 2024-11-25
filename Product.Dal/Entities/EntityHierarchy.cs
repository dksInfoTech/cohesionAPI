using Product.Dal.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Product.Dal.Entities;

public class EntityHierarchy : IAuditableEntity
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

    public bool IsActive { get; set; }

    public bool IsLatest { get; set; }

    public string Type { get; set; }

    public string EntityHierarchyInfo { get; set; }

    public int ClientId { get; set; }

    [StringLength(500, ErrorMessage = "Max length 500")]
    public string? ChildIds { get; set; }

}

