using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Product.Dal.Interfaces;

namespace Product.Dal.Entities;
public class User : IAuditableEntity
{
    [Key]
    [StringLength(50, ErrorMessage = "Max length 50")]
    public string Id { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    [StringLength(50, ErrorMessage = "Max length 50")]
    public string CreatedBy { get; set; }

    [StringLength(50, ErrorMessage = "Max length 50")]
    public string? ModifiedBy { get; set; }

    public int Version { get; set; }

    [Display(Name = "Last Access")]
    public DateTime? LastAccessDate { get; set; }

    [StringLength(200, ErrorMessage = "Max length 200")]
    public string FirstName { get; set; }

    [StringLength(200, ErrorMessage = "Max length 200")]
    public string LastName { get; set; }

    [StringLength(500, ErrorMessage = "Max length 200")]
    public string Password { get; set; }

    [Required(ErrorMessage = "Required")]
    [StringLength(50, ErrorMessage = "Max length 50")]
    public string Country { get; set; }

    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    [StringLength(250, ErrorMessage = "Max length 250")]
    public string? Email { get; set; }

    public bool Active { get; set; }

    public bool SystemAdmin { get; set; }

    [ForeignKey("UserRole")]
    public int? UserRoleId { get; set; }

    public DateTime? ExpirationDate { get; set; }

    [JsonIgnore]
    public virtual UserRole UserRole { get; set; }

    [NotMapped]
    public string Role => UserRole?.Name;

    [NotMapped]
    public string Name => $"{FirstName} {LastName}".Trim();

    [ForeignKey("Image")]
    public int? ImageId { get; set; }

    [JsonIgnore]
    public virtual Image Image { get; set; }
}
