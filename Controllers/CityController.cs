using NewLagosFloodDetectionSystem.Data;
using NewLagosFloodDetectionSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace NewLagosFloodDetectionSystem.Controllers
{
    [Authorize(Roles = "admin")]
    public class CityController : Controller
    {
        private readonly FloodDbContext _context;
        public CityController(FloodDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var cities = _context.Cities.AsNoTracking()
                .Include(x => x.LGA)
                .Where(x => x.Name != "None")
                .ToList();
            return View(cities);
        }

        public IActionResult CreateCity()
        {
            var lgas = _context.LGAs.Where(x => x.LGAName != "None").ToList();
            ViewBag.LGAs = new SelectList(lgas, "LGAId", "LGAName");
            return View();
        }

        [HttpPost]
        public IActionResult CreateCity(City city)
        {
            _context.Cities.Add(city);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, City cityModel)
        {
            if (!string.IsNullOrEmpty(cityModel.Name))
            {
                var city = await _context.Cities.FindAsync(id);
                city.Name = cityModel.Name;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }
        public async Task<IActionResult> Detail(int id)
        {
            var predictions = await _context.CityPredictions.Where(x => x.CityId == id).ToListAsync();
            var cities = await _context.Cities.Where(x => x.LGAId == id).ToListAsync();
            foreach (var prediction in predictions)
            {
                var cityChk = _context.Cities.Find(id);
                cityChk.Predictions = predictions.ToList();
                var city = await _context.Cities.Where(x => x.Predictions.Contains(prediction)).FirstOrDefaultAsync();

                return View(city);
            }

            return View();
        }
        public async Task<IActionResult> DeleteCity()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> DeleteCity(int id)
        {
            var city = _context.Cities.Find(id);
            if (city == null)
            {
                return RedirectToAction(nameof(Index));
            }
            _context.Cities.Remove(city);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


    }
}
