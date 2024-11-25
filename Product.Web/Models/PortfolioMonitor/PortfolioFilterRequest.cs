using System;

namespace Product.Web.Models.PortfolioMonitor;

public class PortfolioFilterRequest
{
    public string ClientIds { get; set; }

    public int? FilterId { get; set; }

    public string? Description { get; set; }

    public string Title { get; set; }

    public string MonitorType { get; set; }
}
