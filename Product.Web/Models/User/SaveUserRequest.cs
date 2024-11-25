namespace Product.Web.Models.User;

public class SaveUserRequest
{
    public string Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int UserRoleId { get; set; }
    public string Country { get; set; }
    public bool Active { get; set; }
    public bool SystemAdmin { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public int? ImageId { get; set; }
}
