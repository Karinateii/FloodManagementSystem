using NewLagosFloodDetectionSystem.Data;
using NewLagosFloodDetectionSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace NewLagosFloodDetectionSystem.Controllers
{
    [Authorize(Roles = "admin")]
    public class LGAController : Controller
    {
        public readonly FloodDbContext _context;
        public LGAController(FloodDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View(_context.LGAs
                .Where(x => x.LGAName != "None")
                .ToList());
        }

        public IActionResult CreateLGA()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateLGA(LGA lgaModel)
        {
            var lga = _context.LGAs.FirstOrDefault(x => x.LGAName.ToLower() == lgaModel.LGAName.ToLower());
            if (lga == null)
            {
                _context.LGAs.Add(lgaModel);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, LGA lgaModel)
        {
            if (!string.IsNullOrEmpty(lgaModel.LGAName))
            {
                var lga = await _context.LGAs.FindAsync(id);
                lga.LGAName = lgaModel.LGAName;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }
        public async Task<IActionResult> Detail(int id)
        {
            var cities = await _context.Cities.Where(x => x.LGAId == id).ToListAsync();
            foreach(var city in cities)
            {
                var lgaChk = _context.LGAs.Find(id);
                lgaChk.Cities = cities.ToList();
                var lga = await _context.LGAs.Where(x => x.Cities.Contains(city)).FirstOrDefaultAsync();
                
                return View(lga);
            }

            return View();
        }
        public async Task<IActionResult> DeleteLGA()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> DeleteLGA(int id)
        {
            var lga = _context.LGAs.Find(id);
            if(lga == null)
            {
                return RedirectToAction(nameof(Index));
            }
            _context.LGAs.Remove(lga);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
