using Microsoft.EntityFrameworkCore;
using Product.Bal.Interfaces;
using Product.Dal;
using Product.Dal.Common.Workflow;
using Product.Dal.Entities;

namespace Product.Bal;

public class ClientService : IClientService
{
    private readonly DBContext _db;

    public ClientService(DBContext db)
    {
        _db = db;
    }

    public async Task DeleteDraft(int id, int? proposalId = null)
    {
        // Get the drafts (there should only be one, but select all with the same client Id)
        IQueryable<ClientDraft> drafts = _db.ClientDrafts.Where(x => x.ClientId == id && !x.IsDeleted);

        // Optionally check the draft is linked to the input proposal Id
        if (proposalId != null)
        {
            drafts = drafts.Where(x => x.ProposalId == proposalId);
        }

        if (drafts.Any())
        {
            // Delete the drafts
            _db.ClientDrafts.RemoveRange(drafts);
            await _db.SaveChangesAsync();
        }
    }

    public ClientDefinition GetClient(int id, int? v = null, bool draft = false)
    {
        // Find the latest client
        var client = _db.Clients.Find(id);

        if (client == null || client.IsDeleted)
        {
            return null;
        }

        if (draft)
        {
            // Get the draft profile
            return _db.ClientDrafts.FirstOrDefault(x => x.ClientId == id && !x.IsDeleted);
        }

        if (v != null && v != client.Version)
        {
            // Get the historical profile
            var history = _db.ClientHistory.AsNoTracking().FirstOrDefault(x => x.ClientId == id && x.Version == v && !x.IsDeleted);
            return history;
        }

        return client;
    }

    public int? GetClientVersionNumber(int clientId)
    {
        return _db.Clients.FirstOrDefault(x => x.Id == clientId && !x.IsDeleted)?.Version;
    }

    public ClientDefinition GetDiffClient(int id, int? v = null, bool draft = false)
    {
        // Find the latest client
        var client = _db.Clients.Find(id);

        if (client == null || client.IsDeleted)
        {
            return null;
        }

        if (draft)
        {
            // Get the current client
            return client;
        }

        // Get the prior historical client
        v = v ?? client.Version;
        var history = _db.ClientHistory.AsNoTracking().FirstOrDefault(x => x.ClientId == id && x.Version == v - 1 && !x.IsDeleted);

        return history;
    }

    public int? GetDraftVersionNumber(int clientId)
    {
        return _db.ClientDrafts.FirstOrDefault(x => x.ClientId == clientId && !x.IsDeleted)?.Version;
    }

    public bool UpdateDraftStatus(string status, int id, int? proposalId = null)
    {
        if (string.IsNullOrEmpty(status) || !ProposalWorkFlowStatus.Values.Contains(status))
        {
            return false;
        }

        // Find the draft
        var draft = _db.ClientDrafts.FirstOrDefault(x => !x.IsDeleted && x.ClientId == id && x.ProposalId == proposalId);

        if (draft == null)
        {
            return false;
        }

        _db.ClientDrafts.Attach(draft);
        draft.Status = status;
        _db.Entry(draft).Property(p => p.Status).IsModified = true;

        return true;
    }
}
