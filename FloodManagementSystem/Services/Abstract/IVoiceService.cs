using GlobalDisasterManagement.Models;

namespace GlobalDisasterManagement.Services.Abstract
{
    /// <summary>
    /// Service interface for voice call operations using Twilio Voice API
    /// </summary>
    public interface IVoiceService
    {
        /// <summary>
        /// Make an outbound voice call
        /// </summary>
        Task<VoiceCall> MakeOutboundCallAsync(string toPhoneNumber, VoiceCallType callType, string? alertId = null, string? incidentId = null);

        /// <summary>
        /// Send disaster alert as voice call
        /// </summary>
        Task<VoiceCall> SendDisasterAlertVoiceAsync(string toPhoneNumber, DisasterAlert alert);

        /// <summary>
        /// Send bulk voice alerts to multiple recipients
        /// </summary>
        Task<int> SendBulkVoiceAlertsAsync(List<string> phoneNumbers, DisasterAlert alert);

        /// <summary>
        /// Send incident confirmation call
        /// </summary>
        Task<VoiceCall> SendIncidentConfirmationCallAsync(string toPhoneNumber, DisasterIncident incident);

        /// <summary>
        /// Send shelter information call
        /// </summary>
        Task<VoiceCall> SendShelterInfoCallAsync(string toPhoneNumber, EmergencyShelter shelter);

        /// <summary>
        /// Send evacuation instructions call
        /// </summary>
        Task<VoiceCall> SendEvacuationInstructionsCallAsync(string toPhoneNumber, EvacuationRoute route);

        /// <summary>
        /// Process inbound call webhook
        /// </summary>
        Task<string> ProcessInboundCallAsync(string from, string to, string callSid);

        /// <summary>
        /// Process call status update webhook
        /// </summary>
        Task ProcessCallStatusAsync(string callSid, string callStatus, int? callDuration = null, string? errorCode = null, string? errorMessage = null);

        /// <summary>
        /// Process IVR DTMF input
        /// </summary>
        Task<string> ProcessIVRInputAsync(string callSid, string digits, IVRMenuState currentState);

        /// <summary>
        /// Generate TwiML for disaster alert
        /// </summary>
        Task<string> GenerateAlertTwiMLAsync(DisasterAlert alert, string? language = "en");

        /// <summary>
        /// Generate TwiML for IVR main menu
        /// </summary>
        Task<string> GenerateIVRMenuTwiMLAsync(IVRMenuState menuState, string? language = "en");

        /// <summary>
        /// Generate TwiML for incident confirmation
        /// </summary>
        Task<string> GenerateIncidentConfirmationTwiMLAsync(DisasterIncident incident);

        /// <summary>
        /// Generate TwiML for shelter information
        /// </summary>
        Task<string> GenerateShelterInfoTwiMLAsync(EmergencyShelter shelter);

        /// <summary>
        /// Get call statistics
        /// </summary>
        Task<Dictionary<string, int>> GetCallStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Retry failed calls
        /// </summary>
        Task<int> RetryFailedCallsAsync(int maxRetries = 3);

        /// <summary>
        /// Get call history for a phone number
        /// </summary>
        Task<List<VoiceCall>> GetCallHistoryAsync(string phoneNumber, int limit = 50);
    }
}
