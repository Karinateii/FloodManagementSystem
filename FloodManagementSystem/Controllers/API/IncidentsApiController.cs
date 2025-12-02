using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GlobalDisasterManagement.Models;
using GlobalDisasterManagement.Services.Interfaces;

namespace GlobalDisasterManagement.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class IncidentsApiController : ControllerBase
    {
        private readonly IIncidentService _incidentService;
        private readonly ILogger<IncidentsApiController> _logger;

        public IncidentsApiController(
            IIncidentService incidentService,
            ILogger<IncidentsApiController> logger)
        {
            _incidentService = incidentService;
            _logger = logger;
        }

        /// <summary>
        /// Get all active incidents
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<DisasterIncident>>> GetActiveIncidents()
        {
            try
            {
                var incidents = await _incidentService.GetActiveIncidentsAsync();
                return Ok(incidents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active incidents");
                return StatusCode(500, "An error occurred while retrieving incidents");
            }
        }

        /// <summary>
        /// Get incident by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DisasterIncident>> GetIncident(Guid id)
        {
            try
            {
                var incident = await _incidentService.GetIncidentByIdAsync(id);
                if (incident == null)
                {
                    return NotFound($"Incident with ID {id} not found");
                }

                return Ok(incident);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving incident {IncidentId}", id);
                return StatusCode(500, "An error occurred while retrieving the incident");
            }
        }

        /// <summary>
        /// Get incidents by disaster type
        /// </summary>
        [HttpGet("type/{disasterType}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<DisasterIncident>>> GetByDisasterType(DisasterType disasterType)
        {
            try
            {
                var incidents = await _incidentService.GetIncidentsByDisasterTypeAsync(disasterType);
                return Ok(incidents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving incidents by type {Type}", disasterType);
                return StatusCode(500, "An error occurred while retrieving incidents");
            }
        }

        /// <summary>
        /// Get incidents by severity level
        /// </summary>
        [HttpGet("severity/{severity}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<DisasterIncident>>> GetBySeverity(IncidentSeverity severity)
        {
            try
            {
                var incidents = await _incidentService.GetIncidentsBySeverityAsync(severity);
                return Ok(incidents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving incidents by severity {Severity}", severity);
                return StatusCode(500, "An error occurred while retrieving incidents");
            }
        }

        /// <summary>
        /// Get incidents near a location
        /// </summary>
        [HttpGet("nearby")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<DisasterIncident>>> GetNearbyIncidents(
            [FromQuery] double latitude,
            [FromQuery] double longitude,
            [FromQuery] double radiusKm = 10)
        {
            try
            {
                var incidents = await _incidentService.GetIncidentsByLocationAsync(latitude, longitude, radiusKm);
                return Ok(incidents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving nearby incidents");
                return StatusCode(500, "An error occurred while retrieving incidents");
            }
        }

        /// <summary>
        /// Create a new incident report
        /// </summary>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DisasterIncident>> CreateIncident([FromBody] DisasterIncident incident)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdIncident = await _incidentService.CreateIncidentAsync(incident);
                return CreatedAtAction(nameof(GetIncident), new { id = createdIncident.Id }, createdIncident);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating incident");
                return StatusCode(500, "An error occurred while creating the incident");
            }
        }

        /// <summary>
        /// Update incident status (Admin only)
        /// </summary>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                var result = await _incidentService.UpdateIncidentStatusAsync(id, request.Status, request.Notes);
                if (!result)
                {
                    return NotFound($"Incident with ID {id} not found");
                }

                return Ok(new { Message = "Status updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating incident status");
                return StatusCode(500, "An error occurred while updating status");
            }
        }

        /// <summary>
        /// Get incident statistics
        /// </summary>
        [HttpGet("statistics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetStatistics()
        {
            try
            {
                var activeCount = await _incidentService.GetActiveIncidentCountAsync();
                var bySeverity = await _incidentService.GetIncidentStatisticsBySeverityAsync();
                var byType = await _incidentService.GetIncidentStatisticsByTypeAsync();

                return Ok(new
                {
                    ActiveIncidents = activeCount,
                    BySeverity = bySeverity,
                    ByDisasterType = byType
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving incident statistics");
                return StatusCode(500, "An error occurred while retrieving statistics");
            }
        }

        /// <summary>
        /// Verify an incident (Admin only)
        /// </summary>
        [HttpPost("{id}/verify")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> VerifyIncident(Guid id, [FromBody] VerifyRequest request)
        {
            try
            {
                var result = await _incidentService.VerifyIncidentAsync(id, request.VerifiedBy);
                if (!result)
                {
                    return NotFound($"Incident with ID {id} not found");
                }

                return Ok(new { Message = "Incident verified successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying incident");
                return StatusCode(500, "An error occurred while verifying the incident");
            }
        }
    }

    public class UpdateStatusRequest
    {
        public IncidentStatus Status { get; set; }
        public string? Notes { get; set; }
    }

    public class VerifyRequest
    {
        public string VerifiedBy { get; set; } = string.Empty;
    }
}
