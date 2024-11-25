using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Product.Dal;

namespace Product.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiHealthController : ControllerBase
    {
        private readonly DBContext _db;

        public ApiHealthController(DBContext db)
        {
            _db = db;
        }

        [HttpGet("Get/Status")]
        public async Task<IActionResult> GetStatus()
        {
            // Datasource TO BE REMOVED: Added for testing purposes
            var data = new
            {
                status = "up",
                env = GetEnvVar("ASPNETCORE_ENVIRONMENT"),
                image = GetEnvVar("IMAGE_TAG"),
                db = _db.Database.GetDbConnection().DataSource
            };

            return Ok(data);
        }


        private string GetEnvVar(string name)
        {
            return Environment.GetEnvironmentVariable(name) ?? "Not set";
        }

    }
}