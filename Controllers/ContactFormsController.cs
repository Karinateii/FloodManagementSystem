using Microsoft.AspNetCore.Mvc;

namespace NewLagosFloodDetectionSystem.Controllers
{
    public class ContactFormsController : Controller
    {
        public IActionResult AboutUs()
        {
            return View();
        }
        public IActionResult ContactUs()
        {
            return View();
        }

    }
}
