using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Product.Dal.Interfaces;

namespace Product.Dal.Entities;
public class ProposalTeamMember : IAuditableEntity
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

    [Required(ErrorMessage = "Required")]
    public string UserId { get; set; }

    [StringLength(500, ErrorMessage = "Max length 500")]
    public string Title { get; set; }

    [StringLength(50, ErrorMessage = "Max length 50")]
    //[StringRange(AllowedValues = new[] { ProposalRole.ForApproval, ProposalRole.ForAssetWriteApproval, ProposalRole.ForApprovalDca, ProposalRole.ForNoting, ProposalRole.ForNoObjections, ProposalRole.ForSupport, ProposalRole.ToFinalise, ProposalRole.Task })]
    public string Role { get; set; }

    [StringLength(50, ErrorMessage = "Max length 50")]
    //[StringRange(AllowedValues = new[] { ProposalDecision.Approved, ProposalDecision.AssetWriteApproved, ProposalDecision.Noted, ProposalDecision.NoObjections, ProposalDecision.Supported, ProposalDecision.Declined, ProposalDecision.Rework, ProposalDecision.Finalised, ProposalDecision.ReworkRiskGrade, ProposalDecision.RefreshRiskGrade, ProposalDecision.TaskCompleted })]
    public string? Decision { get; set; }

    public bool IsFinal { get; set; }

    public string? Comments { get; set; }

    public DateTime? LastDecisionDate { get; set; }

    public DateTime? ExpectedDecisionDate { get; set; }

    [NotMapped]
    public string? UserName => User?.Name;

    [NotMapped]
    public Image? UserImage => User?.Image;

    [NotMapped]
    public string? ProposalStatus => Proposal?.Status;

    [JsonIgnore]
    public virtual Proposal Proposal { get; set; }

    [JsonIgnore]
    public virtual User User { get; set; }
}