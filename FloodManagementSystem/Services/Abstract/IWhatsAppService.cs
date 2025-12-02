using GlobalDisasterManagement.Models;

namespace GlobalDisasterManagement.Services.Abstract
{
    /// <summary>
    /// Service for sending WhatsApp messages through Twilio WhatsApp Business API
    /// </summary>
    public interface IWhatsAppService
    {
        /// <summary>
        /// Send a text WhatsApp message
        /// </summary>
        Task<bool> SendTextMessageAsync(string toPhoneNumber, string message, Guid? incidentId = null, Guid? alertId = null);

        /// <summary>
        /// Send a WhatsApp message with media (image, video, document)
        /// </summary>
        Task<bool> SendMediaMessageAsync(string toPhoneNumber, string message, string mediaUrl, string mediaType, Guid? incidentId = null);

        /// <summary>
        /// Send a template WhatsApp message (pre-approved by WhatsApp)
        /// </summary>
        Task<bool> SendTemplateMessageAsync(string toPhoneNumber, string templateName, string[] parameters, Guid? incidentId = null);

        /// <summary>
        /// Send disaster alert to a single phone number via WhatsApp
        /// </summary>
        Task<bool> SendDisasterAlertAsync(string toPhoneNumber, DisasterAlert alert);

        /// <summary>
        /// Send disaster alert to multiple phone numbers via WhatsApp
        /// </summary>
        Task<int> SendBulkDisasterAlertAsync(List<string> phoneNumbers, DisasterAlert alert);

        /// <summary>
        /// Send incident report confirmation via WhatsApp
        /// </summary>
        Task<bool> SendIncidentConfirmationAsync(string toPhoneNumber, DisasterIncident incident);

        /// <summary>
        /// Send shelter information via WhatsApp with location
        /// </summary>
        Task<bool> SendShelterInfoAsync(string toPhoneNumber, EmergencyShelter shelter);

        /// <summary>
        /// Send evacuation instructions via WhatsApp
        /// </summary>
        Task<bool> SendEvacuationInstructionsAsync(string toPhoneNumber, EvacuationRoute route);

        /// <summary>
        /// Handle incoming WhatsApp messages (webhook)
        /// </summary>
        Task ProcessInboundMessageAsync(string from, string to, string body, string messageSid, string mediaUrl = null, string mediaType = null);

        /// <summary>
        /// Handle WhatsApp message status updates (webhook)
        /// </summary>
        Task ProcessMessageStatusAsync(string messageSid, string status, string errorCode = null, string errorMessage = null);

        /// <summary>
        /// Get WhatsApp message delivery statistics
        /// </summary>
        Task<Dictionary<string, int>> GetMessageStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Retry failed WhatsApp messages
        /// </summary>
        Task<int> RetryFailedMessagesAsync(int maxRetries = 3);

        /// <summary>
        /// Get conversation history for a phone number
        /// </summary>
        Task<List<WhatsAppMessage>> GetConversationHistoryAsync(string phoneNumber, int limit = 50);

        /// <summary>
        /// Subscribe user to WhatsApp disaster alerts
        /// </summary>
        Task<bool> SubscribeToAlertsAsync(string phoneNumber, string userId);

        /// <summary>
        /// Unsubscribe user from WhatsApp disaster alerts
        /// </summary>
        Task<bool> UnsubscribeFromAlertsAsync(string phoneNumber);
    }
}
