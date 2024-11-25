namespace Product.Web.Models.Entity
{
    public class Meta
    {
        public Pagination Pagination { get; set; }
    }

    public class Pagination
    {
        public int CurrentPage { get; set; }
        public int PerPage { get; set; }
        public int From { get; set; }
        public int To { get; set; }
        public int Total { get; set; }
        public int LastPage { get; set; }
    }

    public class Entity
    {
        public LegalName LegalName { get; set; }

        public LegalAddress LegalAddress { get; set; }
        public LegalAddress HeadquartersAddress { get; set; }
        public RegisteredAt RegisteredAt { get; set; }
        public string RegisteredAs { get; set; }
        public string Jurisdiction { get; set; }
        public string Category { get; set; }
        public AssociatedEntity AssociatedEntity { get; set; }
        public string Status { get; set; }


        public List<object> SuccessorEntities { get; set; }
        public DateTime? CreationDate { get; set; }
        public string SubCategory { get; set; }
        public List<object> OtherAddresses { get; set; }
        public List<object> EventGroups { get; set; }
    }

    public class LegalName
    {
        public string Name { get; set; }

    }

    public class LegalAddress
    {
        public List<string> AddressLines { get; set; }
        public string AddressNumber { get; set; }
        public string AddressNumberWithinBuilding { get; set; }
        public string MailRouting { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
    }

    public class RegisteredAt
    {
        public string Id { get; set; }
        public object Other { get; set; }
    }


    public class AssociatedEntity
    {
        public string Lei { get; set; }
        public string Name { get; set; }
    }

    public class Registration
    {
        public DateTime InitialRegistrationDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public string Status { get; set; }
        public DateTime? NextRenewalDate { get; set; }
        public string ManagingLou { get; set; }
        public string CorroborationLevel { get; set; }
        public RegisteredAt ValidatedAt { get; set; }
        public string ValidatedAs { get; set; }
        public List<object> OtherValidationAuthorities { get; set; }
    }


    public class ReportingException
    {
        public string Url { get; set; }
    }

    public class LeiRecord
    {      
        public string Id { get; set; }
        public Attributes Attributes { get; set; }
       
    }

    public class Attributes
    {
        public string Lei { get; set; }
        public Entity Entity { get; set; }
        public Registration Registration { get; set; }
        //public string Bic { get; set; }
        public string ConformityFlag { get; set; }
    }

    public class LEIRecordsResponse
    {
        public Meta Meta { get; set; }
        public List<LeiRecord> Data { get; set; }
    }

}
