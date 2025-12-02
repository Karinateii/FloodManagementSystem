using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GlobalDisasterManagement.Models;
using GlobalDisasterManagement.Services.Interfaces;
using System.Security.Claims;

namespace GlobalDisasterManagement.Controllers
{
    [Authorize]
    public class IncidentController : Controller
    {
        private readonly IIncidentService _incidentService;
        private readonly ILogger<IncidentController> _logger;
        private readonly IWebHostEnvironment _environment;

        public IncidentController(
            IIncidentService incidentService,
            ILogger<IncidentController> logger,
            IWebHostEnvironment environment)
        {
            _incidentService = incidentService;
            _logger = logger;
            _environment = environment;
        }

        // GET: Incident/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                var incidents = await _incidentService.GetActiveIncidentsAsync();
                return View(incidents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading incidents");
                TempData["Error"] = "An error occurred while loading incidents.";
                return View(new List<DisasterIncident>());
            }
        }

        // Helper method to populate ViewBag for the report form
        private void PopulateReportFormViewBag()
        {
            ViewBag.DisasterTypes = Enum.GetValues(typeof(DisasterType))
                .Cast<DisasterType>()
                .Select(dt => new { Value = (int)dt, Text = dt.ToString().Replace("_", " ") })
                .ToList();

            ViewBag.Severities = Enum.GetValues(typeof(IncidentSeverity))
                .Cast<IncidentSeverity>()
                .Select(s => new { Value = (int)s, Text = s.ToString() })
                .ToList();
        }

        // GET: Incident/Report
        public IActionResult Report()
        {
            PopulateReportFormViewBag();
            return View();
        }

        // POST: Incident/Report
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Report(DisasterIncident incident, IFormFile[]? photos)
        {
            try
            {
                // Get current user ID and set it before validation
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "You must be logged in to report an incident.";
                    return RedirectToAction("Login", "UserAuthentication");
                }

                incident.ReporterId = userId;

                // Remove ReporterId from ModelState since it's set programmatically
                ModelState.Remove("ReporterId");

                if (!ModelState.IsValid)
                {
                    PopulateReportFormViewBag();
                    return View(incident);
                }

                // Handle photo uploads
                if (photos != null && photos.Length > 0)
                {
                    var photoUrls = new List<string>();
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "Uploads", "Incidents");
                    Directory.CreateDirectory(uploadsFolder);

                    foreach (var photo in photos)
                    {
                        if (photo.Length > 0)
                        {
                            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(photo.FileName)}";
                            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await photo.CopyToAsync(fileStream);
                            }

                            photoUrls.Add($"/Uploads/Incidents/{uniqueFileName}");
                        }
                    }

                    incident.PhotoUrls = System.Text.Json.JsonSerializer.Serialize(photoUrls);
                }

                await _incidentService.CreateIncidentAsync(incident);

                TempData["Success"] = "Incident reported successfully. Emergency services have been notified.";
                return RedirectToAction(nameof(Details), new { id = incident.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reporting incident");
                TempData["Error"] = "An error occurred while reporting the incident.";
                PopulateReportFormViewBag();
                return View(incident);
            }
        }

        // GET: Incident/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var incident = await _incidentService.GetIncidentByIdAsync(id);
                if (incident == null)
                {
                    TempData["Error"] = "Incident not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(incident);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading incident details");
                TempData["Error"] = "An error occurred while loading incident details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Incident/Map
        public async Task<IActionResult> Map()
        {
            try
            {
                var incidents = await _incidentService.GetActiveIncidentsAsync();
                return View(incidents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading incident map");
                TempData["Error"] = "An error occurred while loading the map.";
                return View(new List<DisasterIncident>());
            }
        }

        // GET: Incident/MyReports
        public async Task<IActionResult> MyReports()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "UserAuthentication");
                }

                var allIncidents = await _incidentService.GetActiveIncidentsAsync();
                var myIncidents = allIncidents.Where(i => i.ReporterId == userId).ToList();

                return View(myIncidents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user reports");
                TempData["Error"] = "An error occurred while loading your reports.";
                return View(new List<DisasterIncident>());
            }
        }

        // POST: Incident/UpdateStatus - Admin only
        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(Guid id, IncidentStatus status, string? notes)
        {
            try
            {
                var result = await _incidentService.UpdateIncidentStatusAsync(id, status, notes);
                if (result)
                {
                    TempData["Success"] = "Incident status updated successfully.";
                }
                else
                {
                    TempData["Error"] = "Failed to update incident status.";
                }

                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating incident status");
                TempData["Error"] = "An error occurred while updating status.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // POST: Incident/Verify - Admin only
        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> Verify(Guid id)
        {
            try
            {
                var userName = User.Identity?.Name ?? "Unknown";
                var result = await _incidentService.VerifyIncidentAsync(id, userName);

                if (result)
                {
                    TempData["Success"] = "Incident verified successfully.";
                }
                else
                {
                    TempData["Error"] = "Failed to verify incident.";
                }

                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying incident");
                TempData["Error"] = "An error occurred while verifying the incident.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }
    }
}
