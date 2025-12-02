using GlobalDisasterManagement.Data;
using GlobalDisasterManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlobalDisasterManagement.Controllers
{
    [Authorize(Roles = "admin")]
    public class CsvFileController : Controller
    {
        private readonly DisasterDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<CsvFileController> _logger;
        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

        public CsvFileController(DisasterDbContext context, IWebHostEnvironment env, ILogger<CsvFileController> logger)
        {
            _context = context;
            _env = env;
            _logger = logger;
        }
        public IActionResult Index()
        {
            var files = _context.CsvFiles.ToList();
            return View(files);

        }
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    ModelState.AddModelError("", "Please select a file to upload.");
                    return View();
                }

                // Validate file size
                if (file.Length > MaxFileSize)
                {
                    ModelState.AddModelError("", $"File size cannot exceed {MaxFileSize / 1024 / 1024}MB.");
                    return View();
                }

                // Validate file extension
                var fileName = Path.GetFileName(file.FileName);
                var fileExtension = Path.GetExtension(fileName)?.ToLower();
                
                if (fileExtension != ".csv")
                {
                    ModelState.AddModelError("", "Only CSV files are allowed.");
                    return View();
                }

                // Sanitize filename
                fileName = Path.GetFileNameWithoutExtension(fileName)
                    .Replace(" ", "_")
                    .Replace("-", "_");
                fileName = $"{fileName}{fileExtension}";

                var csvFileId = Guid.NewGuid();
                var uploadsDir = Path.Combine(_env.WebRootPath, "Uploads");
                
                // Ensure uploads directory exists
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }

                var path = Path.Combine(uploadsDir, csvFileId.ToString() + ".csv");
                
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var objfiles = new CsvFile
                {
                    Id = csvFileId,
                    Name = fileName,
                    UploadDateTime = DateTime.Now,
                    Path = path,
                };

                using (var target = new MemoryStream())
                {
                    await file.CopyToAsync(target);
                    objfiles.Bytes = target.ToArray();
                }

                await _context.CsvFiles.AddAsync(objfiles);
                await _context.SaveChangesAsync();

                _logger.LogInformation("CSV file uploaded successfully: {FileName} (ID: {FileId})", fileName, csvFileId);
                TempData["Success"] = "File uploaded successfully.";
                
                return RedirectToAction("Train", "FloodData", new { id = csvFileId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading CSV file");
                ModelState.AddModelError("", "An error occurred while uploading the file.");
                return View();
            }
        }

        public async Task<IActionResult> Edit()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, IFormFile file)
        {
            if (file != null)
            {
                if (file.Length > 0)
                {
                    //Getting FileName
                    var fileName = Path.GetFileName(file.FileName);
                    //Getting file Extension
                    var fileExtension = Path.GetExtension(fileName);
                    if (fileExtension == ".csv")
                    {

                        var path = Path.Combine(_env.WebRootPath, "Uploads", id.ToString() + ".csv");
                        // Delete the existing file
                        if (System.IO.File.Exists(path))
                        {
                            System.IO.File.Delete(path);
                        }

                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        var objfiles = new CsvFile()
                        {
                            Id = id,
                            Name = fileName,
                            UploadDateTime = DateTime.Now,
                            Path = path,
                        };

                        using (var target = new MemoryStream())
                        {
                            file.CopyTo(target);
                            objfiles.Bytes = target.ToArray();
                        }

                        //_context.CsvFiles.Add(objfiles);
                        await _context.SaveChangesAsync();
                        return RedirectToAction("Train", "FloodData", new { id });
                    }
                }
            }
            return View();
        }

        public async Task<IActionResult> Detail(Guid id)
        {
            var mdlFiles = await _context.CsvFiles.FindAsync(id);
            return View(mdlFiles);
        }


        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var csvFile = await _context.CsvFiles.FindAsync(id);
            if (csvFile == null)
            {
                return RedirectToAction(nameof(Index));
            }
            return View(csvFile);
        }

        [HttpPost]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            // Get the file path
            string filePath = Path.Combine(_env.WebRootPath, "Uploads", id.ToString() + ".csv");
            string modelPath = Path.Combine(_env.WebRootPath, "Uploads", id.ToString() + ".zip");
            var mdlFile = await _context.CsvFiles.FindAsync(id);

            // Check if the file exists
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            // Delete the file
            System.IO.File.Delete(filePath);
            System.IO.File.Delete(modelPath);
            _context.CsvFiles.Remove(mdlFile);
            await _context.SaveChangesAsync();

            // Redirect to the index page or wherever you want to redirect
            return RedirectToAction(nameof(Index));
        }
    }
}
