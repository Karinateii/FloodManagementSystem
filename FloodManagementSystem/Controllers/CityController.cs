using GlobalDisasterManagement.Data;
using GlobalDisasterManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GlobalDisasterManagement.Controllers
{
    [Authorize(Roles = "admin")]
    public class CityController : Controller
    {
        private readonly DisasterDbContext _context;
        public CityController(DisasterDbContext context)
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

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var city = await _context.Cities.FindAsync(id);
            if (city == null)
            {
                return NotFound();
            }

            ViewBag.LGAs = await _context.LGAs.OrderBy(l => l.LGAName).ToListAsync();
            return View(city);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, City cityModel)
        {
            if (id != cityModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var city = await _context.Cities.FindAsync(id);
                    if (city == null)
                    {
                        return NotFound();
                    }

                    city.Name = cityModel.Name;
                    city.LGAId = cityModel.LGAId;

                    _context.Update(city);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = "City updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error updating city: " + ex.Message);
                }
            }

            ViewBag.LGAs = await _context.LGAs.OrderBy(l => l.LGAName).ToListAsync();
            return View(cityModel);
        }
        public async Task<IActionResult> Detail(int id)
        {
            var city = await _context.Cities
                .Include(c => c.LGA)
                .Include(c => c.Predictions)
                .FirstOrDefaultAsync(c => c.Id == id);
            
            if (city == null)
            {
                return NotFound();
            }
            
            return View(city);
        }
        [HttpGet]
        public async Task<IActionResult> DeleteCity(int id)
        {
            var city = await _context.Cities.FindAsync(id);
            if (city == null)
            {
                return RedirectToAction(nameof(Index));
            }
            return View(city);
        }
        [HttpPost]
        [ActionName("DeleteCity")]
        public async Task<IActionResult> DeleteCityConfirmed(int id)
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
