using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using GlobalDisasterManagement.Data;
using GlobalDisasterManagement.Models;
using GlobalDisasterManagement.Services.Abstract;
using GlobalDisasterManagement.Resources;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using System.Globalization;

namespace GlobalDisasterManagement.Services.Implementation
{
    public class SmsService : ISmsService
    {
        private readonly DisasterDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmsService> _logger;
        private readonly IStringLocalizer<SharedResources> _localizer;
        private readonly string _twilioAccountSid;
        private readonly string _twilioAuthToken;
        private readonly string _twilioPhoneNumber;

        public SmsService(
            DisasterDbContext context,
            IConfiguration configuration,
            ILogger<SmsService> logger,
            IStringLocalizer<SharedResources> localizer)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _localizer = localizer;

            _twilioAccountSid = _configuration["Twilio:AccountSid"] ?? throw new ArgumentNullException("Twilio AccountSid not configured");
            _twilioAuthToken = _configuration["Twilio:AuthToken"] ?? throw new ArgumentNullException("Twilio AuthToken not configured");
            _twilioPhoneNumber = _configuration["Twilio:PhoneNumber"] ?? throw new ArgumentNullException("Twilio PhoneNumber not configured");

            TwilioClient.Init(_twilioAccountSid, _twilioAuthToken);
        }

