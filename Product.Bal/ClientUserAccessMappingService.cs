using Product.Bal.Interfaces;
using Product.Dal.Entities;

namespace Product.Bal;

public class ClientUserAccessMappingService : IClientUserAccessMappingService
{
    public IQueryable<int> GetAllowedClientIds(string userId = null)
    {
        throw new NotImplementedException();
    }

    public IQueryable<Client> GetAllowedClients(string userId = null)
    {
        throw new NotImplementedException();
    }

    public IQueryable<string> GetAllowedUserIds(int clientId)
    {
        throw new NotImplementedException();
    }

    public IQueryable<User> GetAllowedUsers(int clientId)
    {
        throw new NotImplementedException();
    }
}
