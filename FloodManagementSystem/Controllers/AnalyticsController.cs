using GlobalDisasterManagement.Models.DTO.Analytics;
using GlobalDisasterManagement.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GlobalDisasterManagement.Controllers
{
    /// <summary>
    /// Analytics and reporting controller
    /// </summary>
    [Authorize]
    public class AnalyticsController : Controller
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ILogger<AnalyticsController> _logger;

        public AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger)
        {
            _analyticsService = analyticsService;
            _logger = logger;
        }

        /// <summary>
        /// Analytics dashboard main page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var kpis = await _analyticsService.GetDashboardKpisAsync();
                return View(kpis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading analytics dashboard");
                TempData["Error"] = "Failed to load analytics dashboard.";
                return View(new DashboardKpiDto());
            }
        }

        /// <summary>
        /// Disaster impact analysis page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DisasterImpact(string? disasterType = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var impact = await _analyticsService.GetDisasterImpactAnalysisAsync(disasterType, startDate, endDate);
                ViewBag.DisasterType = disasterType;
                ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
                ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
                return View(impact);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading disaster impact analysis");
                TempData["Error"] = "Failed to load disaster impact analysis.";
                return View(new DisasterImpactDto());
            }
        }

        /// <summary>
        /// Response effectiveness metrics page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ResponseEffectiveness(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var effectiveness = await _analyticsService.GetResponseEffectivenessAsync(startDate, endDate);
                ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
                ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
                return View(effectiveness);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading response effectiveness");
                TempData["Error"] = "Failed to load response effectiveness metrics.";
                return View(new ResponseEffectivenessDto());
            }
        }

        /// <summary>
        /// Shelter analytics page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ShelterAnalytics(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var analytics = await _analyticsService.GetShelterAnalyticsAsync(startDate, endDate);
                ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
                ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
                return View(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading shelter analytics");
                TempData["Error"] = "Failed to load shelter analytics.";
                return View(new ShelterAnalyticsDto());
            }
        }

        /// <summary>
        /// IoT sensor performance page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> IoTSensorPerformance(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var analytics = await _analyticsService.GetIoTSensorAnalyticsAsync(startDate, endDate);
                ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
                ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
                return View(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading IoT sensor performance");
                TempData["Error"] = "Failed to load IoT sensor performance metrics.";
                return View(new IoTSensorAnalyticsDto());
            }
        }

        /// <summary>
        /// Notification delivery analytics page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> NotificationAnalytics(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var analytics = await _analyticsService.GetNotificationAnalyticsAsync(startDate, endDate);
                ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
                ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
                return View(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading notification analytics");
                TempData["Error"] = "Failed to load notification analytics.";
                return View(new NotificationAnalyticsDto());
            }
        }

        /// <summary>
        /// ML prediction accuracy page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> PredictionAccuracy(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var accuracy = await _analyticsService.GetPredictionAccuracyAsync(startDate, endDate);
                ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
                ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
                return View(accuracy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading prediction accuracy");
                TempData["Error"] = "Failed to load prediction accuracy metrics.";
                return View(new PredictionAccuracyDto());
            }
        }

        /// <summary>
        /// Reports generation page
        /// </summary>
        [HttpGet]
        public IActionResult Reports()
        {
            return View();
        }

        /// <summary>
        /// Generate report
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateReport(ReportRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Invalid report request.";
                    return RedirectToAction(nameof(Reports));
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "System";
                var result = await _analyticsService.GenerateReportAsync(request, userId);

                TempData["Success"] = $"Report generated successfully: {result.FileName}";
                
                // In a real implementation, you would return the file for download
                // return File(fileBytes, "application/pdf", result.FileName);
                
                return RedirectToAction(nameof(Reports));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report");
                TempData["Error"] = "Failed to generate report.";
                return RedirectToAction(nameof(Reports));
            }
        }

        /// <summary>
        /// Export data to CSV
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExportCsv(string dataType, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                startDate ??= DateTime.UtcNow.AddMonths(-1);
                endDate ??= DateTime.UtcNow;

                var csvData = await _analyticsService.ExportToCsvAsync(dataType, startDate.Value, endDate.Value);
                var fileName = $"{dataType}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";

                return File(csvData, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting to CSV");
                TempData["Error"] = "Failed to export data to CSV.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Export data to Excel
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExportExcel(string dataType, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                startDate ??= DateTime.UtcNow.AddMonths(-1);
                endDate ??= DateTime.UtcNow;

                var excelData = await _analyticsService.ExportToExcelAsync(dataType, startDate.Value, endDate.Value);
                var fileName = $"{dataType}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";

                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting to Excel");
                TempData["Error"] = "Failed to export data to Excel.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Comparative analysis page
        /// </summary>
        [HttpGet]
        public IActionResult ComparativeAnalysis()
        {
            return View();
        }

        /// <summary>
        /// Get comparative analysis data
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GetComparativeAnalysis(
            DateTime period1Start, 
            DateTime period1End, 
            DateTime period2Start, 
            DateTime period2End)
        {
            try
            {
                var comparison = await _analyticsService.GetComparativeAnalysisAsync(
                    period1Start, period1End, period2Start, period2End);

                return Json(comparison);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating comparative analysis");
                return Json(new { error = "Failed to generate comparative analysis." });
            }
        }

        /// <summary>
        /// Get real-time snapshot (for dashboard widgets)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRealTimeSnapshot()
        {
            try
            {
                var snapshot = await _analyticsService.GetRealTimeSnapshotAsync();
                return Json(snapshot);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching real-time snapshot");
                return Json(new { error = "Failed to fetch real-time data." });
            }
        }

        /// <summary>
        /// Get time series data for charts
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTimeSeriesData(
            string metricType, 
            DateTime? startDate = null, 
            DateTime? endDate = null, 
            string? groupBy = "day")
        {
            try
            {
                startDate ??= DateTime.UtcNow.AddMonths(-1);
                endDate ??= DateTime.UtcNow;

                var data = await _analyticsService.GetTimeSeriesDataAsync(
                    metricType, startDate.Value, endDate.Value, groupBy);

                return Json(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching time series data");
                return Json(new { error = "Failed to fetch time series data." });
            }
        }

        /// <summary>
        /// Get geographic heat map data
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetHeatMapData(
            string? disasterType = null, 
            DateTime? startDate = null, 
            DateTime? endDate = null)
        {
            try
            {
                var data = await _analyticsService.GetGeographicHeatMapDataAsync(disasterType, startDate, endDate);
                return Json(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching heat map data");
                return Json(new { error = "Failed to fetch heat map data." });
            }
        }

        /// <summary>
        /// Heat map visualization page
        /// </summary>
        [HttpGet]
        public IActionResult HeatMap()
        {
            return View();
        }
    }
}
