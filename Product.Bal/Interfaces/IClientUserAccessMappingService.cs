using Product.Dal.Entities;

namespace Product.Bal.Interfaces;

public interface IClientUserAccessMappingService
{
    /// <summary>
    /// Get all clients that are accessible to a user.
    /// </summary>
    /// <param name="userId">User Id (defaults to the current user context).</param>
    /// <returns></returns>
    IQueryable<int> GetAllowedClientIds(string userId = null);

    /// <summary>
    /// Get all clients that are accessible to a user.
    /// </summary>
    /// <param name="userId">User Id (defaults to the current user context).</param>
    /// <returns></returns>
    IQueryable<Client> GetAllowedClients(string userId = null);

    /// <summary>
    /// Get all users with access to a client.
    /// </summary>
    /// <param name="clientId">Client Id.</param>
    /// <returns></returns>
    IQueryable<string> GetAllowedUserIds(int clientId);

    /// <summary>
    /// Get all users with access to a client.
    /// </summary>
    /// <param name="clientId">Client Id.</param>
    /// <returns></returns>
    IQueryable<User> GetAllowedUsers(int clientId);
}
