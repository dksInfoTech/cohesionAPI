using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Product.Dal;

namespace Product.Bal;

public class UserContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Settings _settings;

    public UserContextService(IHttpContextAccessor httpContextAccessor, IOptions<Settings> settings)
    {
        _httpContextAccessor = httpContextAccessor;
        _settings = settings.Value;
    }

    /// <summary>
    /// Get the current user's LAN ID from the user claim (from JWT) or the Windows based authentication.
    /// </summary>
    /// <returns></returns>
    public string GetUserId()
    {

        string? userId = Environment.UserName.ToLower();
        // if (_httpContextAccessor.HttpContext?.User?.Claims != null && _httpContextAccessor.HttpContext.User.Claims.Any())
        // {
        //     // Get the user Id from the "WindowsAccountName" claim
        //     userId = _httpContextAccessor.HttpContext?.User?.Claims?.SingleOrDefault(x => x.Type == ClaimTypes.UserData)?.Value;
        // }

        if (userId == null)
        {
            // Get the user Id from the "NameIdentifier" claim (this is set by the JWT configuration in Startup)
            // Then convert the user Id from user@domain format to domain\user format
            if (_httpContextAccessor.HttpContext.Items.ContainsKey("User"))
            {
                userId = _httpContextAccessor.HttpContext.Items["User"].ToString();
            }

            if (userId != null)
            {
                // Add the user Id as a new "WindowsAccountName" claim for future use
                // This saves time parsing the user Id multiple times from user@domain format to domain\user format
                _httpContextAccessor.HttpContext.User.AddIdentity(new ClaimsIdentity(new List<Claim>
                                                                    {
                                                                        new Claim(ClaimTypes.WindowsAccountName, userId)
                                                                    }));
            }
            else
            {
                userId = _httpContextAccessor.HttpContext?.User?.Identity?.Name?.ToLower();
            }
        }

        return userId;
    }
}
