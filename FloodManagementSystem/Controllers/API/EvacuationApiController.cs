using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GlobalDisasterManagement.Models;
using GlobalDisasterManagement.Services.Interfaces;

namespace GlobalDisasterManagement.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class EvacuationApiController : ControllerBase
    {
        private readonly IEvacuationService _evacuationService;
        private readonly ILogger<EvacuationApiController> _logger;

        public EvacuationApiController(
            IEvacuationService evacuationService,
            ILogger<EvacuationApiController> logger)
        {
            _evacuationService = evacuationService;
            _logger = logger;
        }

        /// <summary>
        /// Get all active evacuation routes
        /// </summary>
        [HttpGet("routes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<EvacuationRoute>>> GetActiveRoutes()
        {
            try
            {
                var routes = await _evacuationService.GetActiveRoutesAsync();
                return Ok(routes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active routes");
                return StatusCode(500, "An error occurred while retrieving routes");
            }
        }

        /// <summary>
        /// Get evacuation route by ID
        /// </summary>
        [HttpGet("routes/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EvacuationRoute>> GetRoute(Guid id)
        {
            try
            {
                var route = await _evacuationService.GetRouteByIdAsync(id);
                if (route == null)
                {
                    return NotFound($"Route with ID {id} not found");
                }

                return Ok(route);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving route {RouteId}", id);
                return StatusCode(500, "An error occurred while retrieving the route");
            }
        }

        /// <summary>
        /// Find evacuation routes from a location
        /// </summary>
        [HttpGet("routes/from")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<EvacuationRoute>>> GetRoutesFromLocation(
            [FromQuery] double latitude,
            [FromQuery] double longitude)
        {
            try
            {
                var routes = await _evacuationService.GetRoutesFromLocationAsync(latitude, longitude);
                return Ok(routes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding routes from location");
                return StatusCode(500, "An error occurred while finding routes");
            }
        }

        /// <summary>
        /// Get optimal evacuation route
        /// </summary>
        [HttpGet("routes/optimal")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EvacuationRoute>> GetOptimalRoute(
            [FromQuery] double startLat,
            [FromQuery] double startLng,
            [FromQuery] double endLat,
            [FromQuery] double endLng)
        {
            try
            {
                var route = await _evacuationService.GetOptimalRouteAsync(startLat, startLng, endLat, endLng);
                if (route == null)
                {
                    return NotFound("No suitable evacuation route found");
                }

                return Ok(route);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding optimal route");
                return StatusCode(500, "An error occurred while finding the optimal route");
            }
        }

        /// <summary>
        /// Get safe evacuation routes
        /// </summary>
        [HttpGet("routes/safe")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<EvacuationRoute>>> GetSafeRoutes()
        {
            try
            {
                var routes = await _evacuationService.GetSafeRoutesAsync();
                return Ok(routes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving safe routes");
                return StatusCode(500, "An error occurred while retrieving routes");
            }
        }

        /// <summary>
        /// Get emergency contacts
        /// </summary>
        [HttpGet("contacts")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<EmergencyContact>>> GetEmergencyContacts(
            [FromQuery] string? countryCode = null)
        {
            try
            {
                var contacts = await _evacuationService.GetEmergencyContactsAsync(countryCode);
                return Ok(contacts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving emergency contacts");
                return StatusCode(500, "An error occurred while retrieving contacts");
            }
        }

        /// <summary>
        /// Get emergency contacts by service type
        /// </summary>
        [HttpGet("contacts/type/{serviceType}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<EmergencyContact>>> GetContactsByType(EmergencyServiceType serviceType)
        {
            try
            {
                var contacts = await _evacuationService.GetEmergencyContactsByTypeAsync(serviceType);
                return Ok(contacts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving contacts by type {Type}", serviceType);
                return StatusCode(500, "An error occurred while retrieving contacts");
            }
        }

        /// <summary>
        /// Update route status (Admin only)
        /// </summary>
        [HttpPatch("routes/{id}/status")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateRouteStatus(Guid id, [FromBody] UpdateRouteStatusRequest request)
        {
            try
            {
                var result = await _evacuationService.UpdateRouteStatusAsync(id, request.Status);
                if (!result)
                {
                    return NotFound($"Route with ID {id} not found");
                }

                return Ok(new { Message = "Route status updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating route status");
                return StatusCode(500, "An error occurred while updating status");
            }
        }
    }

    public class UpdateRouteStatusRequest
    {
        public RouteStatus Status { get; set; }
    }
}
