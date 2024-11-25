using Product.Dal.Entities;

namespace Product.Dal.Common.Data.Seed;

public static class ProposalSeedData
{
    public static List<Proposal> Get(Dictionary<string,int> clientLookUp)
    {
        return
        new List<Proposal>
        {
            new Proposal {   CreatedBy = "system", ModifiedBy = "john",  ClientId = clientLookUp["BHP Billiton Group"], ClientVersion = 1, Title = "BHP Sub1", Status = "Draft", Decision = "", IsClientUpdate = true , ProposalInfo = $"{{ \"ProposalSummary\":\"BHP Sub1\",\"ProposalDueDate\":\"{DateTime.Now.AddMonths(6).ToString("yyyy-MM-dd")}\",\"Priority\":\"High\",\"Delegationauthority\":\"CRO\",\"ProposalIsScheduledReview\":true }}", },
            new Proposal {   CreatedBy = "system", ModifiedBy = "user",  ClientId = clientLookUp["Down the rabbit hole ltd"], ClientVersion = 1, Title = "DTRH Sub 1", Status = "Draft", Decision = "", IsClientUpdate = true , ProposalInfo = $"{{ \"ProposalSummary\":\"FFA Submission 1\",\"ProposalDueDate\":\"{DateTime.Now.AddMonths(6).ToString("yyyy-MM-dd")}\",\"Priority\":\"High\",\"Delegationauthority\":\"CRO\",\"ProposalIsScheduledReview\":true }}", },
            new Proposal {   CreatedBy = "system", ModifiedBy = "user",  ClientId = clientLookUp["Downer group"], ClientVersion = 1, Title = "DG Sub1", Status = "Draft", Decision = "", IsClientUpdate = true , ProposalInfo = $"{{ \"ProposalSummary\":\"DG Sub1\",\"ProposalDueDate\":\"{DateTime.Now.AddMonths(6).ToString("yyyy-MM-dd")}\",\"Priority\":\"Medium\",\"Delegationauthority\":\"CRO\",\"ProposalIsScheduledReview\":true }}", },
            new Proposal {   CreatedBy = "system", ModifiedBy = "user",  ClientId = clientLookUp["Crown Resorts"], ClientVersion = 1, Title = "CR Sub1", Status = "Draft", Decision = "", IsClientUpdate = true , ProposalInfo = $"{{ \"ProposalSummary\":\"CR 1\",\"ProposalDueDate\":\"{DateTime.Now.AddMonths(6).ToString("yyyy-MM-dd")}\",\"Priority\":\"Low\",\"Delegationauthority\":\"CRO\",\"ProposalIsScheduledReview\":true }}",},
            new Proposal {   CreatedBy = "system", ModifiedBy = "user",  ClientId = clientLookUp["Hilton Group"], ClientVersion = 1, Title = "HG Sub1", Status = "Draft", Decision = "", IsClientUpdate = true , ProposalInfo = $"{{ \"ProposalSummary\":\"HG1\",\"ProposalDueDate\":\"{DateTime.Now.AddMonths(6).ToString("yyyy-MM-dd")}\",\"Priority\":\"High\",\"Delegationauthority\":\"CRO\",\"ProposalIsScheduledReview\":true }}", },
            new Proposal {   CreatedBy = "system", ModifiedBy = "user",  ClientId = clientLookUp["Reliance industries Ltd"], ClientVersion = 1, Title = "RI Sub1", Status = "Draft", Decision = "", IsClientUpdate = true , ProposalInfo = $"{{ \"ProposalSummary\":\"RI 1\",\"ProposalDueDate\":\"{DateTime.Now.AddMonths(6).ToString("yyyy-MM-dd")}\",\"Priority\":\"Medium\",\"Delegationauthority\":\"CRO\",\"ProposalIsScheduledReview\":true }}", },
            new Proposal {   CreatedBy = "system", ModifiedBy = "john", ClientId = clientLookUp["Atlassian group ltd"], ClientVersion = 1, Title = "AG Sub1", Status = "Draft", Decision = "", IsClientUpdate = true , ProposalInfo = $"{{ \"ProposalSummary\":\"AG1 Submission\",\"ProposalDueDate\":\"{DateTime.Now.AddMonths(6).ToString("yyyy-MM-dd")}\",\"Priority\":\"Medium\",\"Delegationauthority\":\"CRO\",\"ProposalIsScheduledReview\":true }}",},
        };

    }
}
