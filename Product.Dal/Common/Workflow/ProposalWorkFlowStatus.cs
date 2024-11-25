using System.Collections.Immutable;
using Product.Dal.Common.Extensions;
using Product.Dal.Enums;

namespace Product.Dal.Common.Workflow;

public static class ProposalWorkFlowStatus
{
  public static ImmutableList<string> Values =>
    [
        ProposalStatus.Draft.ToDescription(),
        ProposalStatus.Pending.ToDescription(),
        ProposalStatus.Closed.ToDescription(),
        ProposalStatus.Cancelled.ToDescription(),
        ProposalStatus.Rework.ToDescription(),
        ProposalStatus.Withdrawn.ToDescription(),
    ];

  public static ImmutableList<string> OpenValues => [ProposalStatus.Draft.ToDescription(), ProposalStatus.Pending.ToDescription(), ProposalStatus.Rework.ToDescription()];

  public static ImmutableList<string> ClosedValues => [ProposalStatus.Closed.ToDescription(), ProposalStatus.Cancelled.ToDescription(), ProposalStatus.Withdrawn.ToDescription()];

  private static readonly ImmutableList<string> PendingAllowedFrom = [ProposalStatus.Draft.ToDescription(), ProposalStatus.Rework.ToDescription()];
  private static readonly ImmutableList<string> ClosedAllowedFrom = [ProposalStatus.Pending.ToDescription()];
  private static readonly ImmutableList<string> CancelledAllowedFrom = [ProposalStatus.Draft.ToDescription(), ProposalStatus.Pending.ToDescription(), ProposalStatus.Rework.ToDescription()];
  private static readonly ImmutableList<string> ReworkAllowedFrom = ClosedAllowedFrom;
  private static readonly ImmutableList<string> WithdrawnAllowedFrom = PendingAllowedFrom;

  /// <summary>
  /// Validate if the status change from one value to another is allowed.
  /// In future this logic should be moved into a proposal repository or potentially a workflow service.
  /// </summary>
  /// <param name="fromStatus">Current status value.</param>
  /// <param name="toStatus">Target status value.</param>
  /// <param name="message">Error message when result is false.</param>
  /// <returns></returns>
  public static bool ValidateWorkflow(string fromStatus, string toStatus, out string? message)
  {
    message = null;

    if (string.IsNullOrEmpty(fromStatus) || string.IsNullOrEmpty(toStatus))
    {
      message = "Status cannot be blank.";
      return false;
    }

    if (!Values.Contains(fromStatus))
    {
      message = $"Status '{fromStatus}' is not allowed.";
      return false;
    }

    if (!Values.Contains(toStatus))
    {
      message = $"New status '{toStatus}' is not allowed.";
      return false;
    }

    if (fromStatus == toStatus)
    {
      message = $"Status is already '{toStatus}'. Please refresh the screen.";
      return false;
    }

    if (toStatus == ProposalStatus.Pending.ToDescription() && !PendingAllowedFrom.Contains(fromStatus))
    {
      message = $"Status '{toStatus}' is not allowed from '{fromStatus}'.";
      return false;
    }

    if (toStatus == ProposalStatus.Closed.ToDescription() && !ClosedAllowedFrom.Contains(fromStatus))
    {
      message = $"Status '{toStatus}' is not allowed from '{fromStatus}'.";
      return false;
    }

    if (toStatus == ProposalStatus.Cancelled.ToDescription() && !CancelledAllowedFrom.Contains(fromStatus))
    {
      message = $"Status '{toStatus}' is not allowed from '{fromStatus}'.";
      return false;
    }

    if (toStatus == ProposalStatus.Withdrawn.ToDescription() && !WithdrawnAllowedFrom.Contains(fromStatus))
    {
      message = $"Status '{toStatus}' is not allowed from '{fromStatus}'.";
      return false;
    }

    if (toStatus == ProposalStatus.Rework.ToDescription() && !ReworkAllowedFrom.Contains(fromStatus))
    {
      if (fromStatus == ProposalStatus.Closed.ToDescription())
      {
        message = $"The submission has been '{ProposalStatus.Closed.ToDescription()}'. Please refresh the screen.";
      }
      else
      {
        message = $"Status '{toStatus}' is not allowed from '{fromStatus}'.";
      }
      return false;
    }

    return true;
  }
}
