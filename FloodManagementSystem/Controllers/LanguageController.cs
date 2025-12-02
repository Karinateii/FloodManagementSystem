using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Localization;

namespace GlobalDisasterManagement.Controllers
{
    public class LanguageController : Controller
    {
        /// <summary>
        /// Sets the language for the current user and redirects back to the previous page.
        /// </summary>
        /// <param name="culture">The culture code (e.g., "en", "fr", "ar", "es", "pt", "ha", "yo", "ig")</param>
        /// <param name="returnUrl">The URL to redirect back to after setting the language</param>
        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(culture))
            {
                return BadRequest("Culture cannot be empty");
            }

            // Validate culture against supported cultures
            var supportedCultures = new[] { "en", "fr", "ar", "es", "pt", "ha", "yo", "ig" };
            if (!supportedCultures.Contains(culture.ToLower()))
            {
                return BadRequest($"Culture '{culture}' is not supported");
            }

            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions 
                { 
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax
                }
            );

            // Redirect to return URL or home page
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// GET endpoint for language switching
        /// </summary>
        [HttpGet]
        public IActionResult Change(string culture, string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(culture))
            {
                return RedirectToAction("Index", "Home");
            }

            // Validate culture against supported cultures
            var supportedCultures = new[] { "en", "fr", "ar", "es", "pt", "ha", "yo", "ig" };
            if (!supportedCultures.Contains(culture.ToLower()))
            {
                return RedirectToAction("Index", "Home");
            }

            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions 
                { 
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax
                }
            );

            // Redirect to return URL or home page
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
