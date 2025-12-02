using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GlobalDisasterManagement.Data;

namespace GlobalDisasterManagement.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly DisasterDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(DisasterDbContext context, ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Get statistics
                var totalCities = await _context.Cities.CountAsync();
                var totalLGAs = await _context.LGAs.CountAsync();
                var totalPredictions = await _context.CityPredictions.CountAsync();
                var highRiskPredictions = await _context.CityPredictions
                    .CountAsync(p => p.Prediction == true.ToString());

                ViewBag.TotalCities = totalCities;
                ViewBag.TotalLGAs = totalLGAs;
                ViewBag.TotalPredictions = totalPredictions;
                ViewBag.HighRiskPredictions = highRiskPredictions;
                ViewBag.RiskPercentage = totalPredictions > 0 
                    ? Math.Round((double)highRiskPredictions / totalPredictions * 100, 2) 
                    : 0;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPredictionsByMonth(string year = "2024")
        {
            try
            {
                var predictions = await _context.CityPredictions
                    .Where(p => p.Year == year)
                    .GroupBy(p => p.Month)
                    .Select(g => new
                    {
                        Month = g.Key,
                        HighRisk = g.Count(p => p.Prediction == true.ToString()),
                        LowRisk = g.Count(p => p.Prediction == false.ToString()),
                        Total = g.Count()
                    })
                    .ToListAsync();

                // Order by month
                var monthOrder = new[] { "January", "February", "March", "April", "May", "June", 
                                        "July", "August", "September", "October", "November", "December" };
                
                var orderedData = predictions
                    .OrderBy(p => Array.IndexOf(monthOrder, p.Month))
                    .ToList();

                return Json(orderedData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting predictions by month");
                return Json(new { error = "Failed to load data" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPredictionsByCity(int limit = 10)
        {
            try
            {
                var cityStats = await _context.CityPredictions
                    .GroupBy(p => new { p.CityId, p.City })
                    .Select(g => new
                    {
                        City = g.Key.City,
                        HighRisk = g.Count(p => p.Prediction == true.ToString()),
                        LowRisk = g.Count(p => p.Prediction == false.ToString()),
                        Total = g.Count()
                    })
                    .OrderByDescending(c => c.HighRisk)
                    .Take(limit)
                    .ToListAsync();

                return Json(cityStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting predictions by city");
                return Json(new { error = "Failed to load data" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetYearlyTrends()
        {
            try
            {
                var yearlyData = await _context.CityPredictions
                    .GroupBy(p => p.Year)
                    .Select(g => new
                    {
                        Year = g.Key,
                        HighRisk = g.Count(p => p.Prediction == true.ToString()),
                        LowRisk = g.Count(p => p.Prediction == false.ToString()),
                        Total = g.Count()
                    })
                    .OrderBy(y => y.Year)
                    .ToListAsync();

                return Json(yearlyData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting yearly trends");
                return Json(new { error = "Failed to load data" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetLGAStats()
        {
            try
            {
                var lgaStats = await _context.LGAs
                    .Include(l => l.Cities)
                    .Select(l => new
                    {
                        LGA = l.LGAName,
                        CitiesCount = l.Cities.Count,
                        TotalPredictions = _context.CityPredictions
                            .Count(p => l.Cities.Any(c => c.Id == p.CityId)),
                        HighRiskCount = _context.CityPredictions
                            .Count(p => l.Cities.Any(c => c.Id == p.CityId) && p.Prediction == true.ToString())
                    })
                    .OrderByDescending(l => l.HighRiskCount)
                    .ToListAsync();

                return Json(lgaStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting LGA statistics");
                return Json(new { error = "Failed to load data" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRecentPredictions(int limit = 10)
        {
            try
            {
                var recentPredictions = await _context.CityPredictions
                    .OrderByDescending(p => p.Id)
                    .Take(limit)
                    .Select(p => new
                    {
                        p.City,
                        p.Month,
                        p.Year,
                        IsHighRisk = p.Prediction == true.ToString(),
                        p.ModelFileName
                    })
                    .ToListAsync();

                return Json(recentPredictions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent predictions");
                return Json(new { error = "Failed to load data" });
            }
        }
    }
}
