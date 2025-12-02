namespace GlobalDisasterManagement.Models.DTO.Analytics
{
    /// <summary>
    /// Dashboard Key Performance Indicators
    /// </summary>
    public class DashboardKpiDto
    {
        // Alert Metrics
        public int TotalAlerts { get; set; }
        public int ActiveAlerts { get; set; }
        public int ResolvedAlerts { get; set; }
        public double AverageAlertResolutionTime { get; set; } // in hours
        public double AlertEffectivenessRate { get; set; } // percentage

        // Incident Metrics
        public int TotalIncidents { get; set; }
        public int PendingIncidents { get; set; }
        public int ResolvedIncidents { get; set; }
        public double AverageResponseTime { get; set; } // in minutes
        public double IncidentResolutionRate { get; set; } // percentage

        // Shelter Metrics
        public int TotalShelters { get; set; }
        public int ActiveShelters { get; set; }
        public int TotalCapacity { get; set; }
        public int CurrentOccupancy { get; set; }
        public double ShelterUtilizationRate { get; set; } // percentage

        // IoT Sensor Metrics
        public int TotalSensors { get; set; }
        public int ActiveSensors { get; set; }
        public int InactiveSensors { get; set; }
        public int MaintenanceSensors { get; set; }
        public double SensorHealthRate { get; set; } // percentage

        // User Engagement
        public int TotalUsers { get; set; }
        public int ActiveUsersToday { get; set; }
        public int NewUsersThisWeek { get; set; }

        // Notification Metrics
        public int NotificationsSentToday { get; set; }
        public int SmsNotifications { get; set; }
        public int PushNotifications { get; set; }
        public int WhatsAppNotifications { get; set; }
        public int VoiceNotifications { get; set; }

        // Period Information
        public DateTime LastUpdated { get; set; }
        public string Period { get; set; } = "Last 24 hours";
    }
}
