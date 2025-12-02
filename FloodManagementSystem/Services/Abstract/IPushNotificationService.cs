using GlobalDisasterManagement.Models;

namespace GlobalDisasterManagement.Services.Abstract
{
    /// <summary>
    /// Service interface for Firebase Cloud Messaging push notifications
    /// </summary>
    public interface IPushNotificationService
    {
        /// <summary>
        /// Register a new device token for push notifications
        /// </summary>
        Task<DeviceToken> RegisterDeviceAsync(string token, string? userId, DevicePlatform platform, string? deviceInfo = null);

        /// <summary>
        /// Update device token last used timestamp
        /// </summary>
        Task UpdateDeviceLastUsedAsync(string token);

        /// <summary>
        /// Deactivate a device token
        /// </summary>
        Task DeactivateDeviceAsync(string token);

        /// <summary>
        /// Send push notification to a single device
        /// </summary>
        Task<bool> SendToDeviceAsync(string deviceToken, string title, string body, string? imageUrl = null, Dictionary<string, string>? data = null, MessagePriority priority = MessagePriority.Normal);

        /// <summary>
        /// Send push notification to multiple devices
        /// </summary>
        Task<(int successCount, int failureCount)> SendToMultipleDevicesAsync(List<string> deviceTokens, string title, string body, string? imageUrl = null, Dictionary<string, string>? data = null, MessagePriority priority = MessagePriority.Normal);

        /// <summary>
        /// Send push notification to a topic
        /// </summary>
        Task<bool> SendToTopicAsync(string topic, string title, string body, string? imageUrl = null, Dictionary<string, string>? data = null, MessagePriority priority = MessagePriority.Normal);

        /// <summary>
        /// Send disaster alert notification to all subscribed devices
        /// </summary>
        Task<(int successCount, int failureCount)> SendDisasterAlertNotificationAsync(DisasterAlert alert);

        /// <summary>
        /// Send bulk disaster alert notifications to multiple users
        /// </summary>
        Task<int> SendBulkDisasterAlertsAsync(DisasterAlert alert, List<string> userIds);

        /// <summary>
        /// Send incident confirmation notification to user
        /// </summary>
        Task<bool> SendIncidentConfirmationNotificationAsync(DisasterIncident incident, string userId);

        /// <summary>
        /// Send shelter information notification
        /// </summary>
        Task<bool> SendShelterInfoNotificationAsync(EmergencyShelter shelter, string userId);

        /// <summary>
        /// Subscribe device to a topic
        /// </summary>
        Task<bool> SubscribeToTopicAsync(string deviceToken, string topic);

        /// <summary>
        /// Subscribe multiple devices to a topic
        /// </summary>
        Task<(int successCount, int failureCount)> SubscribeToTopicAsync(List<string> deviceTokens, string topic);

        /// <summary>
        /// Unsubscribe device from a topic
        /// </summary>
        Task<bool> UnsubscribeFromTopicAsync(string deviceToken, string topic);

        /// <summary>
        /// Unsubscribe multiple devices from a topic
        /// </summary>
        Task<(int successCount, int failureCount)> UnsubscribeFromTopicAsync(List<string> deviceTokens, string topic);

        /// <summary>
        /// Get notification statistics (sent count, failed count, etc.)
        /// </summary>
        Task<Dictionary<string, int>> GetNotificationStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get user's active device tokens
        /// </summary>
        Task<List<DeviceToken>> GetUserDeviceTokensAsync(string userId);

        /// <summary>
        /// Get devices subscribed to a topic
        /// </summary>
        Task<List<DeviceToken>> GetTopicSubscribersAsync(string topic);
    }
}
