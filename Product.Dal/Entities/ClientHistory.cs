using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Product.Dal.Entities;

public class ClientHistory : ClientDefinition
{
        [Key]
        public int Id { get; set; }
        public int? ProposalId { get; set; }

        [ForeignKey("Client")]
        public int ClientId { get; set; }

        [JsonIgnore]
        public virtual Client Client { get; set; }
}
