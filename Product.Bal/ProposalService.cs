using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Product.Bal.Interfaces;
using Product.Dal;
using Product.Dal.Common.Extensions;
using Product.Dal.Common.Workflow;
using Product.Dal.Entities;
using Product.Dal.Enums;
using System.Xml.Linq;

namespace Product.Bal;

public class ProposalService : IProposalService
{

    private readonly DBContext _db;
    private readonly IClientService _clientService;
    private readonly IMapper _mapper;
    private readonly UserContextService _userContextService;

    public ProposalService(DBContext db, IClientService clientService,
        IMapper mapper, UserContextService userContextService)
    {
        _db = db;
        _clientService = clientService;
        _mapper = mapper;
        _userContextService = userContextService;
    }
    public async Task<bool> Finalize(Proposal proposal, DBContext context)
    {
        DateTime dt = DateTime.Now;

        if (proposal == null)
        {
            return false;
        }

        // Determine if the proposal is complete (it's either been declined, or all participants have made a final decision that isn't Rework)
        bool declinedDecisions = proposal.ProposalTeamMembers.Any(x => x.IsFinal && x.Decision == ProposalDecision.Declined.ToDescription());
        bool finalDecisions = proposal.ProposalTeamMembers.All(x => x.IsFinal && x.Decision != ProposalDecision.Rework.ToDescription());
        bool isProposalDecisionCompleted = declinedDecisions || finalDecisions;

        if (!isProposalDecisionCompleted)
        {
            return false;
        }

        // Set the final decision at the proposal level
        string finalDecision = GetFinalDecision(proposal.ProposalTeamMembers);

        // Update the proposal
        proposal.Status = ProposalStatus.Closed.ToDescription();
        proposal.Decision = finalDecision;
        proposal.ClientVersion = _clientService.GetClientVersionNumber(proposal.ClientId);
        proposal.ClosedDate = dt;
        context.Entry(proposal).Property(p => p.Status).IsModified = true;
        context.Entry(proposal).Property(p => p.Decision).IsModified = true;
        context.Entry(proposal).Property(p => p.ClientVersion).IsModified = true;
        context.Entry(proposal).Property(p => p.ClosedDate).IsModified = true;

        // Update team member final flags if the proposal is all done
        if (finalDecisions)
        {
            foreach (var p in proposal.ProposalTeamMembers.Where(x => !x.IsFinal))
            {
                p.IsFinal = true;
            }
        }

        // Identify and process any required updates to the client and draft
        if (proposal.IsClientUpdate)
        {
            if (finalDecision == ProposalDecision.Declined.ToDescription())
            {
                DeleteLimitDrafts(proposal, context);
                // Delete the draft if the proposal was declined
                await _clientService.DeleteDraft(proposal.ClientId, proposal.Id);
                DeleteDashboardEvents(proposal.ClientId, proposal.Id);

                // delete facility
                // delete facilitydocument
                // delete events dashboard
            }
            else
            {
                PublishDraftLimits(proposal, context);
                // Copy the draft to the master client table and delete the draft

                // Find the draft
                var draft = context.ClientDrafts.FirstOrDefault(x => x.ClientId == proposal.ClientId && !x.IsDeleted);

                // Find the client
                var client = context.Clients.Find(proposal.ClientId);

                if (draft != null && client != null && draft.ProposalId == proposal.Id)
                {
                    // Update the master client
                    _mapper.Map(draft, client);

                    // insert data to history table as well
                    SaveClientHistory(draft, context);


                    // Delete the draft
                    context.ClientDrafts.Remove(draft);

                    // Increment the client version number to match the new approved version
                    proposal.ClientVersion = client.Version + 1;
                }
            }
        }
        return true;
    }

    public void DeleteDashboardEvents(int clientId,int proposalId)
    {
        var events = _db.EventDashboard.Where(x => x.ClientId == clientId && x.ProposalId == proposalId);

        if (events.Any())
        {
            // Delete the drafts
            _db.EventDashboard.RemoveRange(events);
            _db.SaveChangesAsync();
        }
    }

    private void SaveClientHistory(ClientDraft draft, DBContext db)
    {
        // create new data
        //ClientHistory newClientHist = new ClientHistory();

        var newClient = new ClientHistory
        {
            Name = draft.Name,
            BasicInformation = draft.BasicInformation,
            OtherInformation = draft.OtherInformation,
            TemplateId = draft.TemplateId,
            ImageId = draft.ImageId,
            ProposalId = draft.ProposalId,
            ClientId = draft.ClientId
        };

        //_mapper.Map(draft, newClient);

        db.ClientHistory.Add(newClient);
        db.SaveChanges();
    }

