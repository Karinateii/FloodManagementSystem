using GlobalDisasterManagement.Models;
using GlobalDisasterManagement.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GlobalDisasterManagement.Controllers
{
    [Authorize]
    public class IoTMonitoringController : Controller
    {
        private readonly IIoTSensorService _sensorService;
        private readonly ILogger<IoTMonitoringController> _logger;

        public IoTMonitoringController(
            IIoTSensorService sensorService,
            ILogger<IoTMonitoringController> logger)
        {
            _sensorService = sensorService;
            _logger = logger;
        }

        /// <summary>
        /// Main IoT monitoring dashboard
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var sensors = await _sensorService.GetActiveSensorsAsync();
                var healthReport = await _sensorService.GetSensorHealthReportAsync();
                var criticalSensors = await _sensorService.GetCriticalWaterLevelSensorsAsync();

                ViewBag.HealthReport = healthReport;
                ViewBag.CriticalSensors = criticalSensors;

                return View(sensors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading IoT monitoring dashboard");
                return View("Error");
            }
        }

        /// <summary>
        /// Water level sensors monitoring
        /// </summary>
        public async Task<IActionResult> WaterLevel()
        {
            try
            {
                var sensors = await _sensorService.GetActiveSensorsAsync();
                var waterLevelSensors = sensors.OfType<WaterLevelSensor>().ToList();
                return View(waterLevelSensors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading water level sensors");
                return View("Error");
            }
        }

        /// <summary>
        /// Rainfall sensors monitoring
        /// </summary>
        public async Task<IActionResult> Rainfall()
        {
            try
            {
                var sensors = await _sensorService.GetActiveSensorsAsync();
                var rainfallSensors = sensors.OfType<RainfallSensor>().ToList();
                return View(rainfallSensors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading rainfall sensors");
                return View("Error");
            }
        }

        /// <summary>
        /// Weather sensors monitoring
        /// </summary>
        public async Task<IActionResult> Weather()
        {
            try
            {
                var sensors = await _sensorService.GetActiveSensorsAsync();
                var weatherSensors = sensors.OfType<WeatherSensor>().ToList();
                return View(weatherSensors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading weather sensors");
                return View("Error");
            }
        }

        /// <summary>
        /// Sensor details and history
        /// </summary>
        public async Task<IActionResult> SensorDetails(Guid id)
        {
            try
            {
                var sensors = await _sensorService.GetActiveSensorsAsync();
                var sensor = sensors.FirstOrDefault(s => s.Id == id);
                if (sensor == null)
                    return NotFound();

                return View(sensor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading sensor details for {SensorId}", id);
                return View("Error");
            }
        }

        /// <summary>
        /// Health monitoring dashboard
        /// </summary>
        public async Task<IActionResult> Health()
        {
            try
            {
                var healthReport = await _sensorService.GetSensorHealthReportAsync();
                return View(healthReport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading health dashboard");
                return View("Error");
            }
        }

        /// <summary>
        /// Analytics dashboard
        /// </summary>
        public async Task<IActionResult> Analytics()
        {
            try
            {
                var statistics = await _sensorService.GetSensorStatisticsAsync();
                return View(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading analytics dashboard");
                return View("Error");
            }
        }
    }
}
