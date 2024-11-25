using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using Product.Dal.Interfaces;

namespace Product.Dal.Entities;
public class Proposal : ProposalDefinition 
{
    public virtual ICollection<ProposalEvent> ProposalEvents { get; set; } = new List<ProposalEvent>();

    public virtual ICollection<ProposalTeamMember> ProposalTeamMembers { get; set; } = new List<ProposalTeamMember>();
}

public class ProposalDefinition : IAuditableEntity
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

    public int? ClientVersion { get; set; }

    [StringLength(500, ErrorMessage = "Max length 500")]
    public string? Title { get; set; }

    [StringLength(20, ErrorMessage = "Max length 20")]
    public string Status { get; set; }

    [StringLength(20, ErrorMessage = "Max length 20")]
    //[StringRange(AllowedValues = new[] { ProposalDecision.Approved, ProposalDecision.AssetWriteApproved, ProposalDecision.Noted, ProposalDecision.NoObjections, ProposalDecision.Supported, ProposalDecision.Declined, ProposalDecision.Rework, ProposalDecision.Finalised, ProposalDecision.ReworkRiskGrade, ProposalDecision.RefreshRiskGrade, ProposalDecision.TaskCompleted })]
    public string? Decision { get; set; }

    public bool IsClientUpdate { get; set; }

    public string? ProposalInfo { get; set; }
    
    public string? Comments { get; set; }

    [ForeignKey("LastContributor")]
    public string? LastContributorId { get; set; }

    public DateTime? LastContributedDate { get; set; }

    public DateTime? ClosedDate { get; set; }

    [NotMapped]
    public virtual string LastContributorName => LastContributor?.Name;

    [NotMapped]
    public virtual string ClientName => Client?.Name;

    [JsonIgnore]
    public virtual Client Client { get; set; }

    [JsonIgnore]
    public virtual User LastContributor { get; set; }
}