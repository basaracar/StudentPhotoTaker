using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StudentPhotoTaker.Models;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Linq;

namespace StudentPhotoTaker.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public IActionResult SavePhoto([FromBody] SavePhotoRequest request)
    {
        if (string.IsNullOrEmpty(request.StudentId) || string.IsNullOrEmpty(request.Photo))
        {
            return BadRequest("Student ID and photo are required.");
        }

        var photoParts = request.Photo.Split(',');
        if (photoParts.Length != 2)
        {
            return BadRequest("Invalid photo format.");
        }

        var imageBytes = Convert.FromBase64String(photoParts[1]);

        using (var image = Image.Load(imageBytes))
        {
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(133, 171),
                Mode = ResizeMode.Crop
            }));

            var directory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "photos",request.Sinif);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var fileName = $"{request.StudentId}.jpg";
            var filePath = Path.Combine(directory, fileName);

            image.Save(filePath);

            return Ok(new { filePath = $"/photos/{request.Sinif}/{fileName}" });
        }

        return Ok();
    }
    [Route("Fotolar/{id?}")]
    public IActionResult Fotolar(string id)
    {
        var photosPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "photos");
        if (!Directory.Exists(photosPath))
        {
            Directory.CreateDirectory(photosPath);
        }

        var model = new FotolarViewModel
        {
            Folders = Directory.GetDirectories(photosPath).Select(Path.GetFileName).ToList()
        };

        if (!string.IsNullOrEmpty(id))
        {
            var directoryPath = Path.Combine(photosPath, id);
            if (Directory.Exists(directoryPath))
            {
                model.CurrentFolder = id;
                model.Photos = Directory.GetFiles(directoryPath).Select(p => $"/photos/{id}/{Path.GetFileName(p)}").ToList();
            }
        }

        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
