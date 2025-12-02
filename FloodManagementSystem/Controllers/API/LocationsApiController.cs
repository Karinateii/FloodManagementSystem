using Microsoft.AspNetCore.Mvc;
using GlobalDisasterManagement.Repositories.Interfaces;
using GlobalDisasterManagement.Models;

namespace GlobalDisasterManagement.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CitiesApiController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CitiesApiController> _logger;

        public CitiesApiController(
            IUnitOfWork unitOfWork,
            ILogger<CitiesApiController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Get all cities
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<City>>> GetAllCities()
        {
            try
            {
                var cities = await _unitOfWork.Cities.GetAllAsync();
                return Ok(cities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cities");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get city by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<City>> GetCity(int id)
        {
            try
            {
                var city = await _unitOfWork.Cities.GetByIdAsync(id);
                if (city == null)
                {
                    return NotFound(new { message = $"City with ID {id} not found" });
                }
                return Ok(city);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving city {Id}", id);
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get cities by LGA
        /// </summary>
        [HttpGet("lga/{lgaId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<City>>> GetCitiesByLGA(int lgaId)
        {
            try
            {
                var cities = await _unitOfWork.Cities.FindAsync(c => c.LGAId == lgaId);
                return Ok(cities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cities for LGA {LgaId}", lgaId);
                return StatusCode(500, new { message = "An error occurred" });
            }
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class LGAsApiController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LGAsApiController> _logger;

        public LGAsApiController(
            IUnitOfWork unitOfWork,
            ILogger<LGAsApiController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Get all LGAs
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<LGA>>> GetAllLGAs()
        {
            try
            {
                var lgas = await _unitOfWork.LGAs.GetAllAsync();
                return Ok(lgas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving LGAs");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get LGA by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LGA>> GetLGA(int id)
        {
            try
            {
                var lga = await _unitOfWork.LGAs.GetByIdAsync(id);
                if (lga == null)
                {
                    return NotFound(new { message = $"LGA with ID {id} not found" });
                }
                return Ok(lga);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving LGA {Id}", id);
                return StatusCode(500, new { message = "An error occurred" });
            }
        }
    }
}
