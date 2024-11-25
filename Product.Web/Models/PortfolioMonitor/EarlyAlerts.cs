namespace Product.Web.Models.PortfolioMonitor
{
    public class EarlyAlerts
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public string Title { get; set; }
        public string Comments { get; set; }
        public string Metric { get; set; }
    }
}