        public async Task<bool> SendSmsAsync(string phoneNumber, string message, MessagePriority priority = MessagePriority.Normal)
        {
            try
            {
                // Normalize phone number (ensure it starts with +)
                phoneNumber = NormalizePhoneNumber(phoneNumber);

                var messageResource = await MessageResource.CreateAsync(
                    body: message,
                    from: new PhoneNumber(_twilioPhoneNumber),
                    to: new PhoneNumber(phoneNumber)
                );

                // Log to database
                var notification = new SmsNotification
                {
                    PhoneNumber = phoneNumber,
                    Message = message,
                    Priority = priority,
                    Status = NotificationStatus.Sent,
                    TwilioMessageSid = messageResource.Sid,
                    SentAt = DateTime.UtcNow
                };

                _context.SmsNotifications.Add(notification);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"SMS sent successfully to {phoneNumber}. SID: {messageResource.Sid}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send SMS to {phoneNumber}");
                
                // Log failed attempt
                var notification = new SmsNotification
                {
                    PhoneNumber = phoneNumber,
                    Message = message,
                    Priority = priority,
                    Status = NotificationStatus.Failed,
                    ErrorMessage = ex.Message,
                    SentAt = DateTime.UtcNow
                };

                _context.SmsNotifications.Add(notification);
                await _context.SaveChangesAsync();

                return false;
            }
        }

        public async Task<int> SendBulkSmsAsync(List<string> phoneNumbers, string message, MessagePriority priority = MessagePriority.Normal)
        {
            int successCount = 0;

            foreach (var phoneNumber in phoneNumbers)
            {
                var success = await SendSmsAsync(phoneNumber, message, priority);
                if (success) successCount++;

                // Add small delay to avoid rate limiting
                await Task.Delay(100);
            }

            _logger.LogInformation($"Bulk SMS sent: {successCount}/{phoneNumbers.Count} successful");
            return successCount;
        }

        public async Task<bool> SendDisasterAlertAsync(DisasterIncident incident, List<string> phoneNumbers)
        {
            return await SendDisasterAlertAsync(incident, phoneNumbers, "en");
        }

        public async Task<bool> SendDisasterAlertAsync(DisasterIncident incident, List<string> phoneNumbers, string languageCode)
        {
            // Set culture for localization
            var culture = new CultureInfo(languageCode);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;

            var severityText = GetSeverityText((int)incident.Severity);
            var localizedSeverity = _localizer[severityText].Value;
            var localizedDisasterType = _localizer[incident.DisasterType.ToString()].Value;
            
            var message = $"⚠️ {_localizer["DisasterAlert"].Value}\n" +
                         $"{_localizer["DisasterType"].Value}: {localizedDisasterType}\n" +
                         $"{_localizer["Severity"].Value}: {localizedSeverity}\n" +
                         $"{_localizer["Location"].Value}: {incident.City?.Name}, {incident.LGA?.LGAName}\n" +
                         $"{_localizer["Description"].Value}: {incident.Description}\n" +
                         $"{_localizer["Time"].Value}: {incident.ReportedAt:g}\n" +
                         $"{_localizer["StaySafe"].Value}";

            var successCount = await SendBulkSmsAsync(phoneNumbers, message, MessagePriority.Critical);
            return successCount > 0;
        }

        public async Task<bool> SendShelterUpdateAsync(EmergencyShelter shelter, List<string> phoneNumbers)
        {
            var availableSpaces = shelter.AvailableSpaces;
            var message = $"🏠 SHELTER UPDATE\n" +
                         $"Name: {shelter.Name}\n" +
                         $"Location: {shelter.Address}\n" +
                         $"Available Spaces: {availableSpaces}/{shelter.TotalCapacity}\n" +
                         $"Facilities: {GetFacilityList(shelter)}\n" +
                         $"Contact: {shelter.ContactPhone}";

            var successCount = await SendBulkSmsAsync(phoneNumbers, message, MessagePriority.High);
            return successCount > 0;
        }

        public async Task<bool> SendEvacuationOrderAsync(string area, string instructions, List<string> phoneNumbers)
        {
            var message = $"🚨 EVACUATION ORDER\n" +
                         $"Area: {area}\n" +
                         $"Action Required: EVACUATE IMMEDIATELY\n" +
                         $"Instructions: {instructions}\n" +
                         $"This is an official emergency order. Follow designated routes and proceed to nearest shelter.";

            var successCount = await SendBulkSmsAsync(phoneNumbers, message, MessagePriority.Critical);
            return successCount > 0;
        }

        public async Task<bool> SendAllClearAsync(string area, List<string> phoneNumbers)
        {
            var message = $"✅ ALL CLEAR\n" +
                         $"Area: {area}\n" +
                         $"The emergency situation has been resolved. It is now safe to return to the area. " +
                         $"Please remain vigilant and follow any additional guidance from authorities.";

            var successCount = await SendBulkSmsAsync(phoneNumbers, message, MessagePriority.Normal);
            return successCount > 0;
        }

        public async Task<List<SmsNotification>> GetNotificationHistoryAsync(int page = 1, int pageSize = 50)
        {
            return await _context.SmsNotifications
                .Include(n => n.DisasterIncident)
                .Include(n => n.User)
                .OrderByDescending(n => n.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<NotificationStatus> GetNotificationStatusAsync(string messageSid)
        {
            try
            {
                var message = await MessageResource.FetchAsync(messageSid);

                return message.Status.ToString() switch
                {
                    "queued" => NotificationStatus.Queued,
                    "sent" => NotificationStatus.Sent,
                    "delivered" => NotificationStatus.Delivered,
                    "failed" or "undelivered" => NotificationStatus.Failed,
                    _ => NotificationStatus.Pending
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to fetch message status for SID: {messageSid}");
                return NotificationStatus.Failed;
            }
        }

        public async Task<int> RetryFailedNotificationsAsync()
        {
            var failedNotifications = await _context.SmsNotifications
                .Where(n => n.Status == NotificationStatus.Failed && n.RetryCount < 3)
                .ToListAsync();

            int successCount = 0;

            foreach (var notification in failedNotifications)
            {
                var success = await SendSmsAsync(notification.PhoneNumber, notification.Message, notification.Priority);
                
                if (success)
                {
                    successCount++;
                }
                else
                {
                    notification.RetryCount++;
                    await _context.SaveChangesAsync();
                }

                await Task.Delay(200);
            }

            _logger.LogInformation($"Retried {failedNotifications.Count} failed notifications. {successCount} successful.");
            return successCount;
        }

        public async Task<List<string>> GetUserPhoneNumbersByCityAsync(int cityId)
        {
            return await _context.Users
                .Where(u => u.CityId == cityId && !string.IsNullOrEmpty(u.PhoneNumber))
                .Select(u => u.PhoneNumber!)
                .ToListAsync();
        }

        public async Task<List<string>> GetUserPhoneNumbersByLgaAsync(int lgaId)
        {
            return await _context.Users
                .Where(u => u.LGAId == lgaId && !string.IsNullOrEmpty(u.PhoneNumber))
                .Select(u => u.PhoneNumber!)
                .ToListAsync();
        }

        // Helper Methods
        private string NormalizePhoneNumber(string phoneNumber)
        {
            // Remove spaces, hyphens, parentheses
            phoneNumber = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

            // If doesn't start with +, assume it's Nigerian (+234)
            if (!phoneNumber.StartsWith("+"))
            {
                if (phoneNumber.StartsWith("0"))
                {
                    phoneNumber = "+234" + phoneNumber.Substring(1);
                }
                else if (phoneNumber.StartsWith("234"))
                {
                    phoneNumber = "+" + phoneNumber;
                }
                else
                {
                    phoneNumber = "+234" + phoneNumber;
                }
            }

            return phoneNumber;
        }

        private string GetSeverityText(int severity)
        {
            return severity switch
            {
                1 => "LOW",
                2 => "MODERATE",
                3 => "HIGH",
                4 => "CRITICAL",
                5 => "CATASTROPHIC",
                _ => "UNKNOWN"
            };
        }

        private string GetFacilityList(EmergencyShelter shelter)
        {
            var facilities = new List<string>();
            
            if (shelter.HasMedicalFacility) facilities.Add("Medical");
            if (shelter.HasFood) facilities.Add("Food");
            if (shelter.HasWater) facilities.Add("Water");
            if (shelter.HasPower) facilities.Add("Power");

            return facilities.Any() ? string.Join(", ", facilities) : "Basic";
        }
    }
}
