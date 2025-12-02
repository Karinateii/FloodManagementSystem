using GlobalDisasterManagement.Data;
using GlobalDisasterManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace GlobalDisasterManagement.Data
{
    public class SmsTemplateSeeder
    {
        private readonly DisasterDbContext _context;
        private readonly ILogger<SmsTemplateSeeder> _logger;

        public SmsTemplateSeeder(DisasterDbContext context, ILogger<SmsTemplateSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                // Check if templates already exist
                if (await _context.SmsTemplates.AnyAsync())
                {
                    _logger.LogInformation("SMS templates already exist. Skipping seed.");
                    return;
                }

                _logger.LogInformation("Seeding SMS templates...");

                var templates = new List<SmsTemplate>
                {
                    // Incident Alert Templates
                    new SmsTemplate
                    {
                        Name = "Disaster Alert - English",
                        Type = NotificationType.IncidentAlert,
                        LanguageCode = "en",
                        TemplateText = "⚠️ DISASTER ALERT\n\nType: {DisasterType}\nSeverity: {Severity}\nLocation: {Location}\nDetails: {Details}\nTime: {Time}\n\nStay safe and follow official instructions.",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new SmsTemplate
                    {
                        Name = "Flood Alert - English",
                        Type = NotificationType.IncidentAlert,
                        LanguageCode = "en",
                        TemplateText = "⚠️ FLOOD ALERT\n\nFlood warning for {Location}\nSeverity: {Severity}\nExpected water level: {Details}\nReported: {Time}\n\nMove to higher ground immediately if instructed.",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new SmsTemplate
                    {
                        Name = "Earthquake Alert - English",
                        Type = NotificationType.IncidentAlert,
                        LanguageCode = "en",
                        TemplateText = "⚠️ EARTHQUAKE ALERT\n\nMagnitude: {Severity}\nEpicenter: {Location}\nTime: {Time}\n\nDROP, COVER, HOLD ON. Expect aftershocks. Check for injuries and damage.",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new SmsTemplate
                    {
                        Name = "Fire Alert - English",
                        Type = NotificationType.IncidentAlert,
                        LanguageCode = "en",
                        TemplateText = "🔥 FIRE ALERT\n\nLocation: {Location}\nSize: {Severity}\nDetails: {Details}\nTime: {Time}\n\nEvacuate if instructed. Stay clear of smoke. Emergency: 911",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },

                    // Shelter Update Templates
                    new SmsTemplate
                    {
                        Name = "Shelter Opening - English",
                        Type = NotificationType.ShelterUpdate,
                        LanguageCode = "en",
                        TemplateText = "🏠 EMERGENCY SHELTER OPEN\n\nName: {ShelterName}\nLocation: {Address}\nCapacity: {Capacity} available\nFacilities: {Facilities}\nContact: {Phone}\n\nOpen 24/7. Bring ID if possible.",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new SmsTemplate
                    {
                        Name = "Shelter Capacity Update - English",
                        Type = NotificationType.ShelterUpdate,
                        LanguageCode = "en",
                        TemplateText = "🏠 SHELTER UPDATE\n\n{ShelterName}\n{AvailableSpaces} spaces available\nFacilities: {Facilities}\nContact: {Phone}\n\nFirst come, first served.",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },

                    // Evacuation Order Templates
                    new SmsTemplate
                    {
                        Name = "Immediate Evacuation - English",
                        Type = NotificationType.EvacuationOrder,
                        LanguageCode = "en",
                        TemplateText = "🚨 IMMEDIATE EVACUATION ORDER\n\nArea: {Area}\nAction: EVACUATE NOW\n\nInstructions:\n{Instructions}\n\nDo not delay. Follow designated routes. Emergency: 911",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new SmsTemplate
                    {
                        Name = "Voluntary Evacuation - English",
                        Type = NotificationType.EvacuationOrder,
                        LanguageCode = "en",
                        TemplateText = "⚠️ VOLUNTARY EVACUATION ADVISED\n\nArea: {Area}\nReason: {Instructions}\n\nResidents advised to evacuate if possible. Shelters available. Monitor updates.",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },

                    // Weather Warning Templates
                    new SmsTemplate
                    {
                        Name = "Severe Weather - English",
                        Type = NotificationType.WeatherWarning,
                        LanguageCode = "en",
                        TemplateText = "🌪️ SEVERE WEATHER WARNING\n\nType: {WarningType}\nArea: {Area}\nExpected: {Time}\n\nSafety advice:\n{SafetyAdvice}\n\nStay indoors. Monitor weather updates.",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new SmsTemplate
                    {
                        Name = "Hurricane Warning - English",
                        Type = NotificationType.WeatherWarning,
                        LanguageCode = "en",
                        TemplateText = "🌀 HURRICANE WARNING\n\nCategory: {Severity}\nLandfall: {Time}\nArea: {Area}\n\nSecure property. Stock supplies. Follow evacuation orders. Stay tuned.",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },

                    // All Clear Templates
                    new SmsTemplate
                    {
                        Name = "All Clear - English",
                        Type = NotificationType.AllClear,
                        LanguageCode = "en",
                        TemplateText = "✅ ALL CLEAR\n\nArea: {Area}\nStatus: Safe to return\n\n{Message}\n\nCheck your property for damage. Report hazards to authorities.",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new SmsTemplate
                    {
                        Name = "Partial All Clear - English",
                        Type = NotificationType.AllClear,
                        LanguageCode = "en",
                        TemplateText = "✅ PARTIAL ALL CLEAR\n\nArea: {Area}\n{Message}\n\nSome areas still restricted. Follow official guidance. Caution advised.",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },

                    // Resource Available Templates
                    new SmsTemplate
                    {
                        Name = "Resource Distribution - English",
                        Type = NotificationType.ResourceAvailable,
                        LanguageCode = "en",
                        TemplateText = "📦 RESOURCE DISTRIBUTION\n\nResource: {ResourceType}\nLocation: {Location}\nSchedule: {Schedule}\n\nRequirements: {Requirements}\n\nBring ID. First come, first served.",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new SmsTemplate
                    {
                        Name = "Water Distribution - English",
                        Type = NotificationType.ResourceAvailable,
                        LanguageCode = "en",
                        TemplateText = "💧 WATER DISTRIBUTION\n\nLocation: {Location}\nTime: {Schedule}\n\nFree bottled water available. Bring containers if possible. Limit per household.",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new SmsTemplate
                    {
                        Name = "Food Distribution - English",
                        Type = NotificationType.ResourceAvailable,
                        LanguageCode = "en",
                        TemplateText = "🍱 FOOD DISTRIBUTION\n\nLocation: {Location}\nTime: {Schedule}\n\nHot meals available. Bring family ID if possible. All welcome.",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },

                    // System Announcement Templates
                    new SmsTemplate
                    {
                        Name = "System Update - English",
                        Type = NotificationType.SystemAnnouncement,
                        LanguageCode = "en",
                        TemplateText = "📢 SYSTEM UPDATE\n\n{AnnouncementText}\n\nGlobal Disaster Management System",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new SmsTemplate
                    {
                        Name = "Emergency Hotline - English",
                        Type = NotificationType.SystemAnnouncement,
                        LanguageCode = "en",
                        TemplateText = "📞 EMERGENCY HOTLINE\n\n{AnnouncementText}\n\nFor emergencies: 911\nFor info: {HotlineNumber}\n\nAvailable 24/7",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },

                    // Incident Verification Templates
                    new SmsTemplate
                    {
                        Name = "Verify Incident Report - English",
                        Type = NotificationType.IncidentVerification,
                        LanguageCode = "en",
                        TemplateText = "✓ INCIDENT REPORT RECEIVED\n\nReference: {ReferenceNumber}\nType: {DisasterType}\nLocation: {Location}\n\nThank you for reporting. We are investigating. Updates will follow.",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },

                    // Status Update Templates
                    new SmsTemplate
                    {
                        Name = "Incident Status Update - English",
                        Type = NotificationType.StatusUpdate,
                        LanguageCode = "en",
                        TemplateText = "📊 STATUS UPDATE\n\nIncident: {IncidentType}\nLocation: {Location}\nStatus: {Status}\n\n{UpdateMessage}\n\nNext update: {NextUpdateTime}",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new SmsTemplate
                    {
                        Name = "Response Team Deployed - English",
                        Type = NotificationType.StatusUpdate,
                        LanguageCode = "en",
                        TemplateText = "🚒 RESPONSE TEAM DEPLOYED\n\nLocation: {Location}\nTeam: {TeamType}\nETA: {ETA}\n\nHelp is on the way. Stay safe.",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                await _context.SmsTemplates.AddRangeAsync(templates);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully seeded {Count} SMS templates", templates.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding SMS templates");
                throw;
            }
        }
    }
}
