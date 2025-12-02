using GlobalDisasterManagement.Data;
using GlobalDisasterManagement.Models;
using GlobalDisasterManagement.Services.Abstract;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Globalization;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML;
using Twilio.TwiML.Voice;
using Twilio.Types;

namespace GlobalDisasterManagement.Services.Implementation
{
    /// <summary>
    /// Implementation of voice call service using Twilio Voice API
    /// </summary>
    public class VoiceService : IVoiceService
    {
        private readonly DisasterDbContext _context;
        private readonly ILogger<VoiceService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IStringLocalizer<Resources.SharedResources> _localizer;
        private readonly string _twilioAccountSid;
        private readonly string _twilioAuthToken;
        private readonly string _voicePhoneNumber;

        public VoiceService(DisasterDbContext context, ILogger<VoiceService> logger, IConfiguration configuration, IStringLocalizer<Resources.SharedResources> localizer)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _localizer = localizer;

            _twilioAccountSid = _configuration["Twilio:AccountSid"] ?? throw new InvalidOperationException("Twilio AccountSid not configured");
            _twilioAuthToken = _configuration["Twilio:AuthToken"] ?? throw new InvalidOperationException("Twilio AuthToken not configured");
            _voicePhoneNumber = _configuration["Twilio:VoicePhoneNumber"] ?? throw new InvalidOperationException("Twilio VoicePhoneNumber not configured");

            TwilioClient.Init(_twilioAccountSid, _twilioAuthToken);
        }

