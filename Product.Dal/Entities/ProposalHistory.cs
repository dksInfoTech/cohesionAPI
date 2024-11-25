using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Product.Dal.Entities;
public class ProposalHistory
{
    [Key]
    public int Id { get; set; }

    public DateTime CreatedDate { get; set; }

    public int ClientId { get; set; }

    public int? ClientVersion { get; set; }

    [StringLength(20, ErrorMessage = "Max length 20")]
    //[StringRange(AllowedValues = new[] { ProposalStatus.Draft, ProposalStatus.Pending, ProposalStatus.Closed, ProposalStatus.Cancelled, ProposalStatus.Rework, ProposalStatus.Withdrawn })]
    public string Status { get; set; }


    [StringLength(20, ErrorMessage = "Max length 20")]
    //[StringRange(AllowedValues = new[] { ProposalDecision.Approved, ProposalDecision.AssetWriteApproved, ProposalDecision.Noted, ProposalDecision.NoObjections, ProposalDecision.Supported, ProposalDecision.Declined, ProposalDecision.Rework, ProposalDecision.Finalised, ProposalDecision.ReworkRiskGrade, ProposalDecision.RefreshRiskGrade, ProposalDecision.TaskCompleted })]
    public string? Decision { get; set; }

    public bool IsClientUpdate { get; set; }

    public string? ProposalInfo { get; set; }

    public string? LastContributorId { get; set; }

    public DateTime? LastContributedDate { get; set; }

    public DateTime? ClosedDate { get; set; }

    public string? LastContributorName { get; set; }

    public virtual IEnumerable<ProposalTeamMember> ProposalTeamMembers { get; set; }

}