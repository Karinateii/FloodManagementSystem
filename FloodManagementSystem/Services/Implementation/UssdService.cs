using GlobalDisasterManagement.Data;
using GlobalDisasterManagement.Models;
using GlobalDisasterManagement.Services.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace GlobalDisasterManagement.Services.Implementation
{
    public class UssdService : IUssdService
    {
        private readonly DisasterDbContext _context;
        private readonly ILogger<UssdService> _logger;

        public UssdService(DisasterDbContext context, ILogger<UssdService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string> ProcessUssdRequestAsync(string sessionId, string phoneNumber, string input, UssdRequestType requestType)
        {
            try
            {
                var session = await GetOrCreateSessionAsync(sessionId, phoneNumber);

                // Clean input
                input = input?.Trim() ?? "";

                if (requestType == UssdRequestType.Initiation || session.CurrentState == UssdMenuState.MainMenu)
                {
                    // Show main menu
                    return FormatResponse(GetMainMenu(), ResponseType.Continue);
                }

                // Process based on current state
                return session.CurrentState switch
                {
                    UssdMenuState.MainMenu => await HandleMainMenuAsync(session, input),
                    UssdMenuState.SelectDisasterType => await HandleDisasterTypeSelectionAsync(session, input),
                    UssdMenuState.SelectCity => await HandleCitySelectionAsync(session, input),
                    UssdMenuState.SelectLGA => await HandleLGASelectionAsync(session, input),
                    UssdMenuState.EnterDescription => await HandleDescriptionEntryAsync(session, input),
                    UssdMenuState.ConfirmReport => await HandleReportConfirmationAsync(session, input),
                    UssdMenuState.ViewAlertsList => await HandleViewAlertsAsync(session, input),
                    UssdMenuState.ViewShelterList => await HandleViewSheltersAsync(session, input),
                    UssdMenuState.SafetyTips => await HandleSafetyTipsAsync(session, input),
                    _ => FormatResponse(GetMainMenu(), ResponseType.Continue)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing USSD request for session {SessionId}", sessionId);
                return FormatResponse("An error occurred. Please try again.", ResponseType.End);
            }
        }

        private async Task<string> HandleMainMenuAsync(UssdSession session, string input)
        {
            switch (input)
            {
                case "1": // Report Disaster
                    await UpdateSessionStateAsync(session.SessionId, UssdMenuState.SelectDisasterType);
                    return FormatResponse(GetDisasterTypesMenu(), ResponseType.Continue);

                case "2": // View Alerts
                    await UpdateSessionStateAsync(session.SessionId, UssdMenuState.ViewAlertsList);
                    var alerts = await GetRecentAlertsAsync(session.PhoneNumber);
                    return FormatResponse(alerts, ResponseType.End);

                case "3": // Find Shelter
                    await UpdateSessionStateAsync(session.SessionId, UssdMenuState.SelectCity);
                    var cityMenu = await GetCitySelectionMenuAsync();
                    return FormatResponse(cityMenu, ResponseType.Continue);

                case "4": // Emergency Contacts
                    await EndSessionAsync(session.SessionId);
                    return FormatResponse(GetEmergencyContacts(), ResponseType.End);

                case "5": // Safety Tips
                    await UpdateSessionStateAsync(session.SessionId, UssdMenuState.SafetyTips);
                    return FormatResponse(GetSafetyTips(), ResponseType.End);

                case "6": // Evacuation Info
                    await UpdateSessionStateAsync(session.SessionId, UssdMenuState.SelectCity);
                    var cityMenuEvac = await GetCitySelectionMenuAsync();
                    return FormatResponse(cityMenuEvac + "\nSelect city for evacuation info:", ResponseType.Continue);

                default:
                    return FormatResponse("Invalid option. Please try again.\n\n" + GetMainMenu(), ResponseType.Continue);
            }
        }

        private async Task<string> HandleDisasterTypeSelectionAsync(UssdSession session, string input)
        {
            var disasterTypes = new Dictionary<string, DisasterType>
            {
                { "1", DisasterType.Flood },
                { "2", DisasterType.Earthquake },
                { "3", DisasterType.Fire },
                { "4", DisasterType.Hurricane },
                { "5", DisasterType.Tornado },
                { "6", DisasterType.Tsunami },
                { "7", DisasterType.Drought },
                { "8", DisasterType.Landslide },
                { "9", DisasterType.Other }
            };

            if (disasterTypes.TryGetValue(input, out var disasterType))
            {
                // Store disaster type in context
                var context = new { DisasterType = disasterType.ToString() };
                await UpdateSessionStateAsync(session.SessionId, UssdMenuState.SelectCity, JsonSerializer.Serialize(context));

                var cityMenu = await GetCitySelectionMenuAsync();
                return FormatResponse(cityMenu + "\nSelect your city:", ResponseType.Continue);
            }

            return FormatResponse("Invalid option. Please try again.\n\n" + GetDisasterTypesMenu(), ResponseType.Continue);
        }

        private async Task<string> HandleCitySelectionAsync(UssdSession session, string input)
        {
            if (int.TryParse(input, out var cityNumber))
            {
                var cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();
                if (cityNumber > 0 && cityNumber <= cities.Count)
                {
                    var selectedCity = cities[cityNumber - 1];
                    
                    // Update context with city
                    var contextData = session.ContextData != null 
                        ? JsonSerializer.Deserialize<Dictionary<string, object>>(session.ContextData) 
                        : new Dictionary<string, object>();
                    
                    contextData!["CityId"] = selectedCity.Id;
                    contextData["CityName"] = selectedCity.Name;

                    await UpdateSessionStateAsync(session.SessionId, UssdMenuState.SelectLGA, JsonSerializer.Serialize(contextData));

                    var lgaMenu = await GetLGASelectionMenuAsync(selectedCity.Id);
                    return FormatResponse(lgaMenu + "\nSelect your LGA:", ResponseType.Continue);
                }
            }

            var cityMenuRetry = await GetCitySelectionMenuAsync();
            return FormatResponse("Invalid selection. Please try again.\n\n" + cityMenuRetry, ResponseType.Continue);
        }

        private async Task<string> HandleLGASelectionAsync(UssdSession session, string input)
        {
            // Get context
            var contextData = session.ContextData != null 
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(session.ContextData) 
                : new Dictionary<string, object>();

            if (contextData != null && contextData.TryGetValue("CityId", out var cityIdObj))
            {
                var cityId = Convert.ToInt32(cityIdObj.ToString());

                if (int.TryParse(input, out var lgaNumber))
                {
                    var lgas = await _context.LGAs.Where(l => l.Cities.Any(c => c.Id == cityId))
                        .OrderBy(l => l.LGAName).ToListAsync();

                    if (lgaNumber > 0 && lgaNumber <= lgas.Count)
                    {
                        var selectedLGA = lgas[lgaNumber - 1];
                        contextData["LGAId"] = selectedLGA.LGAId;
                        contextData["LGAName"] = selectedLGA.LGAName;

                        // Check if we're reporting an incident or viewing shelters
                        if (contextData.ContainsKey("DisasterType"))
                        {
                            // Reporting incident - ask for description
                            await UpdateSessionStateAsync(session.SessionId, UssdMenuState.EnterDescription, JsonSerializer.Serialize(contextData));
                            return FormatResponse("Please describe the incident in a few words:", ResponseType.Continue);
                        }
                        else
                        {
                            // Viewing shelters
                            await EndSessionAsync(session.SessionId);
                            var shelters = await GetNearbySheltersAsync(cityId, selectedLGA.LGAId);
                            return FormatResponse(shelters, ResponseType.End);
                        }
                    }
                }

                var lgaMenuRetry = await GetLGASelectionMenuAsync(cityId);
                return FormatResponse("Invalid selection. Please try again.\n\n" + lgaMenuRetry, ResponseType.Continue);
            }

            return FormatResponse("Session error. Please start again.", ResponseType.End);
        }

        private async Task<string> HandleDescriptionEntryAsync(UssdSession session, string input)
        {
            if (string.IsNullOrWhiteSpace(input) || input.Length < 5)
            {
                return FormatResponse("Description too short. Please provide at least 5 characters:", ResponseType.Continue);
            }

            // Store description in context
            var contextData = session.ContextData != null 
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(session.ContextData) 
                : new Dictionary<string, object>();

            contextData!["Description"] = input;
            await UpdateSessionStateAsync(session.SessionId, UssdMenuState.ConfirmReport, JsonSerializer.Serialize(contextData));

            var summary = $"Confirm Report:\n" +
                         $"Type: {contextData.GetValueOrDefault("DisasterType", "Unknown")}\n" +
                         $"City: {contextData.GetValueOrDefault("CityName", "Unknown")}\n" +
                         $"LGA: {contextData.GetValueOrDefault("LGAName", "Unknown")}\n" +
                         $"Details: {input}\n\n" +
                         $"1. Confirm & Submit\n" +
                         $"2. Cancel";

            return FormatResponse(summary, ResponseType.Continue);
        }

        private async Task<string> HandleReportConfirmationAsync(UssdSession session, string input)
        {
            if (input == "1")
            {
                // Submit the report
                var contextData = JsonSerializer.Deserialize<Dictionary<string, object>>(session.ContextData ?? "{}");

                if (contextData != null)
                {
                    var disasterType = Enum.Parse<DisasterType>(contextData.GetValueOrDefault("DisasterType", "Other").ToString()!);
                    var location = $"{contextData.GetValueOrDefault("CityName", "")}, {contextData.GetValueOrDefault("LGAName", "")}";
                    var description = contextData.GetValueOrDefault("Description", "").ToString() ?? "";

                    var success = await SubmitIncidentReportAsync(session.PhoneNumber, disasterType, location, description);

                    await EndSessionAsync(session.SessionId);

                    if (success)
                    {
                        return FormatResponse("Thank you! Your incident report has been submitted successfully. Authorities have been notified.", ResponseType.End);
                    }
                    else
                    {
                        return FormatResponse("Sorry, there was an error submitting your report. Please try calling our hotline.", ResponseType.End);
                    }
                }
            }
            else if (input == "2")
            {
                await EndSessionAsync(session.SessionId);
                return FormatResponse("Report cancelled. Stay safe!", ResponseType.End);
            }

            return FormatResponse("Invalid option. Enter 1 to confirm or 2 to cancel:", ResponseType.Continue);
        }

        private async Task<string> HandleViewAlertsAsync(UssdSession session, string input)
        {
            var alerts = await GetRecentAlertsAsync(session.PhoneNumber);
            await EndSessionAsync(session.SessionId);
            return FormatResponse(alerts, ResponseType.End);
        }

        private async Task<string> HandleViewSheltersAsync(UssdSession session, string input)
        {
            var shelters = await GetNearbySheltersAsync(null, null);
            await EndSessionAsync(session.SessionId);
            return FormatResponse(shelters, ResponseType.End);
        }

        private async Task<string> HandleSafetyTipsAsync(UssdSession session, string input)
        {
            var tips = GetSafetyTips();
            await EndSessionAsync(session.SessionId);
            return FormatResponse(tips, ResponseType.End);
        }

        public async Task<UssdSession> GetOrCreateSessionAsync(string sessionId, string phoneNumber)
        {
            var session = await _context.UssdSessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.IsActive);

            if (session == null)
            {
                session = new UssdSession
                {
                    SessionId = sessionId,
                    PhoneNumber = phoneNumber,
                    CurrentState = UssdMenuState.MainMenu,
                    CreatedAt = DateTime.UtcNow,
                    LastActivityAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.UssdSessions.Add(session);
                await _context.SaveChangesAsync();
            }
            else
            {
                session.LastActivityAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return session;
        }

        public async Task UpdateSessionStateAsync(string sessionId, UssdMenuState newState, string? contextData = null)
        {
            var session = await _context.UssdSessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.IsActive);

            if (session != null)
            {
                session.CurrentState = newState;
                session.LastActivityAt = DateTime.UtcNow;

                if (contextData != null)
                {
                    session.ContextData = contextData;
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task EndSessionAsync(string sessionId)
        {
            var session = await _context.UssdSessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.IsActive);

            if (session != null)
            {
                session.IsActive = false;
                session.CompletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public string GetMainMenu(string languageCode = "en")
        {
            return "Welcome to Disaster Alert\n\n" +
                   "1. Report Disaster\n" +
                   "2. View Alerts\n" +
                   "3. Find Shelter\n" +
                   "4. Emergency Contacts\n" +
                   "5. Safety Tips\n" +
                   "6. Evacuation Info";
        }

        public string GetDisasterTypesMenu(string languageCode = "en")
        {
            return "Select Disaster Type:\n\n" +
                   "1. Flood\n" +
                   "2. Earthquake\n" +
                   "3. Fire\n" +
                   "4. Hurricane\n" +
                   "5. Tornado\n" +
                   "6. Tsunami\n" +
                   "7. Drought\n" +
                   "8. Landslide\n" +
                   "9. Other";
        }

        public async Task<string> GetRecentAlertsAsync(string phoneNumber, string languageCode = "en")
        {
            var alerts = await _context.DisasterAlerts
                .Where(a => a.Status == AlertStatus.Active)
                .OrderByDescending(a => a.IssuedAt)
                .Take(3)
                .ToListAsync();

            if (!alerts.Any())
            {
                return "No active alerts in your area. Stay safe!";
            }

            var sb = new StringBuilder("ACTIVE ALERTS:\n\n");
            foreach (var alert in alerts)
            {
                sb.AppendLine($"- {alert.DisasterType}");
                sb.AppendLine($"  {alert.Title}");
                sb.AppendLine($"  Severity: {alert.Severity}");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public async Task<string> GetNearbySheltersAsync(int? cityId, int? lgaId, string languageCode = "en")
        {
            var query = _context.EmergencyShelters
                .Where(s => s.IsOperational && s.TotalCapacity > s.CurrentOccupancy);

            if (cityId.HasValue)
            {
                query = query.Where(s => s.CityId == cityId.Value);
            }

            if (lgaId.HasValue)
            {
                query = query.Where(s => s.LGAId == lgaId.Value);
            }

            var shelters = await query.Take(5).ToListAsync();

            if (!shelters.Any())
            {
                return "No available shelters found in your area. Call emergency hotline for assistance.";
            }

            var sb = new StringBuilder("AVAILABLE SHELTERS:\n\n");
            foreach (var shelter in shelters)
            {
                var available = shelter.TotalCapacity - shelter.CurrentOccupancy;
                sb.AppendLine($"- {shelter.Name}");
                sb.AppendLine($"  {shelter.Address}");
                sb.AppendLine($"  Available: {available} spaces");
                sb.AppendLine($"  Phone: {shelter.ContactPhone}");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public string GetEmergencyContacts(string languageCode = "en")
        {
            return "EMERGENCY CONTACTS:\n\n" +
                   "Fire: 112\n" +
                   "Police: 911\n" +
                   "Ambulance: 999\n" +
                   "NEMA: 0800-0010-000\n" +
                   "Red Cross: +234-1-2700-600\n\n" +
                   "Stay safe!";
        }

        public async Task<bool> SubmitIncidentReportAsync(string phoneNumber, DisasterType disasterType, string location, string description)
        {
            try
            {
                var incident = new DisasterIncident
                {
                    Id = Guid.NewGuid(),
                    DisasterType = disasterType,
                    Title = $"{disasterType} Report via USSD",
                    Description = description,
                    Status = IncidentStatus.Reported,
                    Severity = IncidentSeverity.Moderate,
                    Address = location,
                    ReporterId = phoneNumber,
                    ReporterPhone = phoneNumber,
                    ReportedAt = DateTime.UtcNow,
                    IsVerified = false,
                    Latitude = 0, // Will be updated by admin
                    Longitude = 0
                };

                _context.DisasterIncidents.Add(incident);
                await _context.SaveChangesAsync();

                _logger.LogInformation("USSD incident report submitted: {DisasterType} in {Location} by {PhoneNumber}", 
                    disasterType, location, phoneNumber);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting USSD incident report");
                return false;
            }
        }

        public async Task<string> GetCitySelectionMenuAsync(string languageCode = "en")
        {
            var cities = await _context.Cities.OrderBy(c => c.Name).Take(10).ToListAsync();

            if (!cities.Any())
            {
                return "No cities available.";
            }

            var sb = new StringBuilder("Select City:\n\n");
            for (int i = 0; i < cities.Count; i++)
            {
                sb.AppendLine($"{i + 1}. {cities[i].Name}");
            }

            return sb.ToString();
        }

        public async Task<string> GetLGASelectionMenuAsync(int cityId, string languageCode = "en")
        {
            var lgas = await _context.LGAs
                .Where(l => l.Cities.Any(c => c.Id == cityId))
                .OrderBy(l => l.LGAName)
                .Take(10)
                .ToListAsync();

            if (!lgas.Any())
            {
                return "No LGAs available for this city.";
            }

            var sb = new StringBuilder("Select LGA:\n\n");
            for (int i = 0; i < lgas.Count; i++)
            {
                sb.AppendLine($"{i + 1}. {lgas[i].LGAName}");
            }

            return sb.ToString();
        }

        public string GetSafetyTips(DisasterType? disasterType = null, string languageCode = "en")
        {
            if (disasterType.HasValue)
            {
                return disasterType.Value switch
                {
                    DisasterType.Flood => "FLOOD SAFETY:\n- Move to higher ground\n- Avoid walking in water\n- Stay away from power lines\n- Monitor weather updates",
                    DisasterType.Earthquake => "EARTHQUAKE SAFETY:\n- Drop, Cover, Hold On\n- Stay away from windows\n- Expect aftershocks\n- Check for injuries",
                    DisasterType.Fire => "FIRE SAFETY:\n- Evacuate immediately\n- Stay low under smoke\n- Don't use elevators\n- Call 112",
                    _ => "GENERAL SAFETY:\n- Stay calm\n- Follow official instructions\n- Have emergency supplies\n- Keep phone charged"
                };
            }

            return "GENERAL SAFETY TIPS:\n\n" +
                   "- Keep emergency contacts handy\n" +
                   "- Have emergency supplies ready\n" +
                   "- Stay informed via radio/TV\n" +
                   "- Follow official instructions\n" +
                   "- Help others when safe to do so";
        }

        public async Task<string> GetEvacuationInfoAsync(int? cityId, int? lgaId, string languageCode = "en")
        {
            var routes = await _context.EvacuationRoutes
                .Where(r => r.Status == RouteStatus.Open && (!cityId.HasValue || r.CityId == cityId.Value))
                .Take(3)
                .ToListAsync();

            if (!routes.Any())
            {
                return "No evacuation routes currently designated. Follow local authority instructions.";
            }

            var sb = new StringBuilder("EVACUATION ROUTES:\n\n");
            foreach (var route in routes)
            {
                sb.AppendLine($"- {route.Name}");
                sb.AppendLine($"  {route.Description}");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string FormatResponse(string message, ResponseType responseType)
        {
            // Africa's Talking USSD format
            // CON = Continue (show menu and wait for input)
            // END = End session (final message)
            var prefix = responseType == ResponseType.Continue ? "CON " : "END ";
            return prefix + message;
        }

        private enum ResponseType
        {
            Continue,
            End
        }
    }
}
