using System.Web;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Product.Bal;
using Product.Bal.Interfaces;
using Product.Bal.Models;
using Product.Dal;
using Product.Dal.Common.Extensions;
using Product.Dal.Common.Workflow;
using Product.Dal.Entities;
using Product.Dal.Enums;
using Product.Web.Models.Client;
using Product.Web.Models.Proposal;
using Product.Web.Models.Response;
using Product.Web.Models.User;

namespace Product.Web.Controllers;

[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly DBContext _db;
    private readonly IMapper _mapper;
    private readonly Settings _settings;
    private readonly UserContextService _userContextService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserController(DBContext db, IMapper mapper, IOptions<Settings> settings,UserContextService userContextService,IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _mapper = mapper;
        _settings = settings.Value;
        _userContextService = userContextService;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// List of application users.
    /// </summary>
    /// <returns></returns>
    [HttpGet("List")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult Get()
    {
        //var users = _db.Users.OrderBy(c => c.FirstName).ToList()
        //.Select(c => new { c.FirstName, c.LastName, c.Name, c.Id, c.Role, c.Country, c.SystemAdmin, c.Active, c.Image, c.LastAccessDate, c.Email, c.UserRoleId });

        var users = _db.Users
               .AsNoTracking() // Optional: Use if you don't need change tracking
               .OrderBy(c => c.FirstName)
               .Select(c => new
               {
                   c.FirstName,
                   c.LastName,
                   c.Name,
                   c.Id,
                   c.Role,
                   c.Country,
                   c.SystemAdmin,
                   c.Active,
                   c.Image,
                   c.LastAccessDate,
                   c.Email,
                   c.UserRoleId
               });

        return Ok(users.ToList());
    }


    /// <summary>
    /// List of application users.
    /// </summary>
    /// <returns></returns>
    [HttpGet("Login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult Login()
    {
          var users = _db.Users
               .AsNoTracking() // Optional: Use if you don't need change tracking               
               .Select(c => new
               {
                   c.FirstName,
                   c.LastName,
                   c.Name,
                   c.Id,
                   c.Role,
                   c.Country,
                   c.SystemAdmin,
                   c.Active,                  
                   c.Email,
                   c.UserRoleId
               });

        return Ok(users.ToList());
    }

    /// <summary>
    /// Search for application users.
    /// </summary>
    /// <param name="q">Search string.</param>
    /// <param name="clientId">Client identifier.</param>
    /// <returns></returns>
    [HttpGet("Search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult Search(string? q = null, int? clientId = null)
    {
        var userQuery = _db.Users.Where(x => x.Active).ToList();

        if(clientId.HasValue)
        {
            var existingTeamMembers = _db.ClientUserAccessMapping
                                        .Where(c => c.ClientId == clientId && (c.Write || c.Approve || c.Admin))
                                        .Select(s => s.UserId).ToList();
            userQuery = userQuery.Where(u => !existingTeamMembers.Contains(u.Id)).ToList();
        }

        if (!string.IsNullOrEmpty(q))
        {
            // Search within the user name
            userQuery = userQuery.Where(x => x.Name.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // Select the Id and Name only, then convert into KeyValuePair
        var users = userQuery
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Image
            })
            .OrderBy(x => x.Name)
            .Take(100)
            .ToList()
            .Select(x => new KeyValuePair<string, object[]>(x.Id, [x.Name, x.Image]));

        return Ok(users);
    }

    /// <summary>
    /// Create a user.
    /// </summary>
    /// <param name="user">User information.</param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Create([FromBody] SaveUserRequest user)
    {
        if (user == null || string.IsNullOrEmpty(user.Id))
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "Request is empty" });
        }

        // Enforce lowercase convention for user Id
        user.Id = user.Id.ToLower();

        if (_db.Users.Any(x => x.Id == user.Id))
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "The user already exists" });
        }

        var newUser = new User
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            UserRoleId = user.UserRoleId,
            Country = user.Country,
            Active = user.Active,
            SystemAdmin = user.SystemAdmin,
            Email = string.IsNullOrEmpty(user.Email) ? null : user.Email,
            ExpirationDate = user.ExpirationDate,
            Password = user.Password,
            ImageId = user.ImageId,
        };

        _db.Users.Add(newUser);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.Message });
        }

        return Ok(new ActionResponse { Result = 0 });
    }

    /// <summary>
    /// Delete a user.
    /// </summary>
    /// <param name="id">User identifier.</param>
    /// <returns></returns>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete([FromBody] string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "User Id is blank." });
        }

        var user = _db.Users.Find(id.ToLower());

        if (user == null)
        {
            return NotFound(new ActionResponse { Result = 1, Message = "User not found." });
        }

        _db.ClientTeamMembers.RemoveRange(_db.ClientTeamMembers.Where(x => x.ClientUserMap.UserId == id));
        // Remove linked clients
        _db.ClientUserAccessMapping.RemoveRange(_db.ClientUserAccessMapping.Where(x => x.UserId == id));

        // Remove user
        _db.Users.Remove(user);

        try
        {
            // Commit
            await _db.SaveChangesAsync();
        }
        catch (Exception e)
        {
            // Drill down to the Npgsql exception
            var baseException = e.GetBaseException() as Npgsql.NpgsqlException;

            if (baseException != null)
            {
                // FK error is thrown with a 547
                if (baseException.ErrorCode == 547)
                {
                    return Ok(new ActionResponse
                    {
                        Result = 1,
                        Message = "The user is needed for audit and cannot be deleted. Set inactive instead."
                    });
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = baseException.Message });
                }
            }

            return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.Message });
        }

        return Ok(new ActionResponse { Result = 0 });
    }

    /// <summary>
    /// Update a user
    /// </summary>
    /// <param name="user">User information.</param>
    /// <returns></returns>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Edit([FromBody] SaveUserRequest user)
    {
        if (user == null || string.IsNullOrEmpty(user.Id))
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "User Id is blank." });
        }

        // Find the user record
        var editUser = _db.Users.Find(user.Id.ToLower());

        if (editUser == null)
        {
            return NotFound(new ActionResponse { Result = 1, Message = "User not found." });
        }

        // Update the changed properties
        // editUser.FirstName = user.FirstName;
        // editUser.LastName = user.LastName;
        editUser.UserRoleId = user.UserRoleId;
        editUser.Country = user.Country;
        editUser.Active = user.Active;
        editUser.SystemAdmin = user.SystemAdmin;
        editUser.Email = string.IsNullOrEmpty(user.Email) ? null : user.Email;
        editUser.ExpirationDate = user.ExpirationDate;
        editUser.ImageId = user.ImageId;

        try
        {
            // Save the changes
            await _db.SaveChangesAsync();
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.Message });
        }

        return Ok(new ActionResponse { Result = 0 });
    }

    /// <summary>
    /// Update a user password
    /// </summary>
    /// <param name="user">User information.</param>
    /// <returns></returns>
    [HttpPut("Password/Change")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdatePassword([FromBody] SaveUserRequest user)
    {
        if (user == null || string.IsNullOrEmpty(user.Id))
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "User Id is blank." });
        }

        // Find the user record
        var editUser = _db.Users.Find(user.Id.ToLower());

        if (editUser == null)
        {
            return NotFound(new ActionResponse { Result = 1, Message = "User not found." });
        }

        // Update the changed properties           
        editUser.Password = user.Password;
        try
        {
            // Save the changes
            await _db.SaveChangesAsync();
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.Message });
        }

        return Ok(new ActionResponse { Result = 0 });
    }
}
