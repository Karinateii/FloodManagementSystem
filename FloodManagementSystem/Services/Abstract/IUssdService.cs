using GlobalDisasterManagement.Models;

namespace GlobalDisasterManagement.Services.Abstract
{
    /// <summary>
    /// Service for handling USSD interactions with feature phones
    /// </summary>
    public interface IUssdService
    {
        /// <summary>
        /// Process USSD request and generate response
        /// </summary>
        Task<string> ProcessUssdRequestAsync(string sessionId, string phoneNumber, string input, UssdRequestType requestType);

        /// <summary>
        /// Get or create USSD session
        /// </summary>
        Task<UssdSession> GetOrCreateSessionAsync(string sessionId, string phoneNumber);

        /// <summary>
        /// Update session state
        /// </summary>
        Task UpdateSessionStateAsync(string sessionId, UssdMenuState newState, string? contextData = null);

        /// <summary>
        /// End USSD session
        /// </summary>
        Task EndSessionAsync(string sessionId);

        /// <summary>
        /// Get main menu text
        /// </summary>
        string GetMainMenu(string languageCode = "en");

        /// <summary>
        /// Get disaster types menu
        /// </summary>
        string GetDisasterTypesMenu(string languageCode = "en");

        /// <summary>
        /// Get recent alerts for phone number
        /// </summary>
        Task<string> GetRecentAlertsAsync(string phoneNumber, string languageCode = "en");

        /// <summary>
        /// Get nearby shelters
        /// </summary>
        Task<string> GetNearbySheltersAsync(int? cityId, int? lgaId, string languageCode = "en");

        /// <summary>
        /// Get emergency contacts
        /// </summary>
        string GetEmergencyContacts(string languageCode = "en");

        /// <summary>
        /// Submit incident report via USSD
        /// </summary>
        Task<bool> SubmitIncidentReportAsync(string phoneNumber, DisasterType disasterType, string location, string description);

        /// <summary>
        /// Get city selection menu
        /// </summary>
        Task<string> GetCitySelectionMenuAsync(string languageCode = "en");

        /// <summary>
        /// Get LGA selection menu for a city
        /// </summary>
        Task<string> GetLGASelectionMenuAsync(int cityId, string languageCode = "en");

        /// <summary>
        /// Get safety tips
        /// </summary>
        string GetSafetyTips(DisasterType? disasterType = null, string languageCode = "en");

        /// <summary>
        /// Get evacuation information
        /// </summary>
        Task<string> GetEvacuationInfoAsync(int? cityId, int? lgaId, string languageCode = "en");
    }
}
