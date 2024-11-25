using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Product.Dal.Entities;

public class ClientDraft : ClientDefinition
{
    [Key]
    public int Id { get; set; }

    public int? ProposalId { get; set; }


    [ForeignKey("Client")]
    public int ClientId { get; set; }


    [StringLength(20, ErrorMessage = "Max length 20")]
    public string Status { get; set; }

    [JsonIgnore]
    public virtual Client Client { get; set; }
}

