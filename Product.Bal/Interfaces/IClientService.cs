using Product.Dal.Entities;

namespace Product.Bal.Interfaces;

public interface IClientService
{
    /// <summary>
    /// Get the current client version number.
    /// </summary>
    /// <param name="clientId">Client Id.</param>
    /// <returns></returns>
    int? GetClientVersionNumber(int clientId);

    /// <summary>
    /// Get the current client version number.
    /// </summary>
    /// <param name="clientId">Client Id.</param>
    /// <returns></returns>
    int? GetDraftVersionNumber(int clientId);

    /// <summary>
    /// Find the requested client, which may be the latest approved client, a ClientHistory or a ClientDraft.
    /// </summary>
    /// <param name="id">Client Id.</param>
    /// <param name="v">client version number.</param>
    /// <param name="draft">Get the draft client.</param>
    /// <returns></returns>
    ClientDefinition GetClient(int id, int? v = null, bool draft = false);

    /// <summary>
    /// Find the diff/comparison client, which may be the latest approved client, or a ClientHistory.
    /// </summary>
    /// <param name="id">Client Id.</param>
    /// <param name="v">Client version number.</param>
    /// <param name="draft">Get the diff for the draft client.</param>
    /// <returns></returns>
    ClientDefinition GetDiffClient(int id, int? v = null, bool draft = false);

    /// <summary>
    /// Delete draft client.
    /// </summary>
    /// <param name="id">Client Id.</param>
    /// <param name="proposalId">Optional proposal Id.</param>
    /// <param name="ignoreErrors">Optionally ignore errors.</param>
    Task DeleteDraft(int id, int? proposalId = null);

    /// <summary>
    /// Update the status of a client draft.
    /// </summary>
    /// <param name="status">New status.</param>
    /// <param name="id">Client Id.</param>
    /// <param name="proposalId">Optional proposal Id.</param>
    /// <returns></returns>
    bool UpdateDraftStatus(string status, int id, int? proposalId = null);
}
