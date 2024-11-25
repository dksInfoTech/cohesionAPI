using System;
using Product.Bal.Interfaces;
using Product.Bal.Models;
using Product.Dal;
using Product.Dal.Entities;

namespace Product.Bal;

public class FacilityService : IFacilityService
{
    private readonly DBContext _db;

    public FacilityService(DBContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Create/Update a draft facility for the proposal.
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="proposalId"></param>
    /// <param name="facility"></param>
    /// <returns></returns>
    public async Task<Facility> SaveFacilityDraft(SaveFacilityRequest facility)
    {
        // c1 p1 f1
        // c1 p2 f1

        // Find the latest facility
        var fac = _db.Facilities.FirstOrDefault(x => x.Id == facility.Id && x.ProposalId == facility.ProposalId && x.IsActive);

        // Create the draft version from latest version
        if (fac == null)
        {
            var facId = _db.Facilities.Any() ? _db.Facilities.Max(x => x.FacilityId) : 0;
            // version will be always 1
            fac = new Facility
            {
                ClientId = facility.ClientId,
                ProposalId = facility.ProposalId,
                IsDraft = true,
                IsActive = true,
                IsLatest = false,
                FacilityInfo = facility.FacilityInfo,
                Comments = facility.Comments,
                FacilityDocumentId = facility.FacilityDocumentId,
                InterchangableLimitId = facility.InterchangableLimitId,
                FacilityId = facId != 0 ? facId + 1 : 1000
            };
            fac.Version++;
            _db.Facilities.Add(fac);
        }
        else
        {
            // same version will be update
            fac.IsDraft = true;
            fac.IsActive = true;
            fac.IsLatest = false;
            fac.FacilityInfo = facility.FacilityInfo;
            fac.Comments = facility.Comments;
            fac.FacilityDocumentId = facility.FacilityDocumentId;
            fac.InterchangableLimitId = facility.InterchangableLimitId;
        }

        await _db.SaveChangesAsync();

        return fac;
    }

    public async Task<FacilityDocument> SaveFacilityDocumentDraft(SaveFacilityDocumentRequest facDocument)
    {
        // c1 p1 f1
        // c1 p2 f1

        // Find the latest facilityDoc
        var facilityDoc = _db.FacilityDocuments.FirstOrDefault(x => x.Id == facDocument.Id && x.ProposalId == facDocument.ProposalId && x.IsActive);

        // Create the draft version from latest version
        if (facilityDoc == null)
        {
            var facDocId = _db.FacilityDocuments.Any()? _db.FacilityDocuments.Max(x => x.FacilityDocumentId): 0;
            // version will be always 1
            facilityDoc = new FacilityDocument
            {
                ClientId = facDocument.ClientId,
                ProposalId = facDocument.ProposalId,
                IsDraft = true,
                IsActive = true,
                IsLatest = false,
                FacilityDocumentInfo = facDocument.FacilityDocumentInfo,
                Comments = facDocument.Comments,
                FacilityDocumentId = facDocId != 0 ? facDocId + 1 : 1000,
            };
            facilityDoc.Version++;
            _db.FacilityDocuments.Add(facilityDoc);
        }
        else
        {
            // same version will be update
            facilityDoc.IsDraft = true;
            facilityDoc.IsActive = true;
            facilityDoc.IsLatest = false;
            facilityDoc.FacilityDocumentInfo = facDocument.FacilityDocumentInfo;
            facilityDoc.Comments = facDocument.Comments;
        }
        await _db.SaveChangesAsync();

        return facilityDoc;
    }

    public async Task<InterchangableLimit> SaveInterchangableLimitsDraft(SaveInterchangableLimitRequest interchangableLimitRequest)
    {
        // c1 p1 f1
        // c1 p2 f1

        // Find the latest facilityDoc
        var interchangableLimit = _db.InterchangableLimits.FirstOrDefault(x => x.Id == interchangableLimitRequest.Id
                                    && x.ProposalId == interchangableLimitRequest.ProposalId && x.IsActive);

        // Create the draft version from latest version
        if (interchangableLimit == null)
        {
            var ilId = _db.InterchangableLimits
                        .Max(x => x.InterchangableLimitId);
            // version will be always 1
            interchangableLimit = new InterchangableLimit
            {
                ClientId = interchangableLimitRequest.ClientId,
                ProposalId = interchangableLimitRequest.ProposalId,
                IsDraft = true,
                IsActive = true,
                IsLatest = false,
                InterchangableLimitInfo = interchangableLimitRequest.InterchangableLimitInfo,
                Comments = interchangableLimitRequest.Comments,
                FacilityDocumentId = interchangableLimitRequest.FacilityDocumentId,
                InterchangableLimitId = ilId != 0 ? ilId + 1 : 1000,
            };
            interchangableLimit.Version++;
            _db.InterchangableLimits.Add(interchangableLimit);
        }
        else
        {
            // same version will be update
            interchangableLimit.IsDraft = true;
            interchangableLimit.IsActive = true;
            interchangableLimit.IsLatest = false;
            interchangableLimit.InterchangableLimitInfo = interchangableLimitRequest.InterchangableLimitInfo;
            interchangableLimit.Comments = interchangableLimitRequest.Comments;
            interchangableLimit.FacilityDocumentId = interchangableLimitRequest.FacilityDocumentId;
        }
        await _db.SaveChangesAsync();

        return interchangableLimit;
    }

    public async Task CloneLatestFacility(int clientId, int proposalId)
    {
        // c1 p1 f1
        // c1 p2 f1

        // Find the latest facility
        var facs = _db.Facilities.Where(x => x.ClientId == clientId && x.IsLatest && x.IsActive).ToList();

        foreach (var fac in facs)
        {
            // Create the draft version from latest version
            if (fac != null)
            {
                // version will be always 1
                var newFac = new Facility
                {
                    ClientId = clientId,
                    ProposalId = proposalId,
                    IsDraft = true,
                    IsActive = true,
                    IsLatest = false,
                    FacilityInfo = fac.FacilityInfo,
                    Comments = fac.Comments,
                    FacilityDocumentId = fac.FacilityDocumentId,
                    InterchangableLimitId = fac.InterchangableLimitId,
                    FacilityId = fac.FacilityId,
                    Version = fac.Version + 1
                };
                _db.Facilities.Add(newFac);
            }
        }
        await _db.SaveChangesAsync();
    }

    public async Task CloneLatestFacilityDocument(int clientId, int proposalId)
    {
        // c1 p1 f1
        // c1 p2 f1

        // Find the latest facilityDoc
        var facilityDocs = _db.FacilityDocuments.Where(x => x.ClientId == clientId && x.IsLatest && x.IsActive).ToList();

        foreach (var facilityDoc in facilityDocs)
        {
            // Create the draft version from latest version
            if (facilityDoc != null)
            {
                // version will be always 1
                var newFacilityDoc = new FacilityDocument
                {
                    ClientId = clientId,
                    ProposalId = proposalId,
                    IsDraft = true,
                    IsActive = true,
                    IsLatest = false,
                    FacilityDocumentInfo = facilityDoc.FacilityDocumentInfo,
                    Comments = facilityDoc.Comments,
                    FacilityDocumentId = facilityDoc.FacilityDocumentId,
                    Version = facilityDoc.Version + 1
                };
                _db.FacilityDocuments.Add(newFacilityDoc);
            }
        }
        await _db.SaveChangesAsync();
    }

    public async Task CloneLatestInterchangableLimit(int clientId, int proposalId)
    {
        // c1 p1 f1
        // c1 p2 f1

        // Find the latest facilityDoc
        var interchangableLimits = _db.InterchangableLimits.Where(x => x.ClientId == clientId && x.IsLatest && x.IsActive).ToList();
        foreach (var interchangableLimit in interchangableLimits)
        {
            // Create the draft version from latest version
            if (interchangableLimit != null)
            {
                // version will be always 1
                var newInterchangableLimit = new InterchangableLimit
                {
                    ClientId = clientId,
                    ProposalId = proposalId,
                    IsDraft = true,
                    IsActive = true,
                    IsLatest = false,
                    InterchangableLimitInfo = interchangableLimit.InterchangableLimitInfo,
                    Comments = interchangableLimit.Comments,
                    InterchangableLimitId = interchangableLimit.InterchangableLimitId,
                    FacilityDocumentId = interchangableLimit.FacilityDocumentId,
                    Version = interchangableLimit.Version + 1
                };
                _db.InterchangableLimits.Add(newInterchangableLimit);
            }
        }
        await _db.SaveChangesAsync();
    }
}
