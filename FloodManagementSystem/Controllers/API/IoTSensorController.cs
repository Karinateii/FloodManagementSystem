using GlobalDisasterManagement.Models;
using GlobalDisasterManagement.Services.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace GlobalDisasterManagement.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class IoTSensorController : ControllerBase
    {
        private readonly IIoTSensorService _sensorService;
        private readonly ILogger<IoTSensorController> _logger;

        public IoTSensorController(
            IIoTSensorService sensorService,
            ILogger<IoTSensorController> logger)
        {
            _sensorService = sensorService;
            _logger = logger;
        }

        #region Sensor Registration

        /// <summary>
        /// Register a new water level sensor
        /// </summary>
        [HttpPost("water-level/register")]
        public async Task<ActionResult<WaterLevelSensor>> RegisterWaterLevelSensor([FromBody] WaterLevelSensor sensor)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var registeredSensor = await _sensorService.RegisterWaterLevelSensorAsync(sensor);
                return CreatedAtAction(nameof(GetSensorByDeviceId), 
                    new { deviceId = registeredSensor.DeviceId }, 
                    registeredSensor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering water level sensor");
                return StatusCode(500, new { error = "Failed to register sensor", details = ex.Message });
            }
        }

        /// <summary>
        /// Register a new rainfall sensor
        /// </summary>
        [HttpPost("rainfall/register")]
        public async Task<ActionResult<RainfallSensor>> RegisterRainfallSensor([FromBody] RainfallSensor sensor)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var registeredSensor = await _sensorService.RegisterRainfallSensorAsync(sensor);
                return CreatedAtAction(nameof(GetSensorByDeviceId), 
                    new { deviceId = registeredSensor.DeviceId }, 
                    registeredSensor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering rainfall sensor");
                return StatusCode(500, new { error = "Failed to register sensor", details = ex.Message });
            }
        }

        /// <summary>
        /// Register a new weather sensor
        /// </summary>
        [HttpPost("weather/register")]
        public async Task<ActionResult<WeatherSensor>> RegisterWeatherSensor([FromBody] WeatherSensor sensor)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var registeredSensor = await _sensorService.RegisterWeatherSensorAsync(sensor);
                return CreatedAtAction(nameof(GetSensorByDeviceId), 
                    new { deviceId = registeredSensor.DeviceId }, 
                    registeredSensor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering weather sensor");
                return StatusCode(500, new { error = "Failed to register sensor", details = ex.Message });
            }
        }

        #endregion

        #region Data Ingestion

        /// <summary>
        /// Record water level reading from sensor
        /// </summary>
        [HttpPost("{deviceId}/water-level")]
        public async Task<ActionResult<WaterLevelReading>> RecordWaterLevel(
            string deviceId, 
            [FromBody] WaterLevelDataDto data)
        {
            try
            {
                var reading = await _sensorService.RecordWaterLevelAsync(deviceId, data.Level);
                return Ok(reading);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording water level for device {DeviceId}", deviceId);
                return StatusCode(500, new { error = "Failed to record water level", details = ex.Message });
            }
        }

        /// <summary>
        /// Record rainfall reading from sensor
        /// </summary>
        [HttpPost("{deviceId}/rainfall")]
        public async Task<ActionResult<RainfallReading>> RecordRainfall(
            string deviceId, 
            [FromBody] RainfallDataDto data)
        {
            try
            {
                var reading = await _sensorService.RecordRainfallAsync(
                    deviceId, 
                    data.Rainfall, 
                    data.PeriodStart, 
                    data.PeriodEnd);
                return Ok(reading);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording rainfall for device {DeviceId}", deviceId);
                return StatusCode(500, new { error = "Failed to record rainfall", details = ex.Message });
            }
        }

        /// <summary>
        /// Record weather data from sensor
        /// </summary>
        [HttpPost("{deviceId}/weather")]
        public async Task<ActionResult<WeatherReading>> RecordWeather(
            string deviceId, 
            [FromBody] WeatherReading reading)
        {
            try
            {
                var recordedReading = await _sensorService.RecordWeatherDataAsync(deviceId, reading);
                return Ok(recordedReading);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording weather data for device {DeviceId}", deviceId);
                return StatusCode(500, new { error = "Failed to record weather data", details = ex.Message });
            }
        }

        #endregion

        #region Sensor Management

        /// <summary>
        /// Get sensor information by device ID
        /// </summary>
        [HttpGet("device/{deviceId}")]
        public async Task<ActionResult<IoTSensor>> GetSensorByDeviceId(string deviceId)
        {
            try
            {
                var sensor = await _sensorService.GetSensorByDeviceIdAsync(deviceId);
                if (sensor == null)
                    return NotFound(new { error = $"Sensor with device ID {deviceId} not found" });

                return Ok(sensor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sensor {DeviceId}", deviceId);
                return StatusCode(500, new { error = "Failed to retrieve sensor", details = ex.Message });
            }
        }

        /// <summary>
        /// Get all active sensors
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<List<IoTSensor>>> GetActiveSensors()
        {
            try
            {
                var sensors = await _sensorService.GetActiveSensorsAsync();
                return Ok(sensors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active sensors");
                return StatusCode(500, new { error = "Failed to retrieve sensors", details = ex.Message });
            }
        }

        /// <summary>
        /// Get all sensors for a specific city
        /// </summary>
        [HttpGet("city/{cityId}")]
        public async Task<ActionResult<List<IoTSensor>>> GetSensorsByCity(int cityId)
        {
            try
            {
                var sensors = await _sensorService.GetSensorsByCityAsync(cityId);
                return Ok(sensors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sensors for city {CityId}", cityId);
                return StatusCode(500, new { error = "Failed to retrieve sensors", details = ex.Message });
            }
        }

        /// <summary>
        /// Update sensor status
        /// </summary>
        [HttpPut("{sensorId}/status")]
        public async Task<ActionResult> UpdateSensorStatus(Guid sensorId, [FromBody] SensorStatusDto statusDto)
        {
            try
            {
                var result = await _sensorService.UpdateSensorStatusAsync(sensorId, statusDto.Status);
                if (!result)
                    return NotFound(new { error = $"Sensor {sensorId} not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating sensor status for {SensorId}", sensorId);
                return StatusCode(500, new { error = "Failed to update sensor status", details = ex.Message });
            }
        }

        /// <summary>
        /// Deactivate sensor
        /// </summary>
        [HttpPost("{sensorId}/deactivate")]
        public async Task<ActionResult> DeactivateSensor(Guid sensorId)
        {
            try
            {
                var result = await _sensorService.DeactivateSensorAsync(sensorId);
                if (!result)
                    return NotFound(new { error = $"Sensor {sensorId} not found" });

                return Ok(new { message = "Sensor deactivated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating sensor {SensorId}", sensorId);
                return StatusCode(500, new { error = "Failed to deactivate sensor", details = ex.Message });
            }
        }

        #endregion

        #region Data Retrieval

        /// <summary>
        /// Get water level history for a sensor
        /// </summary>
        [HttpGet("{sensorId}/water-level/history")]
        public async Task<ActionResult<List<WaterLevelReading>>> GetWaterLevelHistory(
            Guid sensorId, 
            [FromQuery] DateTime from, 
            [FromQuery] DateTime to)
        {
            try
            {
                var readings = await _sensorService.GetWaterLevelHistoryAsync(sensorId, from, to);
                return Ok(readings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving water level history for sensor {SensorId}", sensorId);
                return StatusCode(500, new { error = "Failed to retrieve water level history", details = ex.Message });
            }
        }

        /// <summary>
        /// Get latest water level reading
        /// </summary>
        [HttpGet("{sensorId}/water-level/latest")]
        public async Task<ActionResult<WaterLevelReading>> GetLatestWaterLevel(Guid sensorId)
        {
            try
            {
                var reading = await _sensorService.GetLatestWaterLevelAsync(sensorId);
                if (reading == null)
                    return NotFound(new { error = $"No water level readings found for sensor {sensorId}" });

                return Ok(reading);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving latest water level for sensor {SensorId}", sensorId);
                return StatusCode(500, new { error = "Failed to retrieve water level", details = ex.Message });
            }
        }

        /// <summary>
        /// Get rainfall history for a sensor
        /// </summary>
        [HttpGet("{sensorId}/rainfall/history")]
        public async Task<ActionResult<List<RainfallReading>>> GetRainfallHistory(
            Guid sensorId, 
            [FromQuery] DateTime from, 
            [FromQuery] DateTime to)
        {
            try
            {
                var readings = await _sensorService.GetRainfallHistoryAsync(sensorId, from, to);
                return Ok(readings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving rainfall history for sensor {SensorId}", sensorId);
                return StatusCode(500, new { error = "Failed to retrieve rainfall history", details = ex.Message });
            }
        }

        /// <summary>
        /// Get latest rainfall reading
        /// </summary>
        [HttpGet("{sensorId}/rainfall/latest")]
        public async Task<ActionResult<RainfallReading>> GetLatestRainfall(Guid sensorId)
        {
            try
            {
                var reading = await _sensorService.GetLatestRainfallAsync(sensorId);
                if (reading == null)
                    return NotFound(new { error = $"No rainfall readings found for sensor {sensorId}" });

                return Ok(reading);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving latest rainfall for sensor {SensorId}", sensorId);
                return StatusCode(500, new { error = "Failed to retrieve rainfall", details = ex.Message });
            }
        }

        /// <summary>
        /// Get weather history for a sensor
        /// </summary>
        [HttpGet("{sensorId}/weather/history")]
        public async Task<ActionResult<List<WeatherReading>>> GetWeatherHistory(
            Guid sensorId, 
            [FromQuery] DateTime from, 
            [FromQuery] DateTime to)
        {
            try
            {
                var readings = await _sensorService.GetWeatherHistoryAsync(sensorId, from, to);
                return Ok(readings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving weather history for sensor {SensorId}", sensorId);
                return StatusCode(500, new { error = "Failed to retrieve weather history", details = ex.Message });
            }
        }

        /// <summary>
        /// Get latest weather reading
        /// </summary>
        [HttpGet("{sensorId}/weather/latest")]
        public async Task<ActionResult<WeatherReading>> GetLatestWeather(Guid sensorId)
        {
            try
            {
                var reading = await _sensorService.GetLatestWeatherAsync(sensorId);
                if (reading == null)
                    return NotFound(new { error = $"No weather readings found for sensor {sensorId}" });

                return Ok(reading);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving latest weather for sensor {SensorId}", sensorId);
                return StatusCode(500, new { error = "Failed to retrieve weather", details = ex.Message });
            }
        }

        #endregion

        #region Analytics

        /// <summary>
        /// Get sensor health report
        /// </summary>
        [HttpGet("health")]
        public async Task<ActionResult> GetSensorHealthReport()
        {
            try
            {
                var report = await _sensorService.GetSensorHealthReportAsync();
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sensor health report");
                return StatusCode(500, new { error = "Failed to retrieve health report", details = ex.Message });
            }
        }

        /// <summary>
        /// Get sensor statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult> GetStatistics()
        {
            try
            {
                var statistics = await _sensorService.GetSensorStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sensor statistics");
                return StatusCode(500, new { error = "Failed to retrieve statistics", details = ex.Message });
            }
        }

        /// <summary>
        /// Get total rainfall for a city
        /// </summary>
        [HttpGet("city/{cityId}/rainfall/total")]
        /// <summary>
        /// Get weather summary for a city
        /// </summary>
        [HttpGet("city/{cityId}/weather/summary")]
        public async Task<ActionResult> GetWeatherSummary(int cityId)
        {
            try
            {
                var summary = await _sensorService.GetWeatherSummaryAsync(cityId);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving weather summary for city {CityId}", cityId);
                return StatusCode(500, new { error = "Failed to retrieve weather summary", details = ex.Message });
            }
        }

        /// <summary>
        /// Get critical sensors (those triggering alerts)
        /// </summary>
        [HttpGet("critical")]
        public async Task<ActionResult> GetCriticalSensors()
        {
            try
            {
                var sensors = await _sensorService.GetCriticalWaterLevelSensorsAsync();
                return Ok(sensors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving critical sensors");
                return StatusCode(500, new { error = "Failed to retrieve critical sensors", details = ex.Message });
            }
        }

        #endregion
    }

    #region DTOs

    public class WaterLevelDataDto
    {
        public double Level { get; set; }
    }

    public class RainfallDataDto
    {
        public double Rainfall { get; set; }
        public DateTime PeriodStart { get; set; } = DateTime.UtcNow.AddMinutes(-15);
        public DateTime PeriodEnd { get; set; } = DateTime.UtcNow;
    }

    public class SensorStatusDto
    {
        public SensorStatus Status { get; set; }
    }

    #endregion
}
