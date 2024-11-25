using Microsoft.EntityFrameworkCore;
using Product.Dal.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Product.Dal.Entities;
public class ProposalEvent : IAuditableEntity
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
    public int ProposalId { get; set; }

    [RegularExpression(@"^[^<>]*$", ErrorMessage = "angle brackets are not allowed")]
    //[SafeHtml]
    [StringLength(250, ErrorMessage = "Max length 250")]
    [Required(ErrorMessage = "Required")]
    public string EventDescription { get; set; }

    public string? DecisionRationaleInfo { get; set; }     

    [RegularExpression(@"^[^<>]*$", ErrorMessage = "angle brackets are not allowed")]
    //[SafeHtml]
    [StringLength(100, ErrorMessage = "Max length 100")]
    public string? OtherDecisionType { get; set; }

    public string? Comments { get; set; }

    public int Order { get; set; }

    [NotMapped]
    public string ClientName => Proposal.ClientName;

    [NotMapped]
    public Image ClientImage => Proposal.Client.Image;

    [JsonIgnore]
    public virtual Proposal Proposal { get; set; }
}