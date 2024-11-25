using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Product.Dal.Entities;
public class ClientTeamMember
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Required")]
    [ForeignKey("Client")]
    public int ClientId { get; set; }

    [Required]
    [ForeignKey("ClientUserMap")]
    public int ClientUserMapId { get; set; }


    [StringLength(50, ErrorMessage = "Max length 50")]
    //[StringRange(AllowedValues = new[] { Models.ProposalRole.ForApproval, Models.ProposalRole.ForAssetWriteApproval, Models.ProposalRole.ForApprovalDca, Models.ProposalRole.ForNoting, Models.ProposalRole.ForNoObjections, Models.ProposalRole.ForSupport, Models.ProposalRole.ToFinalise, Models.ProposalRole.Task })]
    public string ProposalRole { get; set; }

    [NotMapped]
    public string UserId => ClientUserMap.User.Id;
    [NotMapped]
    public string UserName => ClientUserMap.User.Name;
    [NotMapped]
    public Image UserImage => ClientUserMap.User.Image;
    [NotMapped]
    public string UserRole => ClientUserMap.User.UserRole.Name;
    [NotMapped]
    public string UserCountry => ClientUserMap.User.Country;

    [JsonIgnore]
    public virtual ClientUserAccess ClientUserMap { get; set; }

    [JsonIgnore]
    public virtual Client Client { get; set; }
}

