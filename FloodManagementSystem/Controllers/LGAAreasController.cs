using Microsoft.AspNetCore.Mvc;

namespace GlobalDisasterManagement.Controllers
{
    public class LGAAreasController : Controller
    {
        public IActionResult Mainland()
        {
            return View();
        }
        public IActionResult Ikeja()
        {
            return View();
        }
        public IActionResult Ikorodu()
        {
            return View();
        }
        public IActionResult Surulere()
        {
            return View();
        }
        public IActionResult Island()
        {
            return View();
        }
    }
}
