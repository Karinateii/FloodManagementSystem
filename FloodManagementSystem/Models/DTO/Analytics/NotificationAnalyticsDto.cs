namespace GlobalDisasterManagement.Models.DTO.Analytics
{
    /// <summary>
    /// Multi-channel notification analytics
    /// </summary>
    public class NotificationAnalyticsDto
    {
        public int TotalNotificationsSent { get; set; }
        public int NotificationsToday { get; set; }
        public int NotificationsThisWeek { get; set; }
        public int NotificationsThisMonth { get; set; }
        
        // Channel breakdown
        public int SmsNotifications { get; set; }
        public int PushNotifications { get; set; }
        public int WhatsAppNotifications { get; set; }
        public int VoiceNotifications { get; set; }
        public int UssdSessions { get; set; }
        
        // Delivery status
        public int SuccessfulDeliveries { get; set; }
        public int FailedDeliveries { get; set; }
        public int PendingDeliveries { get; set; }
        public double DeliverySuccessRate { get; set; } // percentage
        
        public List<NotificationChannelStatsDto> ChannelStatistics { get; set; } = new();
        public List<TimeSeriesDataDto> NotificationTrend { get; set; } = new();
        public List<NotificationByTypeDto> NotificationsByDisasterType { get; set; } = new();
    }

    /// <summary>
    /// Statistics per notification channel
    /// </summary>
    public class NotificationChannelStatsDto
    {
        public string ChannelName { get; set; } = string.Empty;
        public int TotalSent { get; set; }
        public int Successful { get; set; }
        public int Failed { get; set; }
        public double SuccessRate { get; set; }
        public double AverageDeliveryTime { get; set; } // seconds
    }

    /// <summary>
    /// Notifications grouped by disaster type
    /// </summary>
    public class NotificationByTypeDto
    {
        public string DisasterType { get; set; } = string.Empty;
        public int NotificationCount { get; set; }
        public int RecipientsReached { get; set; }
    }
}
