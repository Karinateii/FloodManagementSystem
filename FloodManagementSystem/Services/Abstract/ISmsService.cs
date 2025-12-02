using GlobalDisasterManagement.Models;

namespace GlobalDisasterManagement.Services.Abstract
{
    public interface ISmsService
    {
        /// <summary>
        /// Send SMS to a single recipient
        /// </summary>
        Task<bool> SendSmsAsync(string phoneNumber, string message, MessagePriority priority = MessagePriority.Normal);

        /// <summary>
        /// Send SMS to multiple recipients
        /// </summary>
        Task<int> SendBulkSmsAsync(List<string> phoneNumbers, string message, MessagePriority priority = MessagePriority.Normal);

        /// <summary>
        /// Send disaster alert SMS
        /// </summary>
        Task<bool> SendDisasterAlertAsync(DisasterIncident incident, List<string> phoneNumbers);

        /// <summary>
        /// Send shelter capacity update SMS
        /// </summary>
        Task<bool> SendShelterUpdateAsync(EmergencyShelter shelter, List<string> phoneNumbers);

        /// <summary>
        /// Send evacuation order SMS
        /// </summary>
        Task<bool> SendEvacuationOrderAsync(string area, string instructions, List<string> phoneNumbers);

        /// <summary>
        /// Send all-clear message
        /// </summary>
        Task<bool> SendAllClearAsync(string area, List<string> phoneNumbers);

        /// <summary>
        /// Get SMS notification history
        /// </summary>
        Task<List<SmsNotification>> GetNotificationHistoryAsync(int page = 1, int pageSize = 50);

        /// <summary>
        /// Get notification status by Message SID
        /// </summary>
        Task<NotificationStatus> GetNotificationStatusAsync(string messageSid);

        /// <summary>
        /// Retry failed SMS notifications
        /// </summary>
        Task<int> RetryFailedNotificationsAsync();

        /// <summary>
        /// Get user's phone number from database
        /// </summary>
        Task<List<string>> GetUserPhoneNumbersByCityAsync(int cityId);

        /// <summary>
        /// Get user's phone numbers by LGA
        /// </summary>
        Task<List<string>> GetUserPhoneNumbersByLgaAsync(int lgaId);
    }
}
