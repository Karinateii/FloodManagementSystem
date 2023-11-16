using Microsoft.AspNetCore.Mvc;

namespace NewLagosFloodDetectionSystem.Controllers
{
    public class TipsController : Controller
    {
        public IActionResult WhatToDo()
        {
            return View();
        }
        public IActionResult ReportFlood()
        {
            return View();
        }
        public IActionResult Planning()
        {
            return View();
        }
        public IActionResult FloodRisks()
        {
            return View();
        }

    }
}
