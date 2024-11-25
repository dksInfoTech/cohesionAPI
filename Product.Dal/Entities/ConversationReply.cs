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
public class ConversationReply : IAuditableEntity
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

    public bool IsDeleted { get; set; }

    public bool Flagged { get; set; }

    [StringLength(500, ErrorMessage = "Max length 500")]
    [Required]
    public string Reply { get; set; }

    [ForeignKey("Conversation")]
    public int ConversationId { get; set; }

    [JsonIgnore]
    public virtual Conversation Conversation { get; set; }
}
