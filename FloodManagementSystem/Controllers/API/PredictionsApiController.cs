using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GlobalDisasterManagement.Repositories.Interfaces;
using GlobalDisasterManagement.Models;

namespace GlobalDisasterManagement.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PredictionsApiController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PredictionsApiController> _logger;

        public PredictionsApiController(
            IUnitOfWork unitOfWork,
            ILogger<PredictionsApiController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Get all flood predictions
        /// </summary>
        [HttpGet]
        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<CityFloodPrediction>>> GetAllPredictions()
        {
            try
            {
                var predictions = await _unitOfWork.CityPredictions.GetAllAsync();
                return Ok(predictions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all predictions");
                return StatusCode(500, new { message = "An error occurred while retrieving predictions" });
            }
        }

        /// <summary>
        /// Get predictions by city ID
        /// </summary>
        [HttpGet("city/{cityId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<CityFloodPrediction>>> GetPredictionsByCity(int cityId)
        {
            try
            {
                var city = await _unitOfWork.Cities.GetByIdAsync(cityId);
                if (city == null)
                {
                    return NotFound(new { message = $"City with ID {cityId} not found" });
                }

                var predictions = await _unitOfWork.CityPredictions.FindAsync(p => p.CityId == cityId);
                return Ok(new
                {
                    cityId,
                    cityName = city.Name,
                    predictions,
                    totalPredictions = predictions.Count(),
                    highRiskCount = predictions.Count(p => p.Prediction == true.ToString())
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving predictions for city {CityId}", cityId);
                return StatusCode(500, new { message = "An error occurred while retrieving predictions" });
            }
        }

        /// <summary>
        /// Get predictions by year
        /// </summary>
        [HttpGet("year/{year}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<CityFloodPrediction>>> GetPredictionsByYear(string year)
        {
            try
            {
                var predictions = await _unitOfWork.CityPredictions.FindAsync(p => p.Year == year);
                return Ok(new
                {
                    year,
                    predictions,
                    totalPredictions = predictions.Count(),
                    highRiskCount = predictions.Count(p => p.Prediction == true.ToString())
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving predictions for year {Year}", year);
                return StatusCode(500, new { message = "An error occurred while retrieving predictions" });
            }
        }

        /// <summary>
        /// Get high-risk predictions
        /// </summary>
        [HttpGet("high-risk")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<CityFloodPrediction>>> GetHighRiskPredictions()
        {
            try
            {
                var predictions = await _unitOfWork.CityPredictions.FindAsync(
                    p => p.Prediction == true.ToString());
                return Ok(predictions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving high-risk predictions");
                return StatusCode(500, new { message = "An error occurred while retrieving predictions" });
            }
        }

        /// <summary>
        /// Get prediction statistics
        /// </summary>
        [HttpGet("statistics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetStatistics()
        {
            try
            {
                var allPredictions = await _unitOfWork.CityPredictions.GetAllAsync();
                var cities = await _unitOfWork.Cities.GetAllAsync();
                
                var statistics = new
                {
                    totalPredictions = allPredictions.Count(),
                    highRiskPredictions = allPredictions.Count(p => p.Prediction == true.ToString()),
                    lowRiskPredictions = allPredictions.Count(p => p.Prediction == false.ToString()),
                    citiesMonitored = cities.Count(),
                    predictionsByCity = allPredictions
                        .GroupBy(p => p.City)
                        .Select(g => new
                        {
                            city = g.Key,
                            total = g.Count(),
                            highRisk = g.Count(p => p.Prediction == true.ToString())
                        }),
                    predictionsByYear = allPredictions
                        .GroupBy(p => p.Year)
                        .Select(g => new
                        {
                            year = g.Key,
                            total = g.Count(),
                            highRisk = g.Count(p => p.Prediction == true.ToString())
                        })
                };

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving statistics");
                return StatusCode(500, new { message = "An error occurred while retrieving statistics" });
            }
        }

        /// <summary>
        /// Get prediction by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CityFloodPrediction>> GetPrediction(int id)
        {
            try
            {
                var prediction = await _unitOfWork.CityPredictions.GetByIdAsync(id);
                if (prediction == null)
                {
                    return NotFound(new { message = $"Prediction with ID {id} not found" });
                }

                return Ok(prediction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving prediction {Id}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the prediction" });
            }
        }
    }
}