    private void PublishDraftLimits(Proposal proposal, DBContext db)
    {
        var draftPreviousDocuments = db.FacilityDocuments.Where(f => proposal.ClientId == f.ClientId && f.IsLatest && f.IsActive);

        if (draftPreviousDocuments != null)
        {
            // here we need to create new version of facility
            foreach (var prevDoc in draftPreviousDocuments)
            {
                prevDoc.IsDraft = false;
                prevDoc.IsLatest = false;
                //prevDoc.Version++;
                db.Entry(prevDoc).Property(p => p.IsLatest).IsModified = true;
                db.Entry(prevDoc).Property(p => p.IsDraft).IsModified = true;
                //db.Entry(prevDoc).Property(p => p.Version).IsModified = true;
            }
        }

        var draftDocuments = db.FacilityDocuments.Where(f => proposal.ClientId == f.ClientId
                                                       && proposal.Id == f.ProposalId
                                                       && f.IsDraft && f.IsActive);
        if (draftDocuments != null)
        {
            // here we need to create new version of facility
            foreach (var draftDoc in draftDocuments)
            {
                draftDoc.IsDraft = false;
                draftDoc.IsLatest = true;
               // draftDoc.Version++;
                db.Entry(draftDoc).Property(p => p.IsLatest).IsModified = true;
                db.Entry(draftDoc).Property(p => p.IsDraft).IsModified = true;
               // db.Entry(draftDoc).Property(p => p.Version).IsModified = true;
            }
        }

        var draftPreviousFacilities = db.Facilities.Where(f => proposal.ClientId == f.ClientId                                                        
                                                        && f.IsLatest && f.IsActive);
        if (draftPreviousFacilities != null)
        {
            // here we need to create new version of facility
            foreach (var prevFacility in draftPreviousFacilities)
            {
                prevFacility.IsDraft = false;
                prevFacility.IsLatest = false;
                //prevFacility.Version++;
                db.Entry(prevFacility).Property(p => p.IsLatest).IsModified = true;
                db.Entry(prevFacility).Property(p => p.IsDraft).IsModified = true;
                //db.Entry(prevFacility).Property(p => p.Version).IsModified = true;
            }
        }

        var draftFacilities = db.Facilities.Where(f => proposal.ClientId == f.ClientId 
                                                        && proposal.Id == f.ProposalId 
                                                        && f.IsDraft && f.IsActive);
        if (draftFacilities != null)
        {
            // here we need to create new version of facility
            foreach (var draftFacility in draftFacilities)
            {
                draftFacility.IsDraft = false;
                draftFacility.IsLatest = true;
                //draftFacility.Version++;
                db.Entry(draftFacility).Property(p => p.IsLatest).IsModified = true;
                db.Entry(draftFacility).Property(p => p.IsDraft).IsModified = true;
                //db.Entry(draftFacility).Property(p => p.Version).IsModified = true;
            }            
        }
    }

    private void DeleteLimitDrafts(Proposal proposal, DBContext db)
    {
        var draftDocument = db.FacilityDocuments.FirstOrDefault(f => proposal.ClientId == f.ClientId
                                                       && proposal.Id == f.ProposalId
                                                       && f.IsDraft && f.IsActive);
        if (draftDocument != null)
        {
            db.FacilityDocuments.Remove(draftDocument);
        }

        var draftFacility = db.Facilities.FirstOrDefault(f => proposal.ClientId == f.ClientId 
                                                        && proposal.Id == f.ProposalId 
                                                        && f.IsDraft && f.IsActive);
        if (draftFacility != null)
        {
            db.Facilities.Remove(draftFacility);          
        }
    }

    public Proposal? Get(int id)
    {
        var proposal = _db.Proposals
                .Include(x => x.ProposalEvents)
                .Include(x => x.ProposalTeamMembers).ThenInclude(c => c.User)
                .Include(x => x.LastContributor)
                .AsNoTracking()                     // Do not allow updates back to DB after fetching the data
                .FirstOrDefault(x => x.Id == id);

        // if (proposal != null)
        // {
        //     string currentUserId = _userContextService.GetUserId();

        //     proposal.ProposalTeamMembers.Where(s => s.Role != ProposalRole.Task.ToDescription()).ToList().ForEach(p =>
        //         {
        //             if (!p.IsFinal && p.UserId != currentUserId)
        //             {
        //                 p.Decision = null;
        //             }
        //         });

        // }

        return proposal;
    }

