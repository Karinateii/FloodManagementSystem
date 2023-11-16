using NewLagosFloodDetectionSystem.Data;
using NewLagosFloodDetectionSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Security.Claims;

namespace NewLagosFloodDetectionSystem.Controllers
{
    public class HomeController : Controller
    {
        public readonly FloodDbContext _context;
        public HomeController(FloodDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}