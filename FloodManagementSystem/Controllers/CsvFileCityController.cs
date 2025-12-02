using GlobalDisasterManagement.Data;
using GlobalDisasterManagement.Models;
using GlobalDisasterManagement.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace GlobalDisasterManagement.Controllers
{
    [Authorize(Roles = "admin")]
    public class CsvFileCityController : Controller
    {
        private readonly DisasterDbContext _context;
        private readonly IWebHostEnvironment _env;
        public CsvFileCityController(DisasterDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public IActionResult Index()
        {
            var files = _context.CsvFileCities.ToList();
            return View(files);
        }

        public IActionResult Upload()
        {
            var cities = _context.Cities.Where(x => x.Name != "None").ToList();
            ViewBag.Cities = new SelectList(cities, "Id", "Name");
            var models = _context.CsvFiles.ToList();
            ViewBag.Models = new SelectList(models, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file, PredictionFileViewModel model)
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

                        var csvFileId = Guid.NewGuid();
                        var path = Path.Combine(_env.WebRootPath, "Uploads", csvFileId.ToString() + ".csv");
                        TempData["CsvFileId"] = csvFileId;
                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        var objfiles = new CsvFileCity()
                        {
                            Id = csvFileId,
                            Name = fileName,
                            UploadDateTime = DateTime.Now,
                            Path = path,
                            Year = model.Year,
                            CityId = model.CityId,
                            ModelId = model.ModelId
                        };

                        using (var target = new MemoryStream())
                        {
                            file.CopyTo(target);
                            objfiles.Bytes = target.ToArray();
                        }
                        var yearCheck = _context.CsvFileCities.FirstOrDefault(x => x.Year ==  model.Year && x.CityId == model.CityId);
                        if(yearCheck == null)
                        {
                            _context.CsvFileCities.Add(objfiles);
                            _context.SaveChanges();
                        }
                        else
                        {
                            return View();
                        }
                        return RedirectToAction("Predict", "FloodData", new { id = csvFileId, cityid = objfiles.CityId, modelid = objfiles.ModelId, year = objfiles.Year});
                    }

                }
            }
            return View();
        }

        public async Task<IActionResult> Edit()
        {
            var cities = _context.Cities.Where(x => x.Name != "None").ToList();
            ViewBag.Cities = new SelectList(cities, "Id", "Name");
            var models = _context.CsvFiles.ToList();
            ViewBag.Models = new SelectList(models, "Id", "Name");
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Edit(Guid id,IFormFile file, PredictionFileViewModel model)
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
                        var objfiles = new CsvFileCity()
                        {
                            Id = id,
                            Name = fileName,
                            UploadDateTime = DateTime.Now,
                            Path = path,
                            Year = model.Year,
                            CityId = model.CityId,
                            ModelId = model.ModelId

                        };

                        using (var target = new MemoryStream())
                        {
                            file.CopyTo(target);
                            objfiles.Bytes = target.ToArray();
                        }

                        var yearCheck = _context.CsvFileCities.FirstOrDefault(x => x.Year == model.Year && x.CityId == model.CityId);
                        if (yearCheck == null)
                        {
                           await _context.SaveChangesAsync();
                        }
                        else
                        {
                            return View();
                        }
                        return RedirectToAction("Predict", "FloodData", new { id = id, cityid = objfiles.CityId, modelid = objfiles.ModelId, year = objfiles.Year });

                    }
                }
            }
            return View();
        }

        public async Task<IActionResult> Detail(Guid id)
        {
            var predFiles = await _context.CsvFileCities.FindAsync(id);
            return View(predFiles);
        }


        public async Task<IActionResult> Delete(Guid id)
        {
            // Get the file path
            string filePath = Path.Combine(_env.WebRootPath, "Uploads", id.ToString() + ".csv");
            var predFiles = await _context.CsvFileCities.FindAsync(id);
            var preds = await _context.CityPredictions.Where(x => x.FileId == id).ToListAsync();


            // Check if the file exists
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            // Delete the file
            System.IO.File.Delete(filePath);
            _context.CsvFileCities.Remove(predFiles);
            _context.CityPredictions.RemoveRange(preds);
            await _context.SaveChangesAsync();
            // Redirect to the index page or wherever you want to redirect
            return RedirectToAction(nameof(Index));
        }


    }
}
