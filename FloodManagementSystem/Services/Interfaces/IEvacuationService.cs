using GlobalDisasterManagement.Models;

namespace GlobalDisasterManagement.Services.Interfaces
{
    public interface IEvacuationService
    {
        Task<EvacuationRoute> CreateRouteAsync(EvacuationRoute route);
        Task<EvacuationRoute?> GetRouteByIdAsync(Guid id);
        Task<IEnumerable<EvacuationRoute>> GetActiveRoutesAsync();
        Task<IEnumerable<EvacuationRoute>> GetRoutesByCityAsync(int cityId);
        Task<IEnumerable<EvacuationRoute>> GetRoutesFromLocationAsync(double latitude, double longitude);
        Task<EvacuationRoute?> GetOptimalRouteAsync(double startLat, double startLng, double endLat, double endLng);
        Task<EvacuationRoute> UpdateRouteAsync(EvacuationRoute route);
        Task<bool> UpdateRouteStatusAsync(Guid id, RouteStatus status);
        Task<IEnumerable<EvacuationRoute>> GetSafeRoutesAsync();
        Task<EmergencyContact> CreateEmergencyContactAsync(EmergencyContact contact);
        Task<IEnumerable<EmergencyContact>> GetEmergencyContactsAsync(string? countryCode = null);
        Task<IEnumerable<EmergencyContact>> GetEmergencyContactsByTypeAsync(EmergencyServiceType type);
        Task<EmergencyContact?> GetPrimaryEmergencyContactAsync(EmergencyServiceType type);
    }
}
