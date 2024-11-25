using Product.Dal;
using Product.Dal.Entities;

namespace Product.Bal.Interfaces;

public interface IProposalService
{
    Proposal? Get(int id);
    ClientDefinition? GetClient(int proposalId);
    ClientDefinition? GetDiffClient(int proposalId);
    Task<bool> Finalize(Proposal proposal, DBContext context);
    void DeleteDashboardEvents(int clientId, int proposalId);
}
