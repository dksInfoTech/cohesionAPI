namespace Product.Dal;

public class Settings
{
    public ConnectionStringsSettingsSection ConnectionStringsSettings { get; set; }
    public IList<string> AllowedImageExtensions { get; set; }
    public IList<string> AllowedImageContentTypes { get; set; }
}

public class ConnectionStringsSettingsSection
{
    public bool DbConnection { get; set; }
}
