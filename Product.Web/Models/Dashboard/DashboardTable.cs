namespace Product.Web.Models.Dashboard;

public class DashboardTable
{
    public int TotalCount { get; set; }
    public IEnumerable<ClientDashboard> Collection { get; set; }
}
