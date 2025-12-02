using GlobalDisasterManagement.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GlobalDisasterManagement.Data
{
    public class DisasterDbContext : IdentityDbContext<User>
    {
        public DisasterDbContext(DbContextOptions<DisasterDbContext> options) : base(options)
        {
        }

        public DbSet<City> Cities { get; set; }
        public DbSet<LGA> LGAs { get; set; }
        public new DbSet<User> Users { get; set; }
        public DbSet<CsvFile> CsvFiles { get; set; }
        public DbSet<CsvFileCity> CsvFileCities { get; set; }
        public DbSet<CityFloodPrediction> CityPredictions { get; set; }
        
        // Emergency Response Entities
        public DbSet<DisasterIncident> DisasterIncidents { get; set; }
        public DbSet<DisasterAlert> DisasterAlerts { get; set; }
        public DbSet<DisasterResource> DisasterResources { get; set; }
        public DbSet<EmergencyShelter> EmergencyShelters { get; set; }
        public DbSet<ShelterCheckIn> ShelterCheckIns { get; set; }
        public DbSet<EvacuationRoute> EvacuationRoutes { get; set; }
        public DbSet<EmergencyContact> EmergencyContacts { get; set; }
        
        // Communication Entities
        public DbSet<SmsNotification> SmsNotifications { get; set; }
        public DbSet<SmsTemplate> SmsTemplates { get; set; }
        public DbSet<UssdSession> UssdSessions { get; set; }
        public DbSet<WhatsAppMessage> WhatsAppMessages { get; set; }
        public DbSet<VoiceCall> VoiceCalls { get; set; }
        public DbSet<DeviceToken> DeviceTokens { get; set; }
        public DbSet<PushNotification> PushNotifications { get; set; }
        
        // IoT and Real-Time Monitoring Entities
        public DbSet<WaterLevelSensor> WaterLevelSensors { get; set; }
        public DbSet<WaterLevelReading> WaterLevelReadings { get; set; }
        public DbSet<RainfallSensor> RainfallSensors { get; set; }
        public DbSet<RainfallReading> RainfallReadings { get; set; }
        public DbSet<WeatherSensor> WeatherSensors { get; set; }
        public DbSet<WeatherReading> WeatherReadings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Add indexes for better query performance
            modelBuilder.Entity<City>()
                .HasIndex(c => c.LGAId)
                .HasDatabaseName("IX_Cities_LGAId");

            modelBuilder.Entity<City>()
                .HasIndex(c => c.Name)
                .HasDatabaseName("IX_Cities_Name");

            modelBuilder.Entity<LGA>()
                .HasIndex(l => l.LGAName)
                .HasDatabaseName("IX_LGAs_Name");

            modelBuilder.Entity<CityFloodPrediction>()
                .HasIndex(p => p.CityId)
                .HasDatabaseName("IX_CityPredictions_CityId");

            modelBuilder.Entity<CityFloodPrediction>()
                .HasIndex(p => p.Year)
                .HasDatabaseName("IX_CityPredictions_Year");

            modelBuilder.Entity<CityFloodPrediction>()
                .HasIndex(p => p.Prediction)
                .HasDatabaseName("IX_CityPredictions_Prediction");

            modelBuilder.Entity<CityFloodPrediction>()
                .HasIndex(p => new { p.CityId, p.Year, p.Month })
                .HasDatabaseName("IX_CityPredictions_CityId_Year_Month");

            modelBuilder.Entity<User>()
                .HasIndex(u => u.CityId)
                .HasDatabaseName("IX_Users_CityId");

            modelBuilder.Entity<User>()
                .HasIndex(u => u.LGAId)
                .HasDatabaseName("IX_Users_LGAId");

            modelBuilder.Entity<CsvFile>()
                .HasIndex(c => c.UploadDateTime)
                .HasDatabaseName("IX_CsvFiles_UploadDateTime");

            // Emergency Response Indexes
            modelBuilder.Entity<DisasterIncident>()
                .HasIndex(i => i.DisasterType)
                .HasDatabaseName("IX_DisasterIncidents_DisasterType");

            modelBuilder.Entity<DisasterIncident>()
                .HasIndex(i => i.Status)
                .HasDatabaseName("IX_DisasterIncidents_Status");

            modelBuilder.Entity<DisasterIncident>()
                .HasIndex(i => i.Severity)
                .HasDatabaseName("IX_DisasterIncidents_Severity");

            modelBuilder.Entity<DisasterIncident>()
                .HasIndex(i => i.ReportedAt)
                .HasDatabaseName("IX_DisasterIncidents_ReportedAt");

            modelBuilder.Entity<DisasterIncident>()
                .HasIndex(i => new { i.DisasterType, i.Status })
                .HasDatabaseName("IX_DisasterIncidents_DisasterType_Status");

            modelBuilder.Entity<DisasterIncident>()
                .HasIndex(i => new { i.CityId, i.Status })
                .HasDatabaseName("IX_DisasterIncidents_CityId_Status");

            modelBuilder.Entity<EmergencyShelter>()
                .HasIndex(s => s.IsActive)
                .HasDatabaseName("IX_EmergencyShelters_IsActive");

            modelBuilder.Entity<EmergencyShelter>()
                .HasIndex(s => new { s.CityId, s.IsActive })
                .HasDatabaseName("IX_EmergencyShelters_CityId_IsActive");

            modelBuilder.Entity<ShelterCheckIn>()
                .HasIndex(c => c.ShelterId)
                .HasDatabaseName("IX_ShelterCheckIns_ShelterId");

            modelBuilder.Entity<ShelterCheckIn>()
                .HasIndex(c => c.IsCheckedOut)
                .HasDatabaseName("IX_ShelterCheckIns_IsCheckedOut");

            modelBuilder.Entity<EvacuationRoute>()
                .HasIndex(r => r.Status)
                .HasDatabaseName("IX_EvacuationRoutes_Status");

            modelBuilder.Entity<EvacuationRoute>()
                .HasIndex(r => new { r.CityId, r.Status })
                .HasDatabaseName("IX_EvacuationRoutes_CityId_Status");

            modelBuilder.Entity<EmergencyContact>()
                .HasIndex(e => e.ServiceType)
                .HasDatabaseName("IX_EmergencyContacts_ServiceType");

            modelBuilder.Entity<EmergencyContact>()
                .HasIndex(e => e.IsActive)
                .HasDatabaseName("IX_EmergencyContacts_IsActive");

            modelBuilder.Entity<DisasterAlert>()
                .HasIndex(a => a.DisasterType)
                .HasDatabaseName("IX_DisasterAlerts_DisasterType");

            modelBuilder.Entity<DisasterAlert>()
                .HasIndex(a => a.Status)
                .HasDatabaseName("IX_DisasterAlerts_Status");

            modelBuilder.Entity<DisasterAlert>()
                .HasIndex(a => a.IssuedAt)
                .HasDatabaseName("IX_DisasterAlerts_IssuedAt");

            modelBuilder.Entity<DisasterResource>()
                .HasIndex(r => r.Type)
                .HasDatabaseName("IX_DisasterResources_Type");

            modelBuilder.Entity<DisasterResource>()
                .HasIndex(r => r.Status)
                .HasDatabaseName("IX_DisasterResources_Status");

            modelBuilder.Entity<DisasterResource>()
                .HasIndex(r => new { r.Type, r.Status })
                .HasDatabaseName("IX_DisasterResources_Type_Status");

            // SMS Notification indexes
            modelBuilder.Entity<SmsNotification>()
                .HasIndex(s => s.Status)
                .HasDatabaseName("IX_SmsNotifications_Status");

            modelBuilder.Entity<SmsNotification>()
                .HasIndex(s => s.SentAt)
                .HasDatabaseName("IX_SmsNotifications_SentAt");

            modelBuilder.Entity<SmsNotification>()
                .HasIndex(s => s.PhoneNumber)
                .HasDatabaseName("IX_SmsNotifications_PhoneNumber");

            modelBuilder.Entity<SmsTemplate>()
                .HasIndex(t => t.Type)
                .HasDatabaseName("IX_SmsTemplates_Type");

            modelBuilder.Entity<SmsTemplate>()
                .HasIndex(t => new { t.LanguageCode, t.Type })
                .HasDatabaseName("IX_SmsTemplates_Language_Type");

            // USSD Session indexes
            modelBuilder.Entity<UssdSession>()
                .HasIndex(u => u.SessionId)
                .HasDatabaseName("IX_UssdSessions_SessionId");

            modelBuilder.Entity<UssdSession>()
                .HasIndex(u => u.PhoneNumber)
                .HasDatabaseName("IX_UssdSessions_PhoneNumber");

            modelBuilder.Entity<UssdSession>()
                .HasIndex(u => u.IsActive)
                .HasDatabaseName("IX_UssdSessions_IsActive");

            modelBuilder.Entity<UssdSession>()
                .HasIndex(u => u.LastActivityAt)
                .HasDatabaseName("IX_UssdSessions_LastActivityAt");

            // WhatsApp Message indexes
            modelBuilder.Entity<WhatsAppMessage>()
                .HasIndex(w => w.MessageSid)
                .IsUnique()
                .HasDatabaseName("IX_WhatsAppMessages_MessageSid");

            modelBuilder.Entity<WhatsAppMessage>()
                .HasIndex(w => w.ToPhoneNumber)
                .HasDatabaseName("IX_WhatsAppMessages_ToPhoneNumber");

            modelBuilder.Entity<WhatsAppMessage>()
                .HasIndex(w => w.Status)
                .HasDatabaseName("IX_WhatsAppMessages_Status");

            modelBuilder.Entity<WhatsAppMessage>()
                .HasIndex(w => w.CreatedAt)
                .HasDatabaseName("IX_WhatsAppMessages_CreatedAt");

            modelBuilder.Entity<WhatsAppMessage>()
                .HasIndex(w => w.ConversationId)
                .HasDatabaseName("IX_WhatsAppMessages_ConversationId");

            // Voice Call indexes
            modelBuilder.Entity<VoiceCall>()
                .HasIndex(v => v.CallSid)
                .IsUnique()
                .HasDatabaseName("IX_VoiceCalls_CallSid");

            modelBuilder.Entity<VoiceCall>()
                .HasIndex(v => v.ToPhoneNumber)
                .HasDatabaseName("IX_VoiceCalls_ToPhoneNumber");

            modelBuilder.Entity<VoiceCall>()
                .HasIndex(v => v.Status)
                .HasDatabaseName("IX_VoiceCalls_Status");

            modelBuilder.Entity<VoiceCall>()
                .HasIndex(v => v.CreatedAt)
                .HasDatabaseName("IX_VoiceCalls_CreatedAt");

            modelBuilder.Entity<VoiceCall>()
                .HasIndex(v => v.DisasterAlertId)
                .HasDatabaseName("IX_VoiceCalls_DisasterAlertId");

            // Device Token indexes
            modelBuilder.Entity<DeviceToken>()
                .HasIndex(dt => dt.Token)
                .IsUnique()
                .HasDatabaseName("IX_DeviceTokens_Token");

            modelBuilder.Entity<DeviceToken>()
                .HasIndex(dt => dt.UserId)
                .HasDatabaseName("IX_DeviceTokens_UserId");

            modelBuilder.Entity<DeviceToken>()
                .HasIndex(dt => dt.Platform)
                .HasDatabaseName("IX_DeviceTokens_Platform");

            modelBuilder.Entity<DeviceToken>()
                .HasIndex(dt => dt.IsActive)
                .HasDatabaseName("IX_DeviceTokens_IsActive");

            modelBuilder.Entity<DeviceToken>()
                .HasIndex(dt => dt.LastUsedAt)
                .HasDatabaseName("IX_DeviceTokens_LastUsedAt");

            // Push Notification indexes
            modelBuilder.Entity<PushNotification>()
                .HasIndex(pn => pn.Status)
                .HasDatabaseName("IX_PushNotifications_Status");

            modelBuilder.Entity<PushNotification>()
                .HasIndex(pn => pn.DisasterAlertId)
                .HasDatabaseName("IX_PushNotifications_DisasterAlertId");

            modelBuilder.Entity<PushNotification>()
                .HasIndex(pn => pn.CreatedAt)
                .HasDatabaseName("IX_PushNotifications_CreatedAt");

            modelBuilder.Entity<PushNotification>()
                .HasIndex(pn => pn.Topic)
                .HasDatabaseName("IX_PushNotifications_Topic");

            modelBuilder.Entity<PushNotification>()
                .HasIndex(pn => pn.Topic)
                .HasDatabaseName("IX_PushNotifications_Topic");

            // IoT Sensor indexes
            modelBuilder.Entity<WaterLevelSensor>()
                .HasIndex(s => s.DeviceId)
                .IsUnique()
                .HasDatabaseName("IX_WaterLevelSensors_DeviceId");

            modelBuilder.Entity<WaterLevelSensor>()
                .HasIndex(s => s.Status)
                .HasDatabaseName("IX_WaterLevelSensors_Status");

            modelBuilder.Entity<WaterLevelSensor>()
                .HasIndex(s => new { s.CityId, s.Status })
                .HasDatabaseName("IX_WaterLevelSensors_CityId_Status");

            modelBuilder.Entity<WaterLevelReading>()
                .HasIndex(r => r.SensorId)
                .HasDatabaseName("IX_WaterLevelReadings_SensorId");

            modelBuilder.Entity<WaterLevelReading>()
                .HasIndex(r => r.Timestamp)
                .HasDatabaseName("IX_WaterLevelReadings_Timestamp");

            modelBuilder.Entity<WaterLevelReading>()
                .HasIndex(r => new { r.SensorId, r.Timestamp })
                .HasDatabaseName("IX_WaterLevelReadings_SensorId_Timestamp");

            modelBuilder.Entity<RainfallSensor>()
                .HasIndex(s => s.DeviceId)
                .IsUnique()
                .HasDatabaseName("IX_RainfallSensors_DeviceId");

            modelBuilder.Entity<RainfallSensor>()
                .HasIndex(s => s.Status)
                .HasDatabaseName("IX_RainfallSensors_Status");

            modelBuilder.Entity<RainfallSensor>()
                .HasIndex(s => new { s.CityId, s.Status })
                .HasDatabaseName("IX_RainfallSensors_CityId_Status");

            modelBuilder.Entity<RainfallReading>()
                .HasIndex(r => r.SensorId)
                .HasDatabaseName("IX_RainfallReadings_SensorId");

            modelBuilder.Entity<RainfallReading>()
                .HasIndex(r => r.Timestamp)
                .HasDatabaseName("IX_RainfallReadings_Timestamp");

            modelBuilder.Entity<RainfallReading>()
                .HasIndex(r => new { r.SensorId, r.Timestamp })
                .HasDatabaseName("IX_RainfallReadings_SensorId_Timestamp");

            modelBuilder.Entity<WeatherSensor>()
                .HasIndex(s => s.DeviceId)
                .IsUnique()
                .HasDatabaseName("IX_WeatherSensors_DeviceId");

            modelBuilder.Entity<WeatherSensor>()
                .HasIndex(s => s.Status)
                .HasDatabaseName("IX_WeatherSensors_Status");

            modelBuilder.Entity<WeatherSensor>()
                .HasIndex(s => new { s.CityId, s.Status })
                .HasDatabaseName("IX_WeatherSensors_CityId_Status");

            modelBuilder.Entity<WeatherReading>()
                .HasIndex(r => r.SensorId)
                .HasDatabaseName("IX_WeatherReadings_SensorId");

            modelBuilder.Entity<WeatherReading>()
                .HasIndex(r => r.Timestamp)
                .HasDatabaseName("IX_WeatherReadings_Timestamp");

            modelBuilder.Entity<WeatherReading>()
                .HasIndex(r => new { r.SensorId, r.Timestamp })
                .HasDatabaseName("IX_WeatherReadings_SensorId_Timestamp");

            // Fix SmsNotification foreign key relationships
            modelBuilder.Entity<SmsNotification>()
                .HasOne(s => s.DisasterAlert)
                .WithMany()
                .HasForeignKey(s => s.DisasterAlertId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<SmsNotification>()
                .HasOne(s => s.DisasterIncident)
                .WithMany()
                .HasForeignKey(s => s.DisasterIncidentId)
                .OnDelete(DeleteBehavior.SetNull);

            // Fix DisasterResource decimal precision
            modelBuilder.Entity<DisasterResource>()
                .Property(r => r.EstimatedValue)
                .HasPrecision(18, 2);
        }
    }
}