    public ClientDefinition? GetClient(int proposalId)
    {
        ClientDefinition? client = null;

        // Get the proposal
        var proposal = _db.Proposals.Find(proposalId);

        if (proposal == null)
        {
            return null;
        }

        if (ProposalWorkFlowStatus.ClosedValues.Contains(proposal.Status))
        {
            // The proposal is closed -> Get the associated client version

            // Get the current client by default
            client = _db.Clients.FirstOrDefault(x => x.Id == proposal.ClientId && !x.IsDeleted);

            // Get the historical client if needed
            if (proposal.ClientVersion != null && client != null && proposal.ClientVersion != client.Version)
            {
                if (proposal.ClientVersion == 0)
                {
                    // If client is not v1, then find client v1 is in the history table
                    // Else if client is v1, do nothing, return the current client v1
                    if (client.Version != 1)
                    {
                        client = _db.ClientHistory.AsNoTracking().FirstOrDefault(x => x.ClientId == proposal.ClientId && x.Version == 1 && !x.IsDeleted);
                    }
                }
                else
                {
                    // Find the historical client
                    client = _db.ClientHistory.AsNoTracking().FirstOrDefault(x => x.ClientId == proposal.ClientId
                                                                            && x.Version == proposal.ClientVersion && !x.IsDeleted);
                }
            }
        }
        else if (proposal.IsClientUpdate)
        {
            // The proposal is open and there is a client update -> Get the client draft
            client = _db.ClientDrafts.FirstOrDefault(x => x.ClientId == proposal.ClientId && !x.IsDeleted);
        }
        else
        {
            // The proposal is open and there is no client update -> Get the current client
            client = _db.Clients.FirstOrDefault(x => x.Id == proposal.ClientId && !x.IsDeleted);
        }

        return client;
    }

    public ClientDefinition? GetDiffClient(int proposalId)
    {
        ClientDefinition? client = null;

        // Get the proposal
        var proposal = _db.Proposals.Find(proposalId);

        if (proposal == null)
        {
            return null;
        }

        if (ProposalWorkFlowStatus.ClosedValues.Contains(proposal.Status))
        {
            // Only require a diff when there is a client update
            if (proposal.IsClientUpdate && proposal.ClientVersion != null)
            {
                if (proposal.Decision == ProposalDecision.Declined.ToDescription() || proposal.Status == ProposalStatus.Withdrawn.ToDescription())
                {
                    client = _db.Clients.FirstOrDefault(x => x.Id == proposal.ClientId && x.Version == proposal.ClientVersion && !x.IsDeleted);

                    if (client == null)
                    {
                        client = _db.ClientHistory.AsNoTracking()
                        .FirstOrDefault(x => x.ClientId == proposal.ClientId
                                        && x.Version == proposal.ClientVersion && !x.IsDeleted);
                    }
                }
                else
                {
                    // The proposal is closed -> Get the prior historical client version
                    client = _db.ClientHistory.AsNoTracking()
                    .FirstOrDefault(x => x.ClientId == proposal.ClientId
                                    && x.Version == proposal.ClientVersion - 1 && !x.IsDeleted);
                }
            }
        }
        else if (proposal.IsClientUpdate)
        {
            // The proposal is open and there is a client update -> Get the current client
            client = _db.Clients.FirstOrDefault(x => x.Id == proposal.ClientId && !x.IsDeleted);
        }
        else
        {
            // The proposal is open and there is no client update -> Get the prior historical client version to the current client
            var currentClientVersion = _db.Clients.FirstOrDefault(x => x.Id == proposal.ClientId && !x.IsDeleted)?.Version;

            if (currentClientVersion != null)
            {
                client = _db.ClientHistory.AsNoTracking()
                .FirstOrDefault(x => x.ClientId == proposal.ClientId
                                && x.Version == currentClientVersion - 1 && !x.IsDeleted);
            }
        }

        return client;
    }

    /// <summary>
    /// Identify the final decision for a proposal.
    /// </summary>
    /// <param name="teamMembers"></param>
    /// <returns></returns>
    private string GetFinalDecision(IEnumerable<ProposalTeamMember> teamMembers)
    {
        if (teamMembers.Any(x => x.Decision == ProposalDecision.Declined.ToDescription()))
        {
            return ProposalDecision.Declined.ToDescription();
        }
        else if (teamMembers.Any(x => x.Decision == ProposalDecision.Approved.ToDescription()))
        {
            return ProposalDecision.Approved.ToDescription();
        }
        else if (teamMembers.Any(x => x.Decision == ProposalDecision.Noted.ToDescription()))
        {
            return ProposalDecision.Noted.ToDescription();
        }
        else if (teamMembers.Any(x => x.Decision == ProposalDecision.Unopposed.ToDescription()))
        {
            return ProposalDecision.Unopposed.ToDescription();
        }
        else if (teamMembers.Any(x => x.Decision == ProposalDecision.TaskCompleted.ToDescription()))
        {
            return ProposalDecision.TaskCompleted.ToDescription();
        }
        else
        {
            return ProposalDecision.Supported.ToDescription();
        }
    }
}
