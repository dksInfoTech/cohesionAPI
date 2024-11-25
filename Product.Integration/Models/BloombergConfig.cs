namespace Product.Integration.Models
{
    public class BloombergConfig
    {
        required public string AuthApiEndpoint { get; set; }
        required public string DataApiEndpoint { get; set; }
        required public string DataLicense { get; set; }
        required public string ClientId { get; set; }
        required public string ClientSecret { get; set; }
        required public string GrantType { get; set; }
        required public string FinMetaDataIdentifier { get; set; }
        required public string FinMetaDataPageSize { get; set; }
    }
}