using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GlobalDisasterManagement.Data;
using GlobalDisasterManagement.Models;
using GlobalDisasterManagement.Services.Abstract;
using Microsoft.EntityFrameworkCore;

namespace GlobalDisasterManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SmsNotificationController : Controller
    {
        private readonly ISmsService _smsService;
        private readonly DisasterDbContext _context;
        private readonly ILogger<SmsNotificationController> _logger;

        public SmsNotificationController(
            ISmsService smsService,
            DisasterDbContext context,
            ILogger<SmsNotificationController> logger)
        {
            _smsService = smsService;
            _context = context;
            _logger = logger;
        }

        // GET: SmsNotification
        public async Task<IActionResult> Index(int page = 1, int pageSize = 20)
        {
            var notifications = await _smsService.GetNotificationHistoryAsync(page, pageSize);
            
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = await _context.SmsNotifications.CountAsync();
            
            return View(notifications);
        }

        // GET: SmsNotification/Send
        public async Task<IActionResult> Send()
        {
            ViewBag.Cities = await _context.Cities.ToListAsync();
            ViewBag.LGAs = await _context.LGAs.ToListAsync();
            ViewBag.DisasterIncidents = await _context.DisasterIncidents
                .Where(i => i.Status != IncidentStatus.Resolved)
                .OrderByDescending(i => i.ReportedAt)
                .Take(10)
                .ToListAsync();
            
            return View();
        }

        // POST: SmsNotification/Send
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(string phoneNumber, string message, MessagePriority priority = MessagePriority.Normal)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] = "Phone number and message are required.";
                return RedirectToAction(nameof(Send));
            }

            try
            {
                var success = await _smsService.SendSmsAsync(phoneNumber, message, priority);
                
                if (success)
                {
                    TempData["Success"] = "SMS sent successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to send SMS. Please check logs.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS to {PhoneNumber}", phoneNumber);
                TempData["Error"] = $"Error: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: SmsNotification/SendBulk
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendBulk(List<string> phoneNumbers, string message, MessagePriority priority = MessagePriority.Normal)
        {
            if (phoneNumbers == null || !phoneNumbers.Any() || string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] = "Phone numbers and message are required.";
                return RedirectToAction(nameof(Send));
            }

            try
            {
                var successCount = await _smsService.SendBulkSmsAsync(phoneNumbers, message, priority);
                TempData["Success"] = $"Sent {successCount} of {phoneNumbers.Count} SMS messages successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bulk SMS");
                TempData["Error"] = $"Error: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: SmsNotification/SendToCity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendToCity(int cityId, string message, MessagePriority priority = MessagePriority.Normal)
        {
            if (cityId <= 0 || string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] = "City and message are required.";
                return RedirectToAction(nameof(Send));
            }

            try
            {
                var phoneNumbers = await _smsService.GetUserPhoneNumbersByCityAsync(cityId);
                
                if (!phoneNumbers.Any())
                {
                    TempData["Warning"] = "No users found in the selected city.";
                    return RedirectToAction(nameof(Send));
                }

                var successCount = await _smsService.SendBulkSmsAsync(phoneNumbers, message, priority);
                TempData["Success"] = $"Sent {successCount} of {phoneNumbers.Count} SMS messages to city residents!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS to city {CityId}", cityId);
                TempData["Error"] = $"Error: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: SmsNotification/SendToLGA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendToLGA(int lgaId, string message, MessagePriority priority = MessagePriority.Normal)
        {
            if (lgaId <= 0 || string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] = "LGA and message are required.";
                return RedirectToAction(nameof(Send));
            }

            try
            {
                var phoneNumbers = await _smsService.GetUserPhoneNumbersByLgaAsync(lgaId);
                
                if (!phoneNumbers.Any())
                {
                    TempData["Warning"] = "No users found in the selected LGA.";
                    return RedirectToAction(nameof(Send));
                }

                var successCount = await _smsService.SendBulkSmsAsync(phoneNumbers, message, priority);
                TempData["Success"] = $"Sent {successCount} of {phoneNumbers.Count} SMS messages to LGA residents!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS to LGA {LgaId}", lgaId);
                TempData["Error"] = $"Error: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: SmsNotification/SendDisasterAlert
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendDisasterAlert(int incidentId, int? cityId = null, int? lgaId = null)
        {
            if (incidentId <= 0)
            {
                TempData["Error"] = "Incident is required.";
                return RedirectToAction(nameof(Send));
            }

            try
            {
                var incident = await _context.DisasterIncidents
                    .Include(i => i.City)
                    .Include(i => i.LGA)
                    .FirstOrDefaultAsync(i => i.Id == Guid.Parse(incidentId.ToString()));

                if (incident == null)
                {
                    TempData["Error"] = "Incident not found.";
                    return RedirectToAction(nameof(Send));
                }

                List<string> phoneNumbers;
                
                if (cityId.HasValue)
                {
                    phoneNumbers = await _smsService.GetUserPhoneNumbersByCityAsync(cityId.Value);
                }
                else if (lgaId.HasValue)
                {
                    phoneNumbers = await _smsService.GetUserPhoneNumbersByLgaAsync(lgaId.Value);
                }
                else
                {
                    // Send to incident's city by default (if has a city)
                    if (incident.CityId.HasValue)
                    {
                        phoneNumbers = await _smsService.GetUserPhoneNumbersByCityAsync(incident.CityId.Value);
                    }
                    else
                    {
                        TempData["Warning"] = "Incident has no associated city.";
                        return RedirectToAction(nameof(Send));
                    }
                }

                if (!phoneNumbers.Any())
                {
                    TempData["Warning"] = "No users found for the selected area.";
                    return RedirectToAction(nameof(Send));
                }

                var successCount = await _smsService.SendDisasterAlertAsync(incident, phoneNumbers);
                TempData["Success"] = $"Sent disaster alert to {successCount} of {phoneNumbers.Count} residents!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending disaster alert for incident {IncidentId}", incidentId);
                TempData["Error"] = $"Error: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: SmsNotification/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var notification = await _context.SmsNotifications
                .Include(n => n.DisasterIncident)
                .Include(n => n.DisasterAlert)
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (notification == null)
            {
                return NotFound();
            }

            return View(notification);
        }

        // POST: SmsNotification/Retry/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Retry(int id)
        {
            try
            {
                var notification = await _context.SmsNotifications.FindAsync(id);
                
                if (notification == null)
                {
                    TempData["Error"] = "Notification not found.";
                    return RedirectToAction(nameof(Index));
                }

                if (notification.Status != NotificationStatus.Failed)
                {
                    TempData["Warning"] = "Only failed notifications can be retried.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var success = await _smsService.SendSmsAsync(
                    notification.PhoneNumber,
                    notification.Message,
                    notification.Priority
                );

                if (success)
                {
                    TempData["Success"] = "SMS resent successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to resend SMS.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrying SMS {NotificationId}", id);
                TempData["Error"] = $"Error: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: SmsNotification/RetryAll
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RetryAll()
        {
            try
            {
                var retried = await _smsService.RetryFailedNotificationsAsync();
                TempData["Success"] = $"Retried {retried} failed notifications!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrying all failed notifications");
                TempData["Error"] = $"Error: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: SmsNotification/Statistics
        public async Task<IActionResult> Statistics()
        {
            var totalSent = await _context.SmsNotifications.CountAsync(n => n.Status == NotificationStatus.Sent || n.Status == NotificationStatus.Delivered);
            var totalFailed = await _context.SmsNotifications.CountAsync(n => n.Status == NotificationStatus.Failed);
            var totalPending = await _context.SmsNotifications.CountAsync(n => n.Status == NotificationStatus.Pending || n.Status == NotificationStatus.Queued);
            var totalDelivered = await _context.SmsNotifications.CountAsync(n => n.Status == NotificationStatus.Delivered);

            var recentNotifications = await _context.SmsNotifications
                .OrderByDescending(n => n.CreatedAt)
                .Take(10)
                .ToListAsync();

            ViewBag.TotalSent = totalSent;
            ViewBag.TotalFailed = totalFailed;
            ViewBag.TotalPending = totalPending;
            ViewBag.TotalDelivered = totalDelivered;
            ViewBag.DeliveryRate = totalSent > 0 ? (double)totalDelivered / totalSent * 100 : 0;
            ViewBag.RecentNotifications = recentNotifications;

            return View();
        }
    }
}
