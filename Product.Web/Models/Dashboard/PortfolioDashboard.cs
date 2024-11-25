using Product.Dal.Entities;

namespace Product.Web.Models.Dashboard
{
    public class PortfolioDashboard
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public string CreditRating { get; set; }
        public string InternalCreditRating { get; set; }        
        public string AnnualReviewDate { get; set; }
        public string NextReviewDate { get; set; }
        public double TotalLimits { get; set; }
        public Image ClientImage { get; set; }
        public bool IsHigher { get; set; }        
        public string Ticker { get; set; }
    }
}
