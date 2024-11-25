namespace Product.Dal.Interfaces;

public interface IAuditLog
{
    public DateTime Timestamp { get; set; }
    public string Username { get; set; }
    public string HostAddress { get; set; }
    public string HostName { get; set; }
    public string Action { get; set; }
}
