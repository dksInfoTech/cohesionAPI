using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Product.Dal;
using Product.Dal.Entities;
using Product.Web.Models.Response;

namespace Product.Web.Controllers;

[Route("api/[controller]")]
public class ImageController : ControllerBase
{
    private readonly DBContext _db;
    private readonly Settings _settings;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ImageController(DBContext db, IOptions<Settings> settings, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _settings = settings.Value;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Upload an image.
    /// </summary>
    /// <returns></returns>
    [HttpPost()]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Post()
    {
        var file = _httpContextAccessor.HttpContext.Request.Form.Files.Count > 0 ? _httpContextAccessor.HttpContext.Request.Form.Files["file"] : null;

        // Validate the required inputs are provided
        if (!_httpContextAccessor.HttpContext.Request.Form.ContainsKey("category"))
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "Image category is missing." });
        }
        var imageCategory = _httpContextAccessor.HttpContext.Request.Form["category"].ToString();
        Image? image;
        try
        {
            image = await CreateOrUpdateImage(file, imageCategory);
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.Message });
        }

        return Ok(new ActionSaveResponse
            {
                Result = 0,
                ModelId = image.Id,
                ModifiedBy = image.ModifiedBy,
                ModifiedDate = image.ModifiedDate,
                ModifiedDateString = string.Format("{0:dd-MMM-yyyy HH:mm:ss}", image.ModifiedDate),
            });
    }

    /// <summary>
    /// Update an image.
    /// </summary>
    /// <returns></returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Put(int id)
    {
        var file = _httpContextAccessor.HttpContext.Request.Form.Files.Count > 0 ? _httpContextAccessor.HttpContext.Request.Form.Files["file"] : null;

        // Validate the required inputs are provided
        if (id == 0)
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "Missing image Id." });
        }
        else if (!_httpContextAccessor.HttpContext.Request.Form.ContainsKey("category"))
        {
            return BadRequest(new ActionResponse { Result = 1, Message = "Image category is missing." });
        }
        var imageCategory = _httpContextAccessor.HttpContext.Request.Form["category"].ToString();
        Image? image;
        try
        {
            image = await CreateOrUpdateImage(file, imageCategory, id);
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ActionResponse { Result = 1, Message = e.Message });
        }

        return Ok(new ActionSaveResponse
            {
                Result = 0,
                ModelId = image.Id,
                ModifiedBy = image.ModifiedBy,
                ModifiedDate = image.ModifiedDate,
                ModifiedDateString = string.Format("{0:dd-MMM-yyyy HH:mm:ss}", image.ModifiedDate),
            });
    }

    private async Task<Image> CreateOrUpdateImage(IFormFile? file, string imageCategory, int? imageId = null)
    {

        // Validate the required inputs are provided          
        if (file == null || file.Length == 0)
        {
            throw new Exception("No file to upload.");
        }

        string filename = HttpUtility.UrlDecode(file.FileName);
        string extension = Path.GetExtension(filename);

        // Validate the file type
        if (string.IsNullOrEmpty(extension) || !_settings.AllowedImageExtensions.Contains(extension.ToLower()))
        {
            throw new Exception("The file extension is not allowed '" + extension + "'" );
        }
        else if (string.IsNullOrEmpty(file.ContentType) || !_settings.AllowedImageContentTypes.Contains(file.ContentType))
        {
            throw new Exception("The file content-type is not allowed '" + file.ContentType + "'");
        }
        // Load file into byte array
        byte[] data;
        using (Stream inputStream = file.OpenReadStream())
        {
            var memoryStream = inputStream as MemoryStream;
            if (memoryStream == null)
            {
                memoryStream = new MemoryStream();
                inputStream.CopyTo(memoryStream);
            }
            data = memoryStream.ToArray();
        }

        // Validate the data is present
        if (data == null || data.Length == 0)
        {
            throw new Exception("No file data.");
        }

        var image = imageId.HasValue ? _db.Images.FirstOrDefault(x => x.Id == imageId.Value && x.ImageCategory == imageCategory) : null;
        if (image == null)
        {
            // Create image record
            image = new Image
            {
                ImageCategory = imageCategory,
                FileName = Path.GetFileName(filename),
                Extension = extension,
                ContentType = file.ContentType,
                ContentSize = data.Length,
                Content = data
            };

            // Create the image and save to get the new Id
            _db.Images.Add(image);
        }
        else
        {
            // Update image record
            image.FileName = Path.GetFileName(filename);
            image.Extension = extension;
            image.ContentType = file.ContentType;
            image.ContentSize = data.Length;
            image.Content = data;


            _db.Entry(image).Property(p => p.FileName).IsModified = true;
            _db.Entry(image).Property(p => p.Extension).IsModified = true;
            _db.Entry(image).Property(p => p.ContentType).IsModified = true;
            _db.Entry(image).Property(p => p.ContentSize).IsModified = true;
            _db.Entry(image).Property(p => p.Content).IsModified = true;
        }

        try
        {
            await _db.SaveChangesAsync();
            return image;
        }
        catch (Exception)
        {
            // error log
            throw;
        }
    }
}
