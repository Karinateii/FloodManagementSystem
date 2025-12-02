using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GlobalDisasterManagement.Models;
using GlobalDisasterManagement.Services.Interfaces;

namespace GlobalDisasterManagement.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class SheltersApiController : ControllerBase
    {
        private readonly IShelterService _shelterService;
        private readonly ILogger<SheltersApiController> _logger;

        public SheltersApiController(
            IShelterService shelterService,
            ILogger<SheltersApiController> logger)
        {
            _shelterService = shelterService;
            _logger = logger;
        }

        /// <summary>
        /// Get all active shelters
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<EmergencyShelter>>> GetActiveShelters()
        {
            try
            {
                var shelters = await _shelterService.GetActiveSheltersAsync();
                return Ok(shelters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active shelters");
                return StatusCode(500, "An error occurred while retrieving shelters");
            }
        }

        /// <summary>
        /// Get shelter by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EmergencyShelter>> GetShelter(Guid id)
        {
            try
            {
                var shelter = await _shelterService.GetShelterByIdAsync(id);
                if (shelter == null)
                {
                    return NotFound($"Shelter with ID {id} not found");
                }

                return Ok(shelter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving shelter {ShelterId}", id);
                return StatusCode(500, "An error occurred while retrieving the shelter");
            }
        }

        /// <summary>
        /// Find shelters near a location
        /// </summary>
        [HttpGet("nearby")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<EmergencyShelter>>> GetNearbyShelters(
            [FromQuery] double latitude,
            [FromQuery] double longitude,
            [FromQuery] double radiusKm = 10)
        {
            try
            {
                var shelters = await _shelterService.GetNearbySheltersAsync(latitude, longitude, radiusKm);
                return Ok(shelters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding nearby shelters");
                return StatusCode(500, "An error occurred while finding shelters");
            }
        }

        /// <summary>
        /// Get shelters with available space
        /// </summary>
        [HttpGet("available")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<EmergencyShelter>>> GetAvailableShelters()
        {
            try
            {
                var shelters = await _shelterService.GetSheltersWithAvailabilityAsync();
                return Ok(shelters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available shelters");
                return StatusCode(500, "An error occurred while retrieving shelters");
            }
        }

        /// <summary>
        /// Check in to a shelter
        /// </summary>
        [HttpPost("{shelterId}/checkin")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ShelterCheckIn>> CheckIn(Guid shelterId, [FromBody] ShelterCheckIn checkIn)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                checkIn.ShelterId = shelterId;
                var result = await _shelterService.CheckInToShelterAsync(checkIn);

                return CreatedAtAction(nameof(GetShelter), new { id = shelterId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing shelter check-in");
                return StatusCode(500, "An error occurred while processing check-in");
            }
        }

        /// <summary>
        /// Check out from shelter
        /// </summary>
        [HttpPost("checkout/{checkInId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> CheckOut(Guid checkInId)
        {
            try
            {
                var result = await _shelterService.CheckOutFromShelterAsync(checkInId);
                if (!result)
                {
                    return NotFound($"Check-in with ID {checkInId} not found");
                }

                return Ok(new { Message = "Checked out successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing checkout");
                return StatusCode(500, "An error occurred while processing checkout");
            }
        }

        /// <summary>
        /// Get shelter capacity statistics
        /// </summary>
        [HttpGet("statistics/capacity")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetCapacityStatistics()
        {
            try
            {
                var totalCapacity = await _shelterService.GetTotalShelterCapacityAsync();
                var totalOccupancy = await _shelterService.GetTotalOccupancyAsync();
                var occupancyStats = await _shelterService.GetShelterOccupancyStatsAsync();

                return Ok(new
                {
                    TotalCapacity = totalCapacity,
                    TotalOccupancy = totalOccupancy,
                    AvailableSpaces = totalCapacity - totalOccupancy,
                    OccupancyPercentage = totalCapacity > 0 ? (double)totalOccupancy / totalCapacity * 100 : 0,
                    PerShelterOccupancy = occupancyStats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving capacity statistics");
                return StatusCode(500, "An error occurred while retrieving statistics");
            }
        }

        /// <summary>
        /// Create a new shelter (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<EmergencyShelter>> CreateShelter([FromBody] EmergencyShelter shelter)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdShelter = await _shelterService.CreateShelterAsync(shelter);
                return CreatedAtAction(nameof(GetShelter), new { id = createdShelter.Id }, createdShelter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating shelter");
                return StatusCode(500, "An error occurred while creating the shelter");
            }
        }

        /// <summary>
        /// Update shelter capacity (Admin only)
        /// </summary>
        [HttpPatch("{id}/capacity")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateCapacity(Guid id, [FromBody] UpdateCapacityRequest request)
        {
            try
            {
                var result = await _shelterService.UpdateShelterCapacityAsync(id, request.CurrentOccupancy);
                if (!result)
                {
                    return NotFound($"Shelter with ID {id} not found");
                }

                return Ok(new { Message = "Capacity updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating shelter capacity");
                return StatusCode(500, "An error occurred while updating capacity");
            }
        }
    }

    public class UpdateCapacityRequest
    {
        public int CurrentOccupancy { get; set; }
    }
}
