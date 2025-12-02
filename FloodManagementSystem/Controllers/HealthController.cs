using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GlobalDisasterManagement.Data;

namespace GlobalDisasterManagement.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly DisasterDbContext _context;
        private readonly ILogger<HealthController> _logger;

        public HealthController(DisasterDbContext context, ILogger<HealthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                // Check database connectivity
                var canConnect = await _context.Database.CanConnectAsync();
                
                if (!canConnect)
                {
                    _logger.LogWarning("Health check failed: Cannot connect to database");
                    return StatusCode(503, new
                    {
                        status = "Unhealthy",
                        timestamp = DateTime.UtcNow,
                        checks = new
                        {
                            database = "Failed"
                        }
                    });
                }

                // Get some basic stats
                var citiesCount = await _context.Cities.CountAsync();
                var predictionsCount = await _context.CityPredictions.CountAsync();

                return Ok(new
                {
                    status = "Healthy",
                    timestamp = DateTime.UtcNow,
                    version = "1.0.0",
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                    checks = new
                    {
                        database = "Healthy",
                        citiesCount,
                        predictionsCount
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed with exception");
                return StatusCode(503, new
                {
                    status = "Unhealthy",
                    timestamp = DateTime.UtcNow,
                    error = ex.Message
                });
            }
        }
    }
}
