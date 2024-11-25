namespace Product.Web.Models.Entity
{
    public class EntityCreate
    {
        public int ClientId { get; set; }

        public string Name { get; set; }

        public string Country { get; set; }

        public string Industry { get; set; }

        public string SourceSystemName { get; set; }

        public string SourceId { get; set; }

        public string? EntityType { get; set; }

        public string? PolicyCountry { get; set; }

        public string? PolicyStatus { get; set; }

        public bool? IsValidCustomer { get; set; }

        public string? CCR { get; set; }

        public string? SI { get; set; }

        public string? Ticker { get; set; }

        public string? SourceEntityInformation { get; set; }
    }
}
