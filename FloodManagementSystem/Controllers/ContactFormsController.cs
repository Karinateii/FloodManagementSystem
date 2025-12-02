using Microsoft.AspNetCore.Mvc;

namespace GlobalDisasterManagement.Controllers
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
