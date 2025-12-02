using GlobalDisasterManagement.Models;

namespace GlobalDisasterManagement.Services.Interfaces
{
    public interface IIncidentService
    {
        Task<DisasterIncident> CreateIncidentAsync(DisasterIncident incident);
        Task<DisasterIncident?> GetIncidentByIdAsync(Guid id);
        Task<IEnumerable<DisasterIncident>> GetActiveIncidentsAsync();
        Task<IEnumerable<DisasterIncident>> GetIncidentsByDisasterTypeAsync(DisasterType disasterType);
        Task<IEnumerable<DisasterIncident>> GetIncidentsByCityAsync(int cityId);
        Task<IEnumerable<DisasterIncident>> GetIncidentsByLGAAsync(int lgaId);
        Task<IEnumerable<DisasterIncident>> GetIncidentsBySeverityAsync(IncidentSeverity severity);
        Task<IEnumerable<DisasterIncident>> GetRecentIncidentsAsync(int count = 20);
        Task<DisasterIncident> UpdateIncidentAsync(DisasterIncident incident);
        Task<bool> UpdateIncidentStatusAsync(Guid id, IncidentStatus status, string? notes = null);
        Task<bool> VerifyIncidentAsync(Guid id, string verifiedBy);
        Task<bool> AssignRespondersAsync(Guid id, List<string> responderIds);
        Task<IEnumerable<DisasterIncident>> GetIncidentsByLocationAsync(double latitude, double longitude, double radiusKm);
        Task<int> GetActiveIncidentCountAsync();
        Task<Dictionary<IncidentSeverity, int>> GetIncidentStatisticsBySeverityAsync();
        Task<Dictionary<DisasterType, int>> GetIncidentStatisticsByTypeAsync();
    }
}
