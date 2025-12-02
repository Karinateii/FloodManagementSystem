using Microsoft.Extensions.Options;
using Microsoft.Extensions.Localization;
using GlobalDisasterManagement.Models;
using GlobalDisasterManagement.Models.Configuration;
using GlobalDisasterManagement.Repositories.Interfaces;
using GlobalDisasterManagement.Services.Interfaces;
using GlobalDisasterManagement.Resources;
using System.Net.Mail;
using System.Globalization;

namespace GlobalDisasterManagement.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStringLocalizer<SharedResources> _localizer;

        public EmailService(
            IOptions<EmailSettings> emailSettings,
            ILogger<EmailService> logger,
            IUnitOfWork unitOfWork,
            IStringLocalizer<SharedResources> localizer)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _localizer = localizer;
        }

        public async Task<bool> SendFloodAlertAsync(CityFloodPrediction prediction, List<string> recipients)
        {
            return await SendFloodAlertAsync(prediction, recipients, "en");
        }

        public async Task<bool> SendFloodAlertAsync(CityFloodPrediction prediction, List<string> recipients, string languageCode)
        {
            if (string.IsNullOrEmpty(_emailSettings.SenderEmail) || 
                string.IsNullOrEmpty(_emailSettings.SenderPassword))
            {
                _logger.LogWarning("Email settings not configured");
                return false;
            }

            if (recipients == null || !recipients.Any())
            {
                _logger.LogWarning("No recipients provided for flood alert");
                return false;
            }

            // Set culture for localization
            var culture = new CultureInfo(languageCode);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;

            try
            {
                using var mail = new MailMessage();
                using var smtpServer = new SmtpClient(_emailSettings.SmtpServer);

                mail.From = new MailAddress(_emailSettings.SenderEmail);
                foreach (var recipient in recipients.Where(r => !string.IsNullOrEmpty(r)))
                {
                    mail.To.Add(recipient);
                }

                if (mail.To.Count == 0)
                {
                    return false;
                }

                mail.Subject = string.Format(_localizer["EmailFloodAlertSubject"].Value, prediction.City);
                mail.Body = $@"
{_localizer["DearResident"].Value},

{_localizer["DisasterAlert"].Value}

📍 {_localizer["Location"].Value}: {prediction.City}
📅 {_localizer["Date"].Value}: {prediction.Month} {prediction.Year}
⚠️ {_localizer["Status"].Value}: {_localizer["HighRiskPredicted"].Value}

{_localizer["DisasterTips"].Value}
• {_localizer["MonitorUpdates"].Value}
• {_localizer["PrepareSupplies"].Value}
• {_localizer["IdentifyRoutes"].Value}
• {_localizer["KeepDocumentsSafe"].Value}
• {_localizer["StayInformed"].Value}

{_localizer["StaySafe"].Value}

{_localizer["AppName"].Value}
";
                mail.IsBodyHtml = false;

                smtpServer.Port = _emailSettings.SmtpPort;
                smtpServer.Credentials = new System.Net.NetworkCredential(
                    _emailSettings.SenderEmail, 
                    _emailSettings.SenderPassword);
                smtpServer.EnableSsl = _emailSettings.EnableSsl;

                await Task.Run(() => smtpServer.Send(mail));
                
                _logger.LogInformation(
                    "Flood alert sent to {Count} recipients for {City}", 
                    mail.To.Count, 
                    prediction.City);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send flood alert for {City}", prediction.City);
                return false;
            }
        }

        public async Task<bool> SendWelcomeEmailAsync(string email, string userName)
        {
            if (string.IsNullOrEmpty(_emailSettings.SenderEmail) || 
                string.IsNullOrEmpty(_emailSettings.SenderPassword))
            {
                _logger.LogWarning("Email settings not configured");
                return false;
            }

            try
            {
                using var mail = new MailMessage();
                using var smtpServer = new SmtpClient(_emailSettings.SmtpServer);

                mail.From = new MailAddress(_emailSettings.SenderEmail);
                mail.To.Add(email);
                mail.Subject = "Welcome to Lagos Flood Detection System";
                mail.Body = $@"
Dear {userName},

Welcome to the Lagos Flood Detection System!

Your account has been successfully created. You can now:
• View flood risk predictions for your area
• Receive real-time alerts
• Access historical data and trends
• Stay informed about weather conditions

Login to your dashboard to get started.

Thank you for joining us in making Lagos safer!

Best regards,
Lagos Flood Detection System Team
";
                mail.IsBodyHtml = false;

                smtpServer.Port = _emailSettings.SmtpPort;
                smtpServer.Credentials = new System.Net.NetworkCredential(
                    _emailSettings.SenderEmail, 
                    _emailSettings.SenderPassword);
                smtpServer.EnableSsl = _emailSettings.EnableSsl;

                await Task.Run(() => smtpServer.Send(mail));
                
                _logger.LogInformation("Welcome email sent to {Email}", email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send welcome email to {Email}", email);
                return false;
            }
        }

        public async Task<bool> SendBulkFloodAlertsAsync(List<CityFloodPrediction> predictions)
        {
            var successCount = 0;
            var failureCount = 0;

            foreach (var prediction in predictions.Where(p => p.Prediction == true.ToString()))
            {
                var users = await _unitOfWork.Users.FindAsync(u => u.CityId == prediction.CityId);
                var recipients = users
                    .Select(u => u.Email)
                    .Where(e => !string.IsNullOrEmpty(e))
                    .Cast<string>()
                    .ToList();

                if (recipients.Any())
                {
                    var success = await SendFloodAlertAsync(prediction, recipients);
                    if (success)
                        successCount++;
                    else
                        failureCount++;
                }
            }

            _logger.LogInformation(
                "Bulk flood alerts completed. Success: {Success}, Failed: {Failed}", 
                successCount, 
                failureCount);

            return failureCount == 0;
        }
    }
}
