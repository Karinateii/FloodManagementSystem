using NewLagosFloodDetectionSystem.Data;
using NewLagosFloodDetectionSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace NewLagosFloodDetectionSystem.Controllers
{
    public class UserController : Controller
    {
        public readonly FloodDbContext _context;
        public UserController(FloodDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var userList = _context.Users.ToList();
            return View(userList);
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
