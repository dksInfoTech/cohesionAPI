namespace Product.Web.Models.Facility
{
    public class FacilityViewModel
    {
        public string FacilityName { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public string ProposalTitle { get; set; }
        public int ProposalId { get; set; }
        public string ProposalStatus { get; set; }       
        public string ProposedLimit { get; set; }
        public string ApprovedLimit { get; set; }
        public string FacilityInfo { get; set; }
    }
}
