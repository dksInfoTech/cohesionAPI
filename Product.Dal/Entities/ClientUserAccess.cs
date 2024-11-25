using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Product.Dal.Entities;

public class ClientUserAccess
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("User")]
    public string UserId { get; set; }

    [ForeignKey("Client")]
    public int ClientId { get; set; }

    public bool Read { get; set; }

    public bool Write { get; set; }

    public bool Approve { get; set; }

    public bool Admin { get; set; }

    public virtual User User { get; set; }

    public virtual Client Client { get; set; }
}
