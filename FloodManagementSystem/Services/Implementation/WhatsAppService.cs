using GlobalDisasterManagement.Data;
using GlobalDisasterManagement.Models;
using GlobalDisasterManagement.Services.Abstract;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Globalization;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace GlobalDisasterManagement.Services.Implementation
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly DisasterDbContext _context;
        private readonly ILogger<WhatsAppService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IStringLocalizer<Resources.SharedResources> _localizer;
        private readonly string _twilioAccountSid;
        private readonly string _twilioAuthToken;
        private readonly string _twilioWhatsAppNumber;

        public WhatsAppService(DisasterDbContext context, ILogger<WhatsAppService> logger, IConfiguration configuration, IStringLocalizer<Resources.SharedResources> localizer)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _localizer = localizer;

            _twilioAccountSid = configuration["Twilio:AccountSid"] ?? throw new InvalidOperationException("Twilio AccountSid not configured");
            _twilioAuthToken = configuration["Twilio:AuthToken"] ?? throw new InvalidOperationException("Twilio AuthToken not configured");
            _twilioWhatsAppNumber = configuration["Twilio:WhatsAppNumber"] ?? "whatsapp:+14155238886"; // Twilio Sandbox default

            TwilioClient.Init(_twilioAccountSid, _twilioAuthToken);
        }

        public async Task<bool> SendTextMessageAsync(string toPhoneNumber, string message, Guid? incidentId = null, Guid? alertId = null)
        {
            try
            {
                toPhoneNumber = NormalizePhoneNumber(toPhoneNumber);

                var whatsAppMessage = new WhatsAppMessage
                {
                    Id = Guid.NewGuid(),
                    ToPhoneNumber = toPhoneNumber,
                    FromPhoneNumber = _twilioWhatsAppNumber,
                    MessageBody = message,
                    MessageType = WhatsAppMessageType.Text,
                    Direction = WhatsAppMessageDirection.Outbound,
                    Status = WhatsAppMessageStatus.Pending,
                    DisasterIncidentId = incidentId,
                    DisasterAlertId = alertId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.WhatsAppMessages.Add(whatsAppMessage);
                await _context.SaveChangesAsync();

                // Send via Twilio
                var twilioMessage = await MessageResource.CreateAsync(
                    from: new PhoneNumber(_twilioWhatsAppNumber),
                    to: new PhoneNumber($"whatsapp:{toPhoneNumber}"),
                    body: message
                );

                // Update message with Twilio response
                whatsAppMessage.MessageSid = twilioMessage.Sid;
                whatsAppMessage.Status = MapTwilioStatus(twilioMessage.Status);
                whatsAppMessage.SentAt = DateTime.UtcNow;

                if (twilioMessage.ErrorCode.HasValue)
                {
                    whatsAppMessage.ErrorCode = twilioMessage.ErrorCode.ToString();
                    whatsAppMessage.ErrorMessage = twilioMessage.ErrorMessage;
                    whatsAppMessage.Status = WhatsAppMessageStatus.Failed;
                    whatsAppMessage.FailedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("WhatsApp message sent to {PhoneNumber}, MessageSid: {MessageSid}", 
                    toPhoneNumber, twilioMessage.Sid);

                return twilioMessage.ErrorCode == null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending WhatsApp message to {PhoneNumber}", toPhoneNumber);
                return false;
            }
        }

        public async Task<bool> SendMediaMessageAsync(string toPhoneNumber, string message, string mediaUrl, string mediaType, Guid? incidentId = null)
        {
            try
            {
                toPhoneNumber = NormalizePhoneNumber(toPhoneNumber);

                var whatsAppMessage = new WhatsAppMessage
                {
                    Id = Guid.NewGuid(),
                    ToPhoneNumber = toPhoneNumber,
                    FromPhoneNumber = _twilioWhatsAppNumber,
                    MessageBody = message,
                    MessageType = DetermineMediaType(mediaType),
                    Direction = WhatsAppMessageDirection.Outbound,
                    Status = WhatsAppMessageStatus.Pending,
                    MediaUrl = mediaUrl,
                    MediaContentType = mediaType,
                    DisasterIncidentId = incidentId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.WhatsAppMessages.Add(whatsAppMessage);
                await _context.SaveChangesAsync();

                // Send via Twilio with media
                var mediaUrls = new List<Uri> { new Uri(mediaUrl) };
                var twilioMessage = await MessageResource.CreateAsync(
                    from: new PhoneNumber(_twilioWhatsAppNumber),
                    to: new PhoneNumber($"whatsapp:{toPhoneNumber}"),
                    body: message,
                    mediaUrl: mediaUrls
                );

                // Update message
                whatsAppMessage.MessageSid = twilioMessage.Sid;
                whatsAppMessage.Status = MapTwilioStatus(twilioMessage.Status);
                whatsAppMessage.SentAt = DateTime.UtcNow;

                if (twilioMessage.ErrorCode.HasValue)
                {
                    whatsAppMessage.ErrorCode = twilioMessage.ErrorCode.ToString();
                    whatsAppMessage.ErrorMessage = twilioMessage.ErrorMessage;
                    whatsAppMessage.Status = WhatsAppMessageStatus.Failed;
                    whatsAppMessage.FailedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("WhatsApp media message sent to {PhoneNumber}, MediaUrl: {MediaUrl}", 
                    toPhoneNumber, mediaUrl);

                return twilioMessage.ErrorCode == null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending WhatsApp media message to {PhoneNumber}", toPhoneNumber);
                return false;
            }
        }

        public async Task<bool> SendTemplateMessageAsync(string toPhoneNumber, string templateName, string[] parameters, Guid? incidentId = null)
        {
            try
            {
                toPhoneNumber = NormalizePhoneNumber(toPhoneNumber);

                // For WhatsApp templates, you need pre-approved templates from WhatsApp
                // This is a placeholder - actual implementation depends on your approved templates
                var templateBody = FormatTemplate(templateName, parameters);

                return await SendTextMessageAsync(toPhoneNumber, templateBody, incidentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending WhatsApp template message to {PhoneNumber}", toPhoneNumber);
                return false;
            }
        }

        public async Task<bool> SendDisasterAlertAsync(string toPhoneNumber, DisasterAlert alert)
        {
            try
            {
                var message = $"🚨 *DISASTER ALERT*\n\n" +
                             $"*{alert.Title}*\n\n" +
                             $"{alert.Message}\n\n" +
                             $"Severity: {alert.Severity}\n" +
                             $"Type: {alert.DisasterType}\n\n" +
                             $"Issued: {alert.IssuedAt:MMM dd, yyyy HH:mm}\n\n" +
                             $"Stay safe and follow official instructions.";

                return await SendTextMessageAsync(toPhoneNumber, message, null, alert.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending disaster alert via WhatsApp to {PhoneNumber}", toPhoneNumber);
                return false;
            }
        }

        public async Task<bool> SendDisasterAlertAsync(string toPhoneNumber, DisasterAlert alert, string languageCode)
        {
            try
            {
                var culture = new CultureInfo(languageCode);
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;

                var localizedType = _localizer[alert.DisasterType.ToString()].Value;
                var localizedSeverity = _localizer[alert.Severity.ToString()].Value;
                
                var message = string.Format(_localizer["WhatsAppDisasterAlert"].Value,
                    localizedType,
                    alert.AffectedCities ?? "Unknown",
                    localizedSeverity,
                    alert.Message);

                return await SendTextMessageAsync(toPhoneNumber, message, null, alert.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending localized disaster alert via WhatsApp to {PhoneNumber}", toPhoneNumber);
                return false;
            }
        }

        public async Task<int> SendBulkDisasterAlertAsync(List<string> phoneNumbers, DisasterAlert alert)
        {
            var successCount = 0;
            var tasks = new List<Task<bool>>();

            foreach (var phoneNumber in phoneNumbers)
            {
                // Add small delay to avoid rate limiting
                await Task.Delay(100);
                tasks.Add(SendDisasterAlertAsync(phoneNumber, alert));
            }

            var results = await Task.WhenAll(tasks);
            successCount = results.Count(r => r);

            _logger.LogInformation("Sent WhatsApp disaster alert to {SuccessCount}/{TotalCount} recipients", 
                successCount, phoneNumbers.Count);

            return successCount;
        }

        public async Task<bool> SendIncidentConfirmationAsync(string toPhoneNumber, DisasterIncident incident)
        {
            try
            {
                var message = $"✅ *Incident Report Confirmed*\n\n" +
                             $"Thank you for reporting a {incident.DisasterType}.\n\n" +
                             $"*Location:* {incident.Address}\n" +
                             $"*Severity:* {incident.Severity}\n" +
                             $"*Status:* {incident.Status}\n" +
                             $"*Report ID:* {incident.Id}\n\n" +
                             $"Emergency services have been notified. We will keep you updated.";

                return await SendTextMessageAsync(toPhoneNumber, message, incident.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending incident confirmation via WhatsApp");
                return false;
            }
        }

        public async Task<bool> SendShelterInfoAsync(string toPhoneNumber, EmergencyShelter shelter)
        {
            try
            {
                var availableSpaces = shelter.TotalCapacity - shelter.CurrentOccupancy;
                var message = $"🏠 *Emergency Shelter Information*\n\n" +
                             $"*{shelter.Name}*\n\n" +
                             $"📍 Address: {shelter.Address}\n" +
                             $"👥 Capacity: {availableSpaces}/{shelter.TotalCapacity} available\n" +
                             $"📞 Contact: {shelter.ContactPhone}\n\n" +
                             $"Facilities:\n" +
                             $"{(shelter.HasFood ? "✓ Food\n" : "")}" +
                             $"{(shelter.HasWater ? "✓ Water\n" : "")}" +
                             $"{(shelter.HasMedicalFacility ? "✓ Medical\n" : "")}" +
                             $"{(shelter.HasPower ? "✓ Power\n" : "")}" +
                             $"{(shelter.HasSecurity ? "✓ Security\n" : "")}\n" +
                             $"Location: https://maps.google.com/?q={shelter.Latitude},{shelter.Longitude}";

                return await SendTextMessageAsync(toPhoneNumber, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending shelter info via WhatsApp");
                return false;
            }
        }

        public async Task<bool> SendEvacuationInstructionsAsync(string toPhoneNumber, EvacuationRoute route)
        {
            try
            {
                var message = $"🚶 *Evacuation Instructions*\n\n" +
                             $"*Route:* {route.Name}\n\n" +
                             $"📍 From: {route.StartAddress}\n" +
                             $"📍 To: {route.EndAddress}\n" +
                             $"📏 Distance: {route.DistanceKm} km\n" +
                             $"⏱️ Est. Time: {route.EstimatedTimeMinutes} minutes\n" +
                             $"Status: {route.Status}\n\n" +
                             $"{(route.Notes != null ? $"⚠️ Notes: {route.Notes}\n\n" : "")}" +
                             $"Please evacuate immediately and stay safe!";

                return await SendTextMessageAsync(toPhoneNumber, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending evacuation instructions via WhatsApp");
                return false;
            }
        }

        public async Task ProcessInboundMessageAsync(string from, string to, string body, string messageSid, string mediaUrl = null, string mediaType = null)
        {
            try
            {
                // Remove "whatsapp:" prefix
                from = from.Replace("whatsapp:", "");

                var message = new WhatsAppMessage
                {
                    Id = Guid.NewGuid(),
                    ToPhoneNumber = to.Replace("whatsapp:", ""),
                    FromPhoneNumber = from,
                    MessageBody = body,
                    MessageSid = messageSid,
                    MessageType = string.IsNullOrEmpty(mediaUrl) ? WhatsAppMessageType.Text : DetermineMediaType(mediaType),
                    Direction = WhatsAppMessageDirection.Inbound,
                    Status = WhatsAppMessageStatus.Delivered,
                    MediaUrl = mediaUrl,
                    MediaContentType = mediaType,
                    CreatedAt = DateTime.UtcNow,
                    DeliveredAt = DateTime.UtcNow
                };

                _context.WhatsAppMessages.Add(message);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Processed inbound WhatsApp message from {From}, MessageSid: {MessageSid}", 
                    from, messageSid);

                // Process commands (e.g., "HELP", "ALERT", "SHELTER")
                await ProcessCommandAsync(from, body.Trim().ToUpper());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing inbound WhatsApp message");
            }
        }

        public async Task ProcessMessageStatusAsync(string messageSid, string status, string errorCode = null, string errorMessage = null)
        {
            try
            {
                var message = await _context.WhatsAppMessages
                    .FirstOrDefaultAsync(m => m.MessageSid == messageSid);

                if (message != null)
                {
                    message.Status = status.ToLower() switch
                    {
                        "queued" => WhatsAppMessageStatus.Queued,
                        "sent" => WhatsAppMessageStatus.Sent,
                        "delivered" => WhatsAppMessageStatus.Delivered,
                        "read" => WhatsAppMessageStatus.Read,
                        "failed" => WhatsAppMessageStatus.Failed,
                        "undelivered" => WhatsAppMessageStatus.Undelivered,
                        _ => message.Status
                    };

                    if (status.ToLower() == "delivered")
                        message.DeliveredAt = DateTime.UtcNow;
                    else if (status.ToLower() == "read")
                        message.ReadAt = DateTime.UtcNow;
                    else if (status.ToLower() == "failed")
                    {
                        message.FailedAt = DateTime.UtcNow;
                        message.ErrorCode = errorCode;
                        message.ErrorMessage = errorMessage;
                    }

                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Updated WhatsApp message status: {MessageSid} -> {Status}", 
                        messageSid, status);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing WhatsApp message status update");
            }
        }

        public async Task<Dictionary<string, int>> GetMessageStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.UtcNow.AddDays(-30);
            endDate ??= DateTime.UtcNow;

            var messages = await _context.WhatsAppMessages
                .Where(m => m.CreatedAt >= startDate && m.CreatedAt <= endDate)
                .ToListAsync();

            return new Dictionary<string, int>
            {
                { "Total", messages.Count },
                { "Sent", messages.Count(m => m.Status == WhatsAppMessageStatus.Sent) },
                { "Delivered", messages.Count(m => m.Status == WhatsAppMessageStatus.Delivered) },
                { "Read", messages.Count(m => m.Status == WhatsAppMessageStatus.Read) },
                { "Failed", messages.Count(m => m.Status == WhatsAppMessageStatus.Failed) },
                { "Pending", messages.Count(m => m.Status == WhatsAppMessageStatus.Pending) },
                { "Outbound", messages.Count(m => m.Direction == WhatsAppMessageDirection.Outbound) },
                { "Inbound", messages.Count(m => m.Direction == WhatsAppMessageDirection.Inbound) }
            };
        }

        public async Task<int> RetryFailedMessagesAsync(int maxRetries = 3)
        {
            var failedMessages = await _context.WhatsAppMessages
                .Where(m => m.Status == WhatsAppMessageStatus.Failed && m.RetryCount < maxRetries)
                .Where(m => m.NextRetryAt == null || m.NextRetryAt <= DateTime.UtcNow)
                .ToListAsync();

            var successCount = 0;

            foreach (var message in failedMessages)
            {
                message.RetryCount++;
                message.NextRetryAt = DateTime.UtcNow.AddMinutes(Math.Pow(2, message.RetryCount)); // Exponential backoff

                var success = await SendTextMessageAsync(message.ToPhoneNumber, message.MessageBody, 
                    message.DisasterIncidentId, message.DisasterAlertId);

                if (success)
                    successCount++;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Retried {Count} failed WhatsApp messages, {SuccessCount} succeeded", 
                failedMessages.Count, successCount);

            return successCount;
        }

        public async Task<List<WhatsAppMessage>> GetConversationHistoryAsync(string phoneNumber, int limit = 50)
        {
            phoneNumber = NormalizePhoneNumber(phoneNumber);

            return await _context.WhatsAppMessages
                .Where(m => m.ToPhoneNumber == phoneNumber || m.FromPhoneNumber == phoneNumber)
                .OrderByDescending(m => m.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<bool> SubscribeToAlertsAsync(string phoneNumber, string userId)
        {
            // Implementation would update user preferences in database
            // For now, send confirmation message
            var message = "✅ You have been subscribed to disaster alerts via WhatsApp. " +
                         "You will receive important updates about disasters in your area.\n\n" +
                         "Reply STOP to unsubscribe.";

            return await SendTextMessageAsync(phoneNumber, message);
        }

        public async Task<bool> UnsubscribeFromAlertsAsync(string phoneNumber)
        {
            // Implementation would update user preferences in database
            var message = "You have been unsubscribed from disaster alerts. " +
                         "Reply START to subscribe again.";

            return await SendTextMessageAsync(phoneNumber, message);
        }

        #region Helper Methods

        private string NormalizePhoneNumber(string phoneNumber)
        {
            // Remove all non-digit characters except +
            phoneNumber = new string(phoneNumber.Where(c => char.IsDigit(c) || c == '+').ToArray());

            // Add + if not present
            if (!phoneNumber.StartsWith("+"))
                phoneNumber = "+" + phoneNumber;

            return phoneNumber;
        }

        private WhatsAppMessageType DetermineMediaType(string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
                return WhatsAppMessageType.Text;

            if (contentType.Contains("image"))
                return WhatsAppMessageType.Image;
            if (contentType.Contains("video"))
                return WhatsAppMessageType.Video;
            if (contentType.Contains("audio"))
                return WhatsAppMessageType.Audio;
            if (contentType.Contains("pdf") || contentType.Contains("document"))
                return WhatsAppMessageType.Document;

            return WhatsAppMessageType.Text;
        }

        private WhatsAppMessageStatus MapTwilioStatus(MessageResource.StatusEnum status)
        {
            if (status == MessageResource.StatusEnum.Queued)
                return WhatsAppMessageStatus.Queued;
            else if (status == MessageResource.StatusEnum.Sent)
                return WhatsAppMessageStatus.Sent;
            else if (status == MessageResource.StatusEnum.Delivered)
                return WhatsAppMessageStatus.Delivered;
            else if (status == MessageResource.StatusEnum.Read)
                return WhatsAppMessageStatus.Read;
            else if (status == MessageResource.StatusEnum.Failed)
                return WhatsAppMessageStatus.Failed;
            else if (status == MessageResource.StatusEnum.Undelivered)
                return WhatsAppMessageStatus.Undelivered;
            else
                return WhatsAppMessageStatus.Pending;
        }

        private string FormatTemplate(string templateName, string[] parameters)
        {
            // Placeholder for template formatting
            // Real implementation would fetch approved WhatsApp templates
            return $"Template: {templateName} with parameters: {string.Join(", ", parameters)}";
        }

        private async Task ProcessCommandAsync(string phoneNumber, string command)
        {
            try
            {
                switch (command)
                {
                    case "HELP":
                        await SendTextMessageAsync(phoneNumber, 
                            "🆘 *Available Commands:*\n\n" +
                            "HELP - Show this help message\n" +
                            "ALERT - Get latest disaster alerts\n" +
                            "SHELTER - Find nearest emergency shelter\n" +
                            "REPORT - Report a disaster incident\n" +
                            "STOP - Unsubscribe from alerts\n" +
                            "START - Subscribe to alerts");
                        break;

                    case "ALERT":
                        var recentAlert = await _context.DisasterAlerts
                            .Where(a => a.Status == AlertStatus.Active)
                            .OrderByDescending(a => a.IssuedAt)
                            .FirstOrDefaultAsync();

                        if (recentAlert != null)
                            await SendDisasterAlertAsync(phoneNumber, recentAlert);
                        else
                            await SendTextMessageAsync(phoneNumber, "No active disaster alerts at this time.");
                        break;

                    case "SHELTER":
                        var nearestShelter = await _context.EmergencyShelters
                            .Where(s => s.IsActive && s.IsOperational)
                            .FirstOrDefaultAsync();

                        if (nearestShelter != null)
                            await SendShelterInfoAsync(phoneNumber, nearestShelter);
                        else
                            await SendTextMessageAsync(phoneNumber, "No emergency shelters available at this time.");
                        break;

                    case "STOP":
                        await UnsubscribeFromAlertsAsync(phoneNumber);
                        break;

                    case "START":
                        await SubscribeToAlertsAsync(phoneNumber, null);
                        break;

                    default:
                        // Log unrecognized command but don't respond (to avoid spam)
                        _logger.LogInformation("Unrecognized WhatsApp command: {Command} from {PhoneNumber}", 
                            command, phoneNumber);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing WhatsApp command: {Command}", command);
            }
        }

        #endregion
    }
}
