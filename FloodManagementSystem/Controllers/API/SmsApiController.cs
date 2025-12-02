using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GlobalDisasterManagement.Data;
using GlobalDisasterManagement.Models;
using GlobalDisasterManagement.Services.Abstract;
using Microsoft.EntityFrameworkCore;

namespace GlobalDisasterManagement.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class SmsApiController : ControllerBase
    {
        private readonly ISmsService _smsService;
        private readonly DisasterDbContext _context;
        private readonly ILogger<SmsApiController> _logger;

        public SmsApiController(
            ISmsService smsService,
            DisasterDbContext context,
            ILogger<SmsApiController> logger)
        {
            _smsService = smsService;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Send SMS to a single recipient
        /// </summary>
        /// <param name="request">SMS send request</param>
        /// <returns>Success status</returns>
        [HttpPost("send")]
        public async Task<IActionResult> SendSms([FromBody] SendSmsRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PhoneNumber) || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { success = false, message = "Phone number and message are required." });
            }

            try
            {
                var success = await _smsService.SendSmsAsync(request.PhoneNumber, request.Message, request.Priority);
                
                if (success)
                {
                    return Ok(new { success = true, message = "SMS sent successfully!" });
                }
                else
                {
                    return StatusCode(500, new { success = false, message = "Failed to send SMS." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS to {PhoneNumber}", request.PhoneNumber);
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Send SMS to multiple recipients
        /// </summary>
        /// <param name="request">Bulk SMS send request</param>
        /// <returns>Number of successful sends</returns>
        [HttpPost("bulk")]
        public async Task<IActionResult> SendBulkSms([FromBody] SendBulkSmsRequest request)
        {
            if (request.PhoneNumbers == null || !request.PhoneNumbers.Any() || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { success = false, message = "Phone numbers and message are required." });
            }

            try
            {
                var successCount = await _smsService.SendBulkSmsAsync(request.PhoneNumbers, request.Message, request.Priority);
                
                return Ok(new 
                { 
                    success = true, 
                    message = $"Sent {successCount} of {request.PhoneNumbers.Count} messages.",
                    successCount,
                    totalCount = request.PhoneNumbers.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bulk SMS");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Send disaster alert for a specific incident
        /// </summary>
        /// <param name="request">Disaster alert request</param>
        /// <returns>Number of successful sends</returns>
        [HttpPost("disaster-alert")]
        public async Task<IActionResult> SendDisasterAlert([FromBody] SendDisasterAlertRequest request)
        {
            if (request.IncidentId <= 0)
            {
                return BadRequest(new { success = false, message = "Incident ID is required." });
            }

            try
            {
                var incident = await _context.DisasterIncidents
                    .Include(i => i.City)
                    .Include(i => i.LGA)
                    .FirstOrDefaultAsync(i => i.Id == Guid.Parse(request.IncidentId.ToString()));

                if (incident == null)
                {
                    return NotFound(new { success = false, message = "Incident not found." });
                }

                List<string> phoneNumbers;

                if (request.CityId.HasValue)
                {
                    phoneNumbers = await _smsService.GetUserPhoneNumbersByCityAsync(request.CityId.Value);
                }
                else if (request.LgaId.HasValue)
                {
                    phoneNumbers = await _smsService.GetUserPhoneNumbersByLgaAsync(request.LgaId.Value);
                }
                else if (request.PhoneNumbers != null && request.PhoneNumbers.Any())
                {
                    phoneNumbers = request.PhoneNumbers;
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
                        return Ok(new { success = false, message = "Incident has no associated city and no recipients specified.", successCount = 0 });
                    }
                }

                if (!phoneNumbers.Any())
                {
                    return Ok(new { success = false, message = "No recipients found.", successCount = 0 });
                }

                var successCount = await _smsService.SendDisasterAlertAsync(incident, phoneNumbers);
                
                return Ok(new 
                { 
                    success = true, 
                    message = $"Sent disaster alert to {successCount} of {phoneNumbers.Count} recipients.",
                    successCount,
                    totalCount = phoneNumbers.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending disaster alert for incident {IncidentId}", request.IncidentId);
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Send shelter update notification
        /// </summary>
        /// <param name="request">Shelter update request</param>
        /// <returns>Number of successful sends</returns>
        [HttpPost("shelter-update")]
        public async Task<IActionResult> SendShelterUpdate([FromBody] SendShelterUpdateRequest request)
        {
            if (request.ShelterId <= 0)
            {
                return BadRequest(new { success = false, message = "Shelter ID is required." });
            }

            try
            {
                var shelter = await _context.EmergencyShelters
                    .Include(s => s.City)
                    .Include(s => s.LGA)
                    .FirstOrDefaultAsync(s => s.Id == Guid.Parse(request.ShelterId.ToString()));

                if (shelter == null)
                {
                    return NotFound(new { success = false, message = "Shelter not found." });
                }

                List<string> phoneNumbers;

                if (request.CityId.HasValue)
                {
                    phoneNumbers = await _smsService.GetUserPhoneNumbersByCityAsync(request.CityId.Value);
                }
                else if (request.LgaId.HasValue)
                {
                    phoneNumbers = await _smsService.GetUserPhoneNumbersByLgaAsync(request.LgaId.Value);
                }
                else if (request.PhoneNumbers != null && request.PhoneNumbers.Any())
                {
                    phoneNumbers = request.PhoneNumbers;
                }
                else
                {
                    // Send to shelter's city by default (if has a city)
                    if (shelter.CityId.HasValue)
                    {
                        phoneNumbers = await _smsService.GetUserPhoneNumbersByCityAsync(shelter.CityId.Value);
                    }
                    else
                    {
                        return Ok(new { success = false, message = "Shelter has no associated city and no recipients specified.", successCount = 0 });
                    }
                }

                if (!phoneNumbers.Any())
                {
                    return Ok(new { success = false, message = "No recipients found.", successCount = 0 });
                }

                var successCount = await _smsService.SendShelterUpdateAsync(shelter, phoneNumbers);
                
                return Ok(new 
                { 
                    success = true, 
                    message = $"Sent shelter update to {successCount} of {phoneNumbers.Count} recipients.",
                    successCount,
                    totalCount = phoneNumbers.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending shelter update for shelter {ShelterId}", request.ShelterId);
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Send evacuation order
        /// </summary>
        /// <param name="request">Evacuation order request</param>
        /// <returns>Number of successful sends</returns>
        [HttpPost("evacuation")]
        public async Task<IActionResult> SendEvacuationOrder([FromBody] SendEvacuationOrderRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Area) || string.IsNullOrWhiteSpace(request.Instructions))
            {
                return BadRequest(new { success = false, message = "Area and instructions are required." });
            }

            try
            {
                List<string> phoneNumbers;

                if (request.CityId.HasValue)
                {
                    phoneNumbers = await _smsService.GetUserPhoneNumbersByCityAsync(request.CityId.Value);
                }
                else if (request.LgaId.HasValue)
                {
                    phoneNumbers = await _smsService.GetUserPhoneNumbersByLgaAsync(request.LgaId.Value);
                }
                else if (request.PhoneNumbers != null && request.PhoneNumbers.Any())
                {
                    phoneNumbers = request.PhoneNumbers;
                }
                else
                {
                    return BadRequest(new { success = false, message = "Must specify city, LGA, or phone numbers." });
                }

                if (!phoneNumbers.Any())
                {
                    return Ok(new { success = false, message = "No recipients found.", successCount = 0 });
                }

                var successCount = await _smsService.SendEvacuationOrderAsync(request.Area, request.Instructions, phoneNumbers);
                
                return Ok(new 
                { 
                    success = true, 
                    message = $"Sent evacuation order to {successCount} of {phoneNumbers.Count} recipients.",
                    successCount,
                    totalCount = phoneNumbers.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending evacuation order for area {Area}", request.Area);
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Send all-clear message
        /// </summary>
        /// <param name="request">All-clear request</param>
        /// <returns>Number of successful sends</returns>
        [HttpPost("all-clear")]
        public async Task<IActionResult> SendAllClear([FromBody] SendAllClearRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Area) || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { success = false, message = "Area and message are required." });
            }

            try
            {
                List<string> phoneNumbers;

                if (request.CityId.HasValue)
                {
                    phoneNumbers = await _smsService.GetUserPhoneNumbersByCityAsync(request.CityId.Value);
                }
                else if (request.LgaId.HasValue)
                {
                    phoneNumbers = await _smsService.GetUserPhoneNumbersByLgaAsync(request.LgaId.Value);
                }
                else if (request.PhoneNumbers != null && request.PhoneNumbers.Any())
                {
                    phoneNumbers = request.PhoneNumbers;
                }
                else
                {
                    return BadRequest(new { success = false, message = "Must specify city, LGA, or phone numbers." });
                }

                if (!phoneNumbers.Any())
                {
                    return Ok(new { success = false, message = "No recipients found.", successCount = 0 });
                }

                // Format the all-clear message with area and message
                var formattedMessage = $"✅ ALL CLEAR\n\nArea: {request.Area}\n{request.Message}";
                var successCount = await _smsService.SendBulkSmsAsync(phoneNumbers, formattedMessage, MessagePriority.Normal);
                
                return Ok(new 
                { 
                    success = true, 
                    message = $"Sent all-clear to {successCount} of {phoneNumbers.Count} recipients.",
                    successCount,
                    totalCount = phoneNumbers.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending all-clear for area {Area}", request.Area);
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get notification history with pagination
        /// </summary>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Items per page</param>
        /// <param name="status">Filter by status</param>
        /// <returns>Paginated notification history</returns>
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory(int page = 1, int pageSize = 20, NotificationStatus? status = null)
        {
            try
            {
                var query = _context.SmsNotifications
                    .Include(n => n.DisasterIncident)
                    .Include(n => n.DisasterAlert)
                    .Include(n => n.User)
                    .OrderByDescending(n => n.CreatedAt)
                    .AsQueryable();

                if (status.HasValue)
                {
                    query = query.Where(n => n.Status == status.Value);
                }

                var totalCount = await query.CountAsync();
                var notifications = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = notifications,
                    page,
                    pageSize,
                    totalCount,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching notification history");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get notification status
        /// </summary>
        /// <param name="id">Notification ID</param>
        /// <returns>Notification details with current status</returns>
        [HttpGet("status/{id}")]
        public async Task<IActionResult> GetStatus(int id)
        {
            try
            {
                var notification = await _context.SmsNotifications
                    .Include(n => n.DisasterIncident)
                    .Include(n => n.DisasterAlert)
                    .Include(n => n.User)
                    .FirstOrDefaultAsync(n => n.Id == id);

                if (notification == null)
                {
                    return NotFound(new { success = false, message = "Notification not found." });
                }

                // Refresh status from Twilio if sent
                if (!string.IsNullOrEmpty(notification.TwilioMessageSid))
                {
                    var updatedStatus = await _smsService.GetNotificationStatusAsync(notification.TwilioMessageSid);
                    notification.Status = updatedStatus;
                    await _context.SaveChangesAsync();
                }

                return Ok(new
                {
                    success = true,
                    data = notification
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching notification status for {NotificationId}", id);
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Retry failed notifications
        /// </summary>
        /// <returns>Number of notifications retried</returns>
        [HttpPost("retry")]
        public async Task<IActionResult> RetryFailed()
        {
            try
            {
                var retried = await _smsService.RetryFailedNotificationsAsync();
                
                return Ok(new
                {
                    success = true,
                    message = $"Retried {retried} failed notifications.",
                    retriedCount = retried
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrying failed notifications");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get SMS statistics
        /// </summary>
        /// <returns>SMS sending statistics</returns>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var totalSent = await _context.SmsNotifications.CountAsync(n => n.Status == NotificationStatus.Sent || n.Status == NotificationStatus.Delivered);
                var totalFailed = await _context.SmsNotifications.CountAsync(n => n.Status == NotificationStatus.Failed);
                var totalPending = await _context.SmsNotifications.CountAsync(n => n.Status == NotificationStatus.Pending || n.Status == NotificationStatus.Queued);
                var totalDelivered = await _context.SmsNotifications.CountAsync(n => n.Status == NotificationStatus.Delivered);

                var deliveryRate = totalSent > 0 ? (double)totalDelivered / totalSent * 100 : 0;

                return Ok(new
                {
                    success = true,
                    statistics = new
                    {
                        totalSent,
                        totalFailed,
                        totalPending,
                        totalDelivered,
                        deliveryRate = Math.Round(deliveryRate, 2)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching SMS statistics");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get SMS templates
        /// </summary>
        /// <param name="type">Filter by notification type</param>
        /// <param name="languageCode">Filter by language</param>
        /// <returns>List of SMS templates</returns>
        [HttpGet("templates")]
        public async Task<IActionResult> GetTemplates(NotificationType? type = null, string languageCode = "en")
        {
            try
            {
                var query = _context.SmsTemplates
                    .Where(t => t.IsActive && t.LanguageCode == languageCode)
                    .AsQueryable();

                if (type.HasValue)
                {
                    query = query.Where(t => t.Type == type.Value);
                }

                var templates = await query.ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = templates
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching SMS templates");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get specific template by ID
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <returns>Template details</returns>
        [HttpGet("templates/{id}")]
        public async Task<IActionResult> GetTemplate(int id)
        {
            try
            {
                var template = await _context.SmsTemplates.FindAsync(id);

                if (template == null)
                {
                    return NotFound(new { success = false, message = "Template not found." });
                }

                return Ok(new
                {
                    success = true,
                    data = template
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching template {TemplateId}", id);
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }

    // Request models
    public class SendSmsRequest
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public MessagePriority Priority { get; set; } = MessagePriority.Normal;
    }

    public class SendBulkSmsRequest
    {
        public List<string> PhoneNumbers { get; set; } = new();
        public string Message { get; set; } = string.Empty;
        public MessagePriority Priority { get; set; } = MessagePriority.Normal;
    }

    public class SendDisasterAlertRequest
    {
        public int IncidentId { get; set; }
        public int? CityId { get; set; }
        public int? LgaId { get; set; }
        public List<string>? PhoneNumbers { get; set; }
    }

    public class SendShelterUpdateRequest
    {
        public int ShelterId { get; set; }
        public int? CityId { get; set; }
        public int? LgaId { get; set; }
        public List<string>? PhoneNumbers { get; set; }
    }

    public class SendEvacuationOrderRequest
    {
        public string Area { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
        public int? CityId { get; set; }
        public int? LgaId { get; set; }
        public List<string>? PhoneNumbers { get; set; }
    }

    public class SendAllClearRequest
    {
        public string Area { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int? CityId { get; set; }
        public int? LgaId { get; set; }
        public List<string>? PhoneNumbers { get; set; }
    }
}
