using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Product.Dal.Interfaces;

namespace Product.Dal.Entities;

public class EventDashboard : IAuditableEntity
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
    [ForeignKey("Client")]
    public int ClientId { get; set; }

    [ForeignKey("Proposal")]
    public int? ProposalId { get; set; }

    public bool IsActive { get; set; }

    public string EventInfo { get; set; }

    public string EventType { get; set; }

    [JsonIgnore]
    public virtual Client Client { get; set; }

    [JsonIgnore]
    public virtual Proposal Proposal { get; set; }
}
