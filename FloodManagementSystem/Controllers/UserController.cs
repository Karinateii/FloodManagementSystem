using GlobalDisasterManagement.Data;
using GlobalDisasterManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GlobalDisasterManagement.Controllers
{
    public class UserController : Controller
    {
        public readonly DisasterDbContext _context;
        public UserController(DisasterDbContext context)
        {
            _context = context;
        }
        public IActionResult Index(string searchTerm, string lgaFilter, string cityFilter)
        {
            var users = _context.Users.AsQueryable();
            
            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                users = users.Where(u => 
                    u.UserName.Contains(searchTerm) || 
                    u.Email.Contains(searchTerm) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(searchTerm))
                );
            }
            
            // Apply LGA filter
            if (!string.IsNullOrWhiteSpace(lgaFilter) && lgaFilter != "All")
            {
                users = users.Where(u => u.LGAName == lgaFilter);
            }
            
            // Apply City filter
            if (!string.IsNullOrWhiteSpace(cityFilter) && cityFilter != "All")
            {
                users = users.Where(u => u.CityName == cityFilter);
            }
            
            // Get distinct values for filter dropdowns
            ViewBag.LGAs = _context.Users
                .Where(u => !string.IsNullOrEmpty(u.LGAName))
                .Select(u => u.LGAName)
                .Distinct()
                .OrderBy(l => l)
                .ToList();
                
            ViewBag.Cities = _context.Users
                .Where(u => !string.IsNullOrEmpty(u.CityName))
                .Select(u => u.CityName)
                .Distinct()
                .OrderBy(c => c)
                .ToList();
            
            ViewBag.SearchTerm = searchTerm;
            ViewBag.LGAFilter = lgaFilter;
            ViewBag.CityFilter = cityFilter;
            
            var userList = users.OrderBy(u => u.UserName).ToList();
            return View(userList);
        }
        
        public async Task<IActionResult> Details(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        public async Task<IActionResult> Delete()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Delete(String id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Cancel()
        {
            return RedirectToAction(nameof(Index));
        }

    }
}
