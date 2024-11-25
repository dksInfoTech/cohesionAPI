using Microsoft.EntityFrameworkCore;
using Product.Dal.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Product.Dal.Entities;
public class UserRole : IAuditableEntity
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

    [StringLength(50, ErrorMessage = "Max length 50")]
    public string Name { get; set; }

    [StringLength(50, ErrorMessage = "Max length 50")]
    public string Description { get; set; }

    [JsonIgnore]
    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();

    [JsonIgnore]
    public virtual ICollection<User> Users { get; set; } = new List<User>();

    public virtual ICollection<RoleCountry> Countries { get; set; } = new List<RoleCountry>();
}
