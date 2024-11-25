using System.Collections.Immutable;
using Product.Dal.Common.Extensions;
using Product.Dal.Entities;
using Product.Dal.Enums;

namespace Product.Dal.Common.Workflow;

public static class ProposalWorkFlowDecision
{
    public static ImmutableList<string> Values =>
        [
            ProposalDecision.Approved.ToDescription(),
            ProposalDecision.Rework.ToDescription(),
            ProposalDecision.Noted.ToDescription(),
            ProposalDecision.Supported.ToDescription(),
            ProposalDecision.Unopposed.ToDescription(),
            ProposalDecision.Finalised.ToDescription(),
            ProposalDecision.TaskCompleted.ToDescription(),
            ProposalDecision.Declined.ToDescription(),
        ];
    public static ImmutableList<string> LookUpValues =>
        [
            ProposalDecision.Approved.ToDescription(),
            ProposalDecision.Rework.ToDescription(),
            ProposalDecision.Noted.ToDescription(),
            ProposalDecision.Supported.ToDescription(),
            ProposalDecision.Unopposed.ToDescription(),
            ProposalDecision.Finalised.ToDescription(),
            ProposalDecision.Declined.ToDescription(),
        ];

    // Validation Rules
    public static readonly List<string> ApprovedAllowedFromRole = new List<string> { ProposalRole.ForApproval.ToDescription() };
    public static readonly List<string> NotedAllowedFromRole = new List<string> { ProposalRole.ForNoting.ToDescription() };
    public static readonly List<string> TaskCompletedAllowedFromRole = new List<string> { ProposalRole.Task.ToDescription() };
    public static readonly List<string> UnopposedAllowedFromRole = new List<string> { ProposalRole.ForUnopposed.ToDescription() };
    public static readonly List<string> SupportedAllowedFromRole = new List<string> { ProposalRole.ForSupport.ToDescription() };
    public static readonly List<string> FinalisedAllowedFromRole = new List<string> { ProposalRole.ToFinalise.ToDescription() };
    public static readonly List<string> DeclinedAllowedFromRole = new List<string> {
                                                                                        ProposalRole.ForApproval.ToDescription(),
                                                                                        ProposalRole.ForNoting.ToDescription(),
                                                                                        ProposalRole.ForUnopposed.ToDescription(),
                                                                                        ProposalRole.ForSupport.ToDescription(),
                                                                                        ProposalRole.ToFinalise.ToDescription()
                                                                                    };
    public static readonly List<string> ReworkAllowedFromRole = new List<string> {
                                                                                    ProposalRole.ForApproval.ToDescription(),
                                                                                    ProposalRole.ForNoting.ToDescription(),
                                                                                    ProposalRole.ForUnopposed.ToDescription(),
                                                                                    ProposalRole.ForSupport.ToDescription(),
                                                                                    ProposalRole.ToFinalise.ToDescription()
                                                                                  };

    /// <summary>
    /// Validate if the decision value is allowed.
    /// </summary>
    /// <param name="decision">Target decision value.</param>
    /// <param name="role">Current role value.</param>
    /// <param name="message">Error message when result is false.</param>
    /// <returns></returns>
    public static bool Validate(string decision, string role, out string message)
    {
        message = null;

        if (string.IsNullOrEmpty(decision))
        {
            message = "Decision cannot be blank.";
            return false;
        }

        if (string.IsNullOrEmpty(role))
        {
            message = "Role cannot be blank.";
            return false;
        }

        if (!Values.Contains(decision))
        {
            message = $"Decision '{decision}' is not allowed.";
            return false;
        }

        if (!ProposalWorkFlowRole.Values.Contains(role))
        {
            message = $"Role '{role}' is not allowed.";
            return false;
        }

        if (decision == ProposalDecision.Approved.ToDescription() && !ApprovedAllowedFromRole.Contains(role))
        {
            message = $"Decision '{decision}' is not allowed for role '{role}'.";
            return false;
        }

        if (decision == ProposalDecision.Noted.ToDescription() && !NotedAllowedFromRole.Contains(role))
        {
            message = $"Decision '{decision}' is not allowed for role '{role}'.";
            return false;
        }

        if (decision == ProposalDecision.Unopposed.ToDescription() && !UnopposedAllowedFromRole.Contains(role))
        {
            message = $"Decision '{decision}' is not allowed for role '{role}'.";
            return false;
        }

        if (decision == ProposalDecision.Supported.ToDescription() && !SupportedAllowedFromRole.Contains(role))
        {
            message = $"Decision '{decision}' is not allowed for role '{role}'.";
            return false;
        }

        if (decision == ProposalDecision.Declined.ToDescription() && !DeclinedAllowedFromRole.Contains(role))
        {
            message = $"Decision '{decision}' is not allowed for role '{role}'.";
            return false;
        }

        if (decision == ProposalDecision.Rework.ToDescription() && !ReworkAllowedFromRole.Contains(role))
        {
            message = $"Decision '{decision}' is not allowed for role '{role}'.";
            return false;
        }

        if (decision == ProposalDecision.Finalised.ToDescription() && !FinalisedAllowedFromRole.Contains(role))
        {
            message = $"Decision '{decision}' is not allowed for role '{role}'.";
            return false;
        }

        if (decision == ProposalDecision.TaskCompleted.ToDescription() && !TaskCompletedAllowedFromRole.Contains(role))
        {
            message = $"Decision '{decision}' is not allowed for role '{role}'.";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Identify the final decision for a proposal.
    /// </summary>
    /// <param name="clientTeamMembers"></param>
    /// <returns></returns>
    public static string GetFinalDecision(IEnumerable<ProposalTeamMember> clientTeamMembers)
    {
        if (clientTeamMembers.Any(x => x.Decision == ProposalDecision.Declined.ToDescription()))
        {
            return ProposalDecision.Declined.ToDescription();
        }
        else if (clientTeamMembers.Any(x => x.Decision == ProposalDecision.Approved.ToDescription()))
        {
            return ProposalDecision.Approved.ToDescription();
        }
        else if (clientTeamMembers.Any(x => x.Decision == ProposalDecision.Noted.ToDescription()))
        {
            return ProposalDecision.Noted.ToDescription();
        }
        else if (clientTeamMembers.Any(x => x.Decision == ProposalDecision.Unopposed.ToDescription()))
        {
            return ProposalDecision.Unopposed.ToDescription();
        }
        else if (clientTeamMembers.Any(x => x.Decision == ProposalDecision.TaskCompleted.ToDescription()))
        {
            return ProposalDecision.TaskCompleted.ToDescription();
        }
        else
        {
            return ProposalDecision.Supported.ToDescription();
        }
    }
}
