﻿using Product.Dal.Entities;

namespace Product.Dal.Common.Data.Seed;

public static class ClientDraftSeedData
{
    public static List<ClientDraft> Get(Dictionary<string, int> clientLookUp, Dictionary<int, int> proposalLookUp)
    {
        return new List<ClientDraft>
            {
                new ClientDraft { ClientId = clientLookUp["BHP Billiton Group"],ProposalId=proposalLookUp[clientLookUp["BHP Billiton Group"]],Status="Draft",Version = 0, ImageId=3, IsDeleted = false, Name = "BHP Billiton Group", BasicInformation = $"{{\"ClientName\":\"BHP Billiton Group\",\"ClientSegment\":\"Multinational\",\"CounterpartyCreditRating\":\"BBB\",\"EntityType\":\"Group\", \"BusinessUnit\":\"Retail banking\",\"Location\":\"Australia\",\"Ticker\":\"BHP\",\"NextReviewDate\":\"{DateTime.Now.AddDays(6).ToString("yyyy-MM-dd")}\"}}", OtherInformation = null, CreatedBy = "system", ModifiedBy = "john", TemplateId = 1 },
                new ClientDraft { ClientId = clientLookUp["Down the rabbit hole ltd"],ProposalId=proposalLookUp[clientLookUp["Down the rabbit hole ltd"]],Status="Draft",Version = 0, IsDeleted = false, Name = "Down the rabbit hole ltd", BasicInformation = $"{{ \"ClientName\":\"Down the rabbit hole ltd\", \"ClientSegment\":\"CORPORATE\", \"CounterpartyCreditRating\":\"AA-\", \"EntityType\":\"Legal Entity\", \"BusinessUnit\":\"Retail banking\", \"Location\":\"Australia\", \"Ticker\":\"FFA\", \"NextReviewDate\":\"{DateTime.Now.AddDays(3).ToString("yyyy-MM-dd")}\" }}", OtherInformation = null,  CreatedBy = "system", ModifiedBy = "john", TemplateId = 1 },
                new ClientDraft { ClientId = clientLookUp["Downer group"],ProposalId=proposalLookUp[clientLookUp["Downer group"]],Status="Draft",Version = 0,ImageId=4, IsDeleted = false, Name = "Downer group", BasicInformation = $"{{ \"ClientName\":\"Downer group\", \"ClientSegment\":\"CORPORATE\", \"CounterpartyCreditRating\":\"BBB+\", \"EntityType\":\"Legal Entity\", \"BusinessUnit\":\"Retail banking\", \"Location\":\"American Samoa\", \"Ticker\":\"DG\", \"NextReviewDate\":\"{DateTime.Now.AddMonths(6).ToString("yyyy-MM-dd")}\" }}", OtherInformation = null, CreatedBy = "system", ModifiedBy = "john", TemplateId = 1 },
                new ClientDraft { ClientId = clientLookUp["Crown Resorts"],ProposalId=proposalLookUp[clientLookUp["Crown Resorts"]],Status="Draft",Version = 0,ImageId=1,  IsDeleted = false, Name = "Crown Resorts", BasicInformation = $"{{ \"ClientName\":\"Crown Resorts\", \"ClientSegment\":\"CORPORATE\", \"CounterpartyCreditRating\":\"BBB-\", \"EntityType\":\"Subsidiary\", \"BusinessUnit\":\"Retail banking\", \"Location\":\"China\", \"Ticker\":\"CR\", \"NextReviewDate\":\"{DateTime.Now.AddMonths(6).ToString("yyyy-MM-dd")}\" }}", OtherInformation = null, CreatedBy = "system", ModifiedBy = "john", TemplateId = 1 },
                new ClientDraft { ClientId = clientLookUp["Hilton Group"],ProposalId=proposalLookUp[clientLookUp["Hilton Group"]],Status="Draft",Version = 0,ImageId=2, IsDeleted = false, Name = "Hilton Group", BasicInformation = $"{{ \"ClientName\":\"Hilton Group\", \"ClientSegment\":\"CONSUMER & SERVICES\", \"CounterpartyCreditRating\":\"BBB\", \"EntityType\":\"Group\", \"BusinessUnit\":\"Retail banking\", \"Location\":\"China\", \"Ticker\":\"HG\", \"NextReviewDate\":\"{DateTime.Now.AddMonths(6).ToString("yyyy-MM-dd")}\" }}", OtherInformation = null, CreatedBy = "system", ModifiedBy = "john", TemplateId = 1 },
                new ClientDraft { ClientId = clientLookUp["Reliance industries Ltd"],ProposalId=proposalLookUp[clientLookUp["Reliance industries Ltd"]],Status="Draft",Version = 0,IsDeleted = false, Name = "Reliance industries Ltd", BasicInformation = $"{{ \"ClientName\":\"Reliance industries Ltd\", \"ClientSegment\":\"OIL AND GAS\", \"CounterpartyCreditRating\":\"AA-\", \"EntityType\":\"Subsidiary\", \"BusinessUnit\":\"Retail banking\", \"Location\":\"Cambodia\", \"Ticker\":\"RI\", \"NextReviewDate\":\"{DateTime.Now.AddMonths(6).ToString("yyyy-MM-dd")}\" }}", OtherInformation = null, CreatedBy = "system", ModifiedBy = "john", TemplateId = 1 },
                new ClientDraft { ClientId = clientLookUp["Atlassian group ltd"],ProposalId=proposalLookUp[clientLookUp["Atlassian group ltd"]],Status="Draft",Version = 0,ImageId=5, IsDeleted = false, Name = "Atlassian group ltd", BasicInformation = $"{{ \"ClientName\":\"Atlassian group ltd\", \"ClientSegment\":\"TELECOMM, MEDIA, ENTERTAINMENT & TECH\", \"CounterpartyCreditRating\":\"AA-\", \"EntityType\":\"Group\", \"BusinessUnit\":\"Retail banking\", \"Location\":\"Australia\", \"Ticker\":\"AG\", \"NextReviewDate\":\"{DateTime.Now.AddMonths(6).ToString("yyyy-MM-dd")}\" }}", OtherInformation = null, CreatedBy = "system", ModifiedBy = "john", TemplateId = 1 }
            };
    }
}