        public async Task<VoiceCall> MakeOutboundCallAsync(string toPhoneNumber, VoiceCallType callType, string? alertId = null, string? incidentId = null)
        {
            try
            {
                var voiceCall = new VoiceCall
                {
                    ToPhoneNumber = NormalizePhoneNumber(toPhoneNumber),
                    FromPhoneNumber = _voicePhoneNumber,
                    Direction = VoiceCallDirection.Outbound,
                    CallType = callType,
                    Status = VoiceCallStatus.Initiated,
                    DisasterAlertId = string.IsNullOrEmpty(alertId) ? null : Guid.Parse(alertId),
                    DisasterIncidentId = string.IsNullOrEmpty(incidentId) ? null : Guid.Parse(incidentId)
                };

                _context.VoiceCalls.Add(voiceCall);
                await _context.SaveChangesAsync();

                // Build TwiML URL for the call
                var twimlUrl = BuildTwiMLUrl(callType, alertId, incidentId);

                // Create Twilio call
                var call = await CallResource.CreateAsync(
                    to: new PhoneNumber(voiceCall.ToPhoneNumber),
                    from: new PhoneNumber(_voicePhoneNumber),
                    url: new Uri(twimlUrl),
                    statusCallback: new Uri($"{GetBaseUrl()}/api/voicewebhook/status"),
                    statusCallbackEvent: new List<string> { "initiated", "ringing", "answered", "completed" },
                    record: callType == VoiceCallType.IVRMenu // Only record IVR sessions
                );

                voiceCall.CallSid = call.Sid;
                voiceCall.Status = MapTwilioStatus(call.Status);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Initiated voice call {CallSid} to {PhoneNumber} for {CallType}", 
                    call.Sid, toPhoneNumber, callType);

                return voiceCall;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error making outbound call to {PhoneNumber}", toPhoneNumber);
                throw;
            }
        }

        public async Task<VoiceCall> SendDisasterAlertVoiceAsync(string toPhoneNumber, DisasterAlert alert)
        {
            return await MakeOutboundCallAsync(toPhoneNumber, VoiceCallType.DisasterAlert, alert.Id.ToString(), null);
        }

        public async Task<int> SendBulkVoiceAlertsAsync(List<string> phoneNumbers, DisasterAlert alert)
        {
            int successCount = 0;

            foreach (var phoneNumber in phoneNumbers)
            {
                try
                {
                    await SendDisasterAlertVoiceAsync(phoneNumber, alert);
                    successCount++;
                    
                    // Rate limiting - 100ms delay between calls
                    await System.Threading.Tasks.Task.Delay(100);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send voice alert to {PhoneNumber}", phoneNumber);
                }
            }

            _logger.LogInformation("Sent {SuccessCount}/{TotalCount} voice alerts for alert {AlertId}", 
                successCount, phoneNumbers.Count, alert.Id);

            return successCount;
        }

        public async Task<VoiceCall> SendIncidentConfirmationCallAsync(string toPhoneNumber, DisasterIncident incident)
        {
            return await MakeOutboundCallAsync(toPhoneNumber, VoiceCallType.IncidentConfirmation, null, incident.Id.ToString());
        }

        public async Task<VoiceCall> SendShelterInfoCallAsync(string toPhoneNumber, EmergencyShelter shelter)
        {
            var voiceCall = new VoiceCall
            {
                ToPhoneNumber = NormalizePhoneNumber(toPhoneNumber),
                FromPhoneNumber = _voicePhoneNumber,
                Direction = VoiceCallDirection.Outbound,
                CallType = VoiceCallType.ShelterInformation,
                Status = VoiceCallStatus.Initiated,
                Metadata = System.Text.Json.JsonSerializer.Serialize(new { ShelterId = shelter.Id })
            };

            _context.VoiceCalls.Add(voiceCall);
            await _context.SaveChangesAsync();

            var twimlUrl = $"{GetBaseUrl()}/api/voicewebhook/shelter/{shelter.Id}";

            var call = await CallResource.CreateAsync(
                to: new PhoneNumber(voiceCall.ToPhoneNumber),
                from: new PhoneNumber(_voicePhoneNumber),
                url: new Uri(twimlUrl),
                statusCallback: new Uri($"{GetBaseUrl()}/api/voicewebhook/status")
            );

            voiceCall.CallSid = call.Sid;
            voiceCall.Status = MapTwilioStatus(call.Status);
            await _context.SaveChangesAsync();

            return voiceCall;
        }

        public async Task<VoiceCall> SendEvacuationInstructionsCallAsync(string toPhoneNumber, EvacuationRoute route)
        {
            var voiceCall = new VoiceCall
            {
                ToPhoneNumber = NormalizePhoneNumber(toPhoneNumber),
                FromPhoneNumber = _voicePhoneNumber,
                Direction = VoiceCallDirection.Outbound,
                CallType = VoiceCallType.EvacuationInstructions,
                Status = VoiceCallStatus.Initiated,
                Metadata = System.Text.Json.JsonSerializer.Serialize(new { RouteId = route.Id })
            };

            _context.VoiceCalls.Add(voiceCall);
            await _context.SaveChangesAsync();

            var twimlUrl = $"{GetBaseUrl()}/api/voicewebhook/evacuation/{route.Id}";

            var call = await CallResource.CreateAsync(
                to: new PhoneNumber(voiceCall.ToPhoneNumber),
                from: new PhoneNumber(_voicePhoneNumber),
                url: new Uri(twimlUrl),
                statusCallback: new Uri($"{GetBaseUrl()}/api/voicewebhook/status")
            );

            voiceCall.CallSid = call.Sid;
            voiceCall.Status = MapTwilioStatus(call.Status);
            await _context.SaveChangesAsync();

            return voiceCall;
        }

        public async Task<string> ProcessInboundCallAsync(string from, string to, string callSid)
        {
            var voiceCall = new VoiceCall
            {
                ToPhoneNumber = to,
                FromPhoneNumber = NormalizePhoneNumber(from),
                Direction = VoiceCallDirection.Inbound,
                CallType = VoiceCallType.IVRMenu,
                CallSid = callSid,
                Status = VoiceCallStatus.InProgress,
                MenuState = IVRMenuState.MainMenu,
                AnsweredAt = DateTime.UtcNow
            };

            _context.VoiceCalls.Add(voiceCall);
            await _context.SaveChangesAsync();

            return await GenerateIVRMenuTwiMLAsync(IVRMenuState.MainMenu);
        }

        public async System.Threading.Tasks.Task ProcessCallStatusAsync(string callSid, string callStatus, int? callDuration = null, string? errorCode = null, string? errorMessage = null)
        {
            var voiceCall = await _context.VoiceCalls.FirstOrDefaultAsync(c => c.CallSid == callSid);
            
            if (voiceCall == null)
            {
                _logger.LogWarning("Received status update for unknown call {CallSid}", callSid);
                return;
            }

            voiceCall.Status = MapTwilioStatusString(callStatus);
            voiceCall.Duration = callDuration;
            voiceCall.ErrorCode = errorCode;
            voiceCall.ErrorMessage = errorMessage;

            switch (voiceCall.Status)
            {
                case VoiceCallStatus.Ringing:
                    voiceCall.RingingAt = DateTime.UtcNow;
                    break;
                case VoiceCallStatus.InProgress:
                    voiceCall.AnsweredAt = DateTime.UtcNow;
                    break;
                case VoiceCallStatus.Completed:
                    voiceCall.CompletedAt = DateTime.UtcNow;
                    break;
                case VoiceCallStatus.Failed:
                case VoiceCallStatus.Busy:
                case VoiceCallStatus.NoAnswer:
                    voiceCall.FailedAt = DateTime.UtcNow;
                    break;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated call {CallSid} status to {Status}", callSid, voiceCall.Status);
        }

        public async Task<string> ProcessIVRInputAsync(string callSid, string digits, IVRMenuState currentState)
        {
            var voiceCall = await _context.VoiceCalls.FirstOrDefaultAsync(c => c.CallSid == callSid);
            
            if (voiceCall == null)
            {
                return await GenerateIVRMenuTwiMLAsync(IVRMenuState.MainMenu);
            }

            voiceCall.UserInput = (voiceCall.UserInput ?? "") + digits;
            
            // Process menu navigation
            var nextState = ProcessMenuNavigation(currentState, digits);
            voiceCall.MenuState = nextState;
            
            await _context.SaveChangesAsync();

            return await GenerateIVRMenuTwiMLAsync(nextState);
        }

        public async Task<string> GenerateAlertTwiMLAsync(DisasterAlert alert, string? language = "en")
        {
            var response = new VoiceResponse();
            
            var culture = new CultureInfo(language ?? "en");
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            
            // Greeting
            var intro = _localizer["VoiceAlertIntro"].Value;
            response.Say(intro, voice: "alice", language: GetTwilioLanguageCode(language ?? "en"));
            
            response.Pause(length: 1);

            // Alert details
            var affectedAreas = !string.IsNullOrEmpty(alert.AffectedCities) ? alert.AffectedCities : "your area";
            var localizedType = _localizer[alert.DisasterType.ToString()].Value;
            var localizedSeverity = _localizer[alert.Severity.ToString()].Value;
            
            var alertMessage = string.Format(_localizer["VoiceAlertDisaster"].Value, 
                localizedType, affectedAreas, localizedSeverity);
            
            response.Say(alertMessage, voice: "alice", language: GetTwilioLanguageCode(language ?? "en"));
            
            response.Pause(length: 1);
            
            var evacuateMsg = _localizer["VoiceAlertEvacuate"].Value;
            response.Say(evacuateMsg, voice: "alice", language: GetTwilioLanguageCode(language ?? "en"));
            
            response.Pause(length: 2);

            // Repeat option
            var gather = response.Gather(numDigits: 1, action: new Uri($"{GetBaseUrl()}/api/voicewebhook/alert-repeat/{alert.Id}"));
            gather.Say("Press 1 to hear this alert again, or hang up.", voice: "alice", language: GetTwilioLanguageCode(language ?? "en"));

            var staySafe = _localizer["VoiceAlertStaySafe"].Value;
            response.Say(staySafe, voice: "alice", language: GetTwilioLanguageCode(language ?? "en"));
            response.Hangup();

            return response.ToString();
        }

        private string GetTwilioLanguageCode(string languageCode)
        {
            return languageCode switch
            {
                "en" => "en-US",
                "fr" => "fr-FR",
                "es" => "es-ES",
                "pt" => "pt-BR",
                "ar" => "ar",
                _ => "en-US"
            };
        }

        public async Task<string> GenerateIVRMenuTwiMLAsync(IVRMenuState menuState, string? language = "en")
        {
            var response = new VoiceResponse();

            switch (menuState)
            {
                case IVRMenuState.MainMenu:
                    var mainGather = response.Gather(
                        numDigits: 1, 
                        action: new Uri($"{GetBaseUrl()}/api/voicewebhook/ivr-input"),
                        method: Twilio.Http.HttpMethod.Post
                    );
                    mainGather.Say("Welcome to the Global Disaster Management System. " +
                                  "Press 1 for disaster alerts. " +
                                  "Press 2 to find emergency shelters. " +
                                  "Press 3 for evacuation routes. " +
                                  "Press 4 to report an incident. " +
                                  "Press 0 to repeat this menu.", 
                                  voice: "alice", language: "en-US");
                    break;

                case IVRMenuState.DisasterAlerts:
                    var alerts = await _context.DisasterAlerts
                        .Where(a => a.Status == AlertStatus.Active && a.ExpiresAt > DateTime.UtcNow)
                        .OrderByDescending(a => a.IssuedAt)
                        .Take(3)
                        .ToListAsync();

                    if (alerts.Any())
                    {
                        response.Say($"There are {alerts.Count} active disaster alerts.", voice: "alice");
                        response.Pause(length: 1);

                        foreach (var alert in alerts)
                        {
                            var areas = !string.IsNullOrEmpty(alert.AffectedCities) ? alert.AffectedCities : "your area";
                            response.Say($"{alert.DisasterType} alert in {areas}. {alert.Message}", 
                                voice: "alice", language: "en-US");
                            response.Pause(length: 2);
                        }
                    }
                    else
                    {
                        response.Say("There are no active disaster alerts at this time.", voice: "alice");
                    }

                    var alertGather = response.Gather(numDigits: 1, action: new Uri($"{GetBaseUrl()}/api/voicewebhook/ivr-input"));
                    alertGather.Say("Press 9 to return to the main menu.", voice: "alice");
                    break;

                case IVRMenuState.FindShelter:
                    response.Say("To find the nearest emergency shelter, please visit our website or send SHELTER to our WhatsApp number.", 
                        voice: "alice", language: "en-US");
                    
                    var shelters = await _context.EmergencyShelters
                        .Where(s => s.IsActive && s.CurrentOccupancy < s.TotalCapacity)
                        .OrderBy(s => s.Name)
                        .Take(3)
                        .ToListAsync();

                    if (shelters.Any())
                    {
                        response.Say($"Here are {shelters.Count} available shelters:", voice: "alice");
                        foreach (var shelter in shelters)
                        {
                            response.Say($"{shelter.Name} at {shelter.Address}. Capacity: {shelter.TotalCapacity - shelter.CurrentOccupancy} spaces available. Contact: {shelter.ContactPhone}", 
                                voice: "alice", language: "en-US");
                            response.Pause(length: 2);
                        }
                    }

                    var shelterGather = response.Gather(numDigits: 1, action: new Uri($"{GetBaseUrl()}/api/voicewebhook/ivr-input"));
                    shelterGather.Say("Press 9 to return to the main menu.", voice: "alice");
                    break;

                case IVRMenuState.EvacuationRoutes:
                    response.Say("For evacuation route information, please contact emergency services or visit our website.", 
                        voice: "alice", language: "en-US");
                    
                    var routeGather = response.Gather(numDigits: 1, action: new Uri($"{GetBaseUrl()}/api/voicewebhook/ivr-input"));
                    routeGather.Say("Press 9 to return to the main menu.", voice: "alice");
                    break;

                case IVRMenuState.ReportIncident:
                    response.Say("To report a disaster incident, please use our mobile app or website. " +
                                "For immediate emergencies, please contact your local emergency services.", 
                        voice: "alice", language: "en-US");
                    
                    var reportGather = response.Gather(numDigits: 1, action: new Uri($"{GetBaseUrl()}/api/voicewebhook/ivr-input"));
                    reportGather.Say("Press 9 to return to the main menu.", voice: "alice");
                    break;

                default:
                    response.Say("Invalid option. Returning to main menu.", voice: "alice");
                    return await GenerateIVRMenuTwiMLAsync(IVRMenuState.MainMenu);
            }

            response.Say("Thank you for using the Global Disaster Management System. Stay safe.", voice: "alice");
            response.Hangup();

            return response.ToString();
        }

        public async Task<string> GenerateIncidentConfirmationTwiMLAsync(DisasterIncident incident)
        {
            var response = new VoiceResponse();

            response.Say($"Thank you for reporting a {incident.DisasterType} incident.", voice: "alice", language: "en-US");
            response.Pause(length: 1);
            response.Say($"Your report has been received and is being reviewed by emergency responders. " +
                        $"Your incident reference number is {incident.Id}. " +
                        $"If this is an immediate life-threatening emergency, please contact emergency services directly.", 
                        voice: "alice", language: "en-US");
            response.Say("Thank you. Stay safe.", voice: "alice");
            response.Hangup();

            return response.ToString();
        }

        public async Task<string> GenerateShelterInfoTwiMLAsync(EmergencyShelter shelter)
        {
            var response = new VoiceResponse();

            response.Say($"Emergency shelter information for {shelter.Name}.", voice: "alice", language: "en-US");
            response.Pause(length: 1);
            response.Say($"Location: {shelter.Address}. " +
                        $"Capacity: {shelter.TotalCapacity} people. " +
                        $"Currently available spaces: {shelter.TotalCapacity - shelter.CurrentOccupancy}. " +
                        $"Contact number: {shelter.ContactPhone}. " +
                        $"Facilities available: {(shelter.HasFood ? "Food" : "")} {(shelter.HasWater ? "Water" : "")} {(shelter.HasMedicalFacility ? "Medical care" : "")}.", 
                        voice: "alice", language: "en-US");
            response.Say("Please proceed to this shelter if you need emergency accommodation.", voice: "alice");
            response.Hangup();

            return response.ToString();
        }

        public async Task<Dictionary<string, int>> GetCallStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.UtcNow.AddDays(-30);
            endDate ??= DateTime.UtcNow;

            var calls = await _context.VoiceCalls
                .Where(c => c.CreatedAt >= startDate && c.CreatedAt <= endDate)
                .ToListAsync();

            return new Dictionary<string, int>
            {
                { "Total", calls.Count },
                { "Completed", calls.Count(c => c.Status == VoiceCallStatus.Completed) },
                { "Failed", calls.Count(c => c.Status == VoiceCallStatus.Failed) },
                { "NoAnswer", calls.Count(c => c.Status == VoiceCallStatus.NoAnswer) },
                { "Busy", calls.Count(c => c.Status == VoiceCallStatus.Busy) },
                { "InProgress", calls.Count(c => c.Status == VoiceCallStatus.InProgress) },
                { "Outbound", calls.Count(c => c.Direction == VoiceCallDirection.Outbound) },
                { "Inbound", calls.Count(c => c.Direction == VoiceCallDirection.Inbound) }
            };
        }

        public async Task<int> RetryFailedCallsAsync(int maxRetries = 3)
        {
            var failedCalls = await _context.VoiceCalls
                .Where(c => (c.Status == VoiceCallStatus.Failed || c.Status == VoiceCallStatus.NoAnswer || c.Status == VoiceCallStatus.Busy) &&
                           c.RetryCount < maxRetries &&
                           (c.NextRetryAt == null || c.NextRetryAt <= DateTime.UtcNow))
                .ToListAsync();

            int successCount = 0;

            foreach (var call in failedCalls)
            {
                try
                {
                    call.RetryCount++;
                    call.NextRetryAt = DateTime.UtcNow.AddMinutes(Math.Pow(2, call.RetryCount)); // Exponential backoff

                    await MakeOutboundCallAsync(
                        call.ToPhoneNumber, 
                        call.CallType, 
                        call.DisasterAlertId?.ToString(), 
                        call.DisasterIncidentId?.ToString()
                    );

                    successCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to retry call {CallId}", call.Id);
                }

                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("Retried {SuccessCount}/{TotalCount} failed calls", successCount, failedCalls.Count);
            return successCount;
        }

        public async Task<List<VoiceCall>> GetCallHistoryAsync(string phoneNumber, int limit = 50)
        {
            return await _context.VoiceCalls
                .Where(c => c.ToPhoneNumber == phoneNumber || c.FromPhoneNumber == phoneNumber)
                .OrderByDescending(c => c.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        // Helper methods
        private string NormalizePhoneNumber(string phoneNumber)
        {
            // Remove all non-digit characters except +
            var cleaned = new string(phoneNumber.Where(c => char.IsDigit(c) || c == '+').ToArray());
            
            // Add + if not present
            if (!cleaned.StartsWith("+"))
            {
                cleaned = "+" + cleaned;
            }

            return cleaned;
        }

        private string BuildTwiMLUrl(VoiceCallType callType, string? alertId, string? incidentId)
        {
            var baseUrl = GetBaseUrl();

            return callType switch
            {
                VoiceCallType.DisasterAlert => $"{baseUrl}/api/voicewebhook/alert/{alertId}",
                VoiceCallType.IncidentConfirmation => $"{baseUrl}/api/voicewebhook/incident/{incidentId}",
                VoiceCallType.IVRMenu => $"{baseUrl}/api/voicewebhook/ivr-menu",
                _ => $"{baseUrl}/api/voicewebhook/default"
            };
        }

        private string GetBaseUrl()
        {
            return _configuration["AppSettings:BaseUrl"] ?? "https://yourdomain.com";
        }

        private VoiceCallStatus MapTwilioStatus(CallResource.StatusEnum status)
        {
            if (status == CallResource.StatusEnum.Queued)
                return VoiceCallStatus.Queued;
            else if (status == CallResource.StatusEnum.Ringing)
                return VoiceCallStatus.Ringing;
            else if (status == CallResource.StatusEnum.InProgress)
                return VoiceCallStatus.InProgress;
            else if (status == CallResource.StatusEnum.Completed)
                return VoiceCallStatus.Completed;
            else if (status == CallResource.StatusEnum.Failed)
                return VoiceCallStatus.Failed;
            else if (status == CallResource.StatusEnum.Busy)
                return VoiceCallStatus.Busy;
            else if (status == CallResource.StatusEnum.NoAnswer)
                return VoiceCallStatus.NoAnswer;
            else if (status == CallResource.StatusEnum.Canceled)
                return VoiceCallStatus.Canceled;
            else
                return VoiceCallStatus.Initiated;
        }

        private VoiceCallStatus MapTwilioStatusString(string status)
        {
            return status.ToLower() switch
            {
                "queued" => VoiceCallStatus.Queued,
                "ringing" => VoiceCallStatus.Ringing,
                "in-progress" => VoiceCallStatus.InProgress,
                "completed" => VoiceCallStatus.Completed,
                "failed" => VoiceCallStatus.Failed,
                "busy" => VoiceCallStatus.Busy,
                "no-answer" => VoiceCallStatus.NoAnswer,
                "canceled" => VoiceCallStatus.Canceled,
                _ => VoiceCallStatus.Initiated
            };
        }

        private IVRMenuState ProcessMenuNavigation(IVRMenuState currentState, string digits)
        {
            if (currentState == IVRMenuState.MainMenu)
            {
                return digits switch
                {
                    "1" => IVRMenuState.DisasterAlerts,
                    "2" => IVRMenuState.FindShelter,
                    "3" => IVRMenuState.EvacuationRoutes,
                    "4" => IVRMenuState.ReportIncident,
                    "0" => IVRMenuState.MainMenu,
                    _ => IVRMenuState.MainMenu
                };
            }

            // Return to main menu on 9
            if (digits == "9")
            {
                return IVRMenuState.MainMenu;
            }

            return currentState;
        }
    }
}
