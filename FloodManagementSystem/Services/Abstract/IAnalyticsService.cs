using GlobalDisasterManagement.Models.DTO.Analytics;

namespace GlobalDisasterManagement.Services.Abstract
{
    /// <summary>
    /// Service for generating analytics and reports
    /// </summary>
    public interface IAnalyticsService
    {
        /// <summary>
        /// Get dashboard KPIs and metrics
        /// </summary>
        Task<DashboardKpiDto> GetDashboardKpisAsync(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get disaster impact analysis
        /// </summary>
        Task<DisasterImpactDto> GetDisasterImpactAnalysisAsync(
            string? disasterType = null, 
            DateTime? startDate = null, 
            DateTime? endDate = null);

        /// <summary>
        /// Get emergency response effectiveness metrics
        /// </summary>
        Task<ResponseEffectivenessDto> GetResponseEffectivenessAsync(
            DateTime? startDate = null, 
            DateTime? endDate = null);

        /// <summary>
        /// Get shelter utilization analytics
        /// </summary>
        Task<ShelterAnalyticsDto> GetShelterAnalyticsAsync(
            DateTime? startDate = null, 
            DateTime? endDate = null);

        /// <summary>
        /// Get IoT sensor performance analytics
        /// </summary>
        Task<IoTSensorAnalyticsDto> GetIoTSensorAnalyticsAsync(
            DateTime? startDate = null, 
            DateTime? endDate = null);

        /// <summary>
        /// Get notification delivery analytics
        /// </summary>
        Task<NotificationAnalyticsDto> GetNotificationAnalyticsAsync(
            DateTime? startDate = null, 
            DateTime? endDate = null);

        /// <summary>
        /// Get ML prediction accuracy metrics
        /// </summary>
        Task<PredictionAccuracyDto> GetPredictionAccuracyAsync(
            DateTime? startDate = null, 
            DateTime? endDate = null);

        /// <summary>
        /// Get time series data for trends
        /// </summary>
        Task<List<TimeSeriesDataDto>> GetTimeSeriesDataAsync(
            string metricType, 
            DateTime startDate, 
            DateTime endDate, 
            string? groupBy = "day");

        /// <summary>
        /// Get geographic heat map data
        /// </summary>
        Task<List<GeographicImpactDto>> GetGeographicHeatMapDataAsync(
            string? disasterType = null, 
            DateTime? startDate = null, 
            DateTime? endDate = null);

        /// <summary>
        /// Generate comprehensive report
        /// </summary>
        Task<ReportResultDto> GenerateReportAsync(ReportRequestDto request, string userId);

        /// <summary>
        /// Export data to CSV
        /// </summary>
        Task<byte[]> ExportToCsvAsync(string dataType, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Export data to Excel
        /// </summary>
        Task<byte[]> ExportToExcelAsync(string dataType, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get comparative analysis between periods
        /// </summary>
        Task<Dictionary<string, object>> GetComparativeAnalysisAsync(
            DateTime period1Start, 
            DateTime period1End, 
            DateTime period2Start, 
            DateTime period2End);

        /// <summary>
        /// Get real-time analytics snapshot
        /// </summary>
        Task<Dictionary<string, object>> GetRealTimeSnapshotAsync();
    }
}
