using GlobalDisasterManagement.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GlobalDisasterManagement.Controllers
{
    public class TipsController : Controller
    {
        private readonly DisasterDbContext _context;

        public TipsController(DisasterDbContext context)
        {
            _context = context;
        }

        public IActionResult WhatToDo()
        {
            return View();
        }

        public IActionResult ReportDisaster()
        {
            // Redirect to the actual incident reporting form
            return RedirectToAction("Report", "Incident");
        }

        public IActionResult Planning()
        {
            return View();
        }

        public IActionResult DisasterRisks()
        {
            return View();
        }

    }
}
