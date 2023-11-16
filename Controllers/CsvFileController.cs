using NewLagosFloodDetectionSystem.Data;
using NewLagosFloodDetectionSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;

namespace NewLagosFloodDetectionSystem.Controllers
{
    [Authorize(Roles = "admin")]
    public class CsvFileController : Controller
    {
        private readonly FloodDbContext _context;
        private readonly IWebHostEnvironment _env;
        public CsvFileController(FloodDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
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
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file != null)
            {
                if (file.Length > 0)
                {
                    //Getting FileName
                    var fileName = Path.GetFileName(file.FileName);
                    //Getting file Extension
                    var fileExtension = Path.GetExtension(fileName);
                    if(fileExtension == ".csv")
                    {

                        var csvFileId = Guid.NewGuid();
                        var path = Path.Combine(_env.WebRootPath, "Uploads", csvFileId.ToString() + ".csv");
                        TempData["CsvFileId"] = csvFileId;
                        using(var stream = new FileStream(path, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        var objfiles = new CsvFile()
                        {
                            Id = csvFileId,
                            Name = fileName,
                            UploadDateTime = DateTime.Now,
                            Path = path,
                        };

                        using (var target = new MemoryStream())
                        {
                            file.CopyTo(target);
                            objfiles.Bytes = target.ToArray();
                        }

                        _context.CsvFiles.Add(objfiles);
                        _context.SaveChanges();
                        return RedirectToAction("Train", "FloodData", new { id = csvFileId});
                    }

                }
            }
            return View();
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


        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
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
