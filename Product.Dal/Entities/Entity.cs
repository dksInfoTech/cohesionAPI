using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Product.Dal.Interfaces;

namespace Product.Dal.Entities;
public class Entity : IAuditableEntity
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

    public int? ClientId { get; set; }

    [Required]
    [StringLength(200, ErrorMessage = "Max length 200")]
    public string Name { get; set; }

    [StringLength(100, ErrorMessage = "Max length 100")]
    public string Country { get; set; }

    public string? EntityType { get; set; }

    public string? PolicyCountry { get; set; }

    public string? PolicyStatus { get; set; }

    public bool? IsValidCustomer { get; set; }

    public string? CCR { get; set; }

    public string? SI { get; set; }

    public string? SourceEntityInformation { get; set; }

    [NotMapped]
    public string? ClientName => Client?.Name;

    [StringLength(50, ErrorMessage = "Max length 50")]
    public string SourceSystemName { get; set; }

    [StringLength(50, ErrorMessage = "Max length 50")]
    public string SourceId { get; set; }

    [StringLength(20, ErrorMessage = "Max length 20")]
    public string? Ticker { get; set; }

    [JsonIgnore]
    public virtual Client Client { get; set; }
}

