using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Product.Dal.Interfaces;

namespace Product.Dal.Entities;
public class Conversation : IAuditableEntity
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

    [StringLength(20, ErrorMessage = "Max length 20")]
    [Required]
    public string ProposalStatusCaptured { get; set; }

    [StringLength(300, ErrorMessage = "Max length 300")]
    [Required]
    public string ThreadTopic { get; set; }

    [ForeignKey("Proposal")]
    public int ProposalId { get; set; }

    [StringLength(100, ErrorMessage = "Max length 100")]
    [Required]
    public string LinkKey { get; set; }

    [ForeignKey("Client")]
    public int ClientId { get; set; }

    [StringLength(300, ErrorMessage = "Max length 300")]
    public string PinTo { get; set; }

    public bool IsDeleted { get; set; }

    public bool Flagged { get; set; }

    [JsonIgnore]
    public virtual Proposal Proposal { get; set; }

    [JsonIgnore]
    public virtual Client Client { get; set; }

    public virtual ICollection<ConversationReply> Replies { get; set; } = new List<ConversationReply>();
}
