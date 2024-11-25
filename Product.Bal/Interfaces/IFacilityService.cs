using System;
using Product.Bal.Models;
using Product.Dal.Entities;

namespace Product.Bal.Interfaces;

public interface IFacilityService
{
    Task<Facility> SaveFacilityDraft(SaveFacilityRequest facility);

    Task<FacilityDocument> SaveFacilityDocumentDraft(SaveFacilityDocumentRequest facDocument);

    Task<InterchangableLimit> SaveInterchangableLimitsDraft(SaveInterchangableLimitRequest facDocument);

    Task CloneLatestFacility(int clientId, int proposalId);

    Task CloneLatestFacilityDocument(int clientId, int proposalId);

    Task CloneLatestInterchangableLimit(int clientId, int proposalId);
}
