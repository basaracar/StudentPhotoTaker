using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StudentPhotoTaker.Models;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Linq;
using System.IO.Compression;

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
            return BadRequest("Öğrenci ID veya resim boş olamaz.");
        }

        var photoParts = request.Photo.Split(',');
        if (photoParts.Length != 2)
        {
            return BadRequest("Hatalı resim formatı.");
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

            if (System.IO.File.Exists(filePath))
            {
                return Conflict($"{request.Sinif} sınıfında {request.StudentId} numaralı öğrenci zaten kayıt edilmiş .");
            }

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
            Folders = Directory.GetDirectories(photosPath).Select(Path.GetFileName).OrderBy(f => f).ToList()
        };

        if (string.IsNullOrEmpty(id))
        {
            id = "9A";
         }    
            var directoryPath = Path.Combine(photosPath, id);
            if (Directory.Exists(directoryPath))
            {
                model.CurrentFolder = id;
                model.Photos = Directory.GetFiles(directoryPath)
                                        .Select(p => $"/photos/{id}/{Path.GetFileName(p)}")
                                        .OrderBy(p => p)
                                        .ToList();
            }

        return View(model);
    }

    public IActionResult DownloadFolder(string id)
    {
        var photosPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "photos");
        var directoryPath = Path.Combine(photosPath, id);

        if (!Directory.Exists(directoryPath))
        {
            return NotFound();
        }

        var tempZipPath = Path.GetTempFileName() + ".zip";
        ZipFile.CreateFromDirectory(directoryPath, tempZipPath);

        var memory = new MemoryStream();
        using (var stream = new FileStream(tempZipPath, FileMode.Open))
        {
            stream.CopyTo(memory);
        }
        memory.Position = 0;

        System.IO.File.Delete(tempZipPath);

        return File(memory, "application/zip", $"{id}.zip");
    }

    public IActionResult DownloadAllFolders()
    {
        var photosPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "photos");

        if (!Directory.Exists(photosPath))
        {
            return NotFound();
        }

        var tempZipPath = Path.GetTempFileName() + ".zip";
        ZipFile.CreateFromDirectory(photosPath, tempZipPath);

        var memory = new MemoryStream();
        using (var stream = new FileStream(tempZipPath, FileMode.Open))
        {
            stream.CopyTo(memory);
        }
        memory.Position = 0;

        System.IO.File.Delete(tempZipPath);

        return File(memory, "application/zip", "tum_fotolar.zip");
    }

    [HttpPost]
    public IActionResult DeletePhoto([FromBody] SavePhotoRequest request)
    {
        if (string.IsNullOrEmpty(request.Photo))
        {
            return BadRequest("Resim yolu boş olamaz.");
        }

        var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var filePath = Path.Combine(webRootPath, request.Photo.TrimStart('/'));

        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
            return Ok();
        }

        return NotFound();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }


}
