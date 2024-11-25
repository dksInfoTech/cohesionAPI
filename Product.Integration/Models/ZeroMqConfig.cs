namespace Product.Integration.Models
{
    public class ZeroMqConfig
    {
        required public string ApiServiceSubUrl { get; set; }
        required public string FdeServiceSubPort { get; set; }
        required public string ApiServiceSubPort { get; set; }
    }
}