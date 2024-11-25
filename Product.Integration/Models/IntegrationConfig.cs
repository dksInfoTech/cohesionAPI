namespace Product.Integration.Models
{
    public class IntegrationConfig
    {
        required public BloombergConfig BloombergConfig { get; set; }
        required public ZeroMqConfig ZeroMqConfig { get; set; }
    }
}