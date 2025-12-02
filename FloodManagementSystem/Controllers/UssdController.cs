using Microsoft.AspNetCore.Mvc;
using GlobalDisasterManagement.Services.Abstract;
using GlobalDisasterManagement.Models;

namespace GlobalDisasterManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UssdController : ControllerBase
    {
        private readonly IUssdService _ussdService;
        private readonly ILogger<UssdController> _logger;

        public UssdController(IUssdService ussdService, ILogger<UssdController> logger)
        {
            _ussdService = ussdService;
            _logger = logger;
        }

        /// <summary>
        /// Africa's Talking USSD callback endpoint
        /// </summary>
        /// <param name="sessionId">Session ID from Africa's Talking</param>
        /// <param name="serviceCode">USSD service code dialed</param>
        /// <param name="phoneNumber">User's phone number</param>
        /// <param name="text">User input (empty for new session)</param>
        /// <returns>USSD response in Africa's Talking format</returns>
        [HttpPost("callback")]
        [Produces("text/plain")]
        public async Task<ContentResult> HandleUssdCallback(
            [FromForm] string sessionId,
            [FromForm] string serviceCode,
            [FromForm] string phoneNumber,
            [FromForm] string text)
        {
            try
            {
                _logger.LogInformation("USSD Request - Session: {SessionId}, Phone: {PhoneNumber}, Input: {Text}", 
                    sessionId, phoneNumber, text);

                // Determine if this is an initial request or a response
                var requestType = string.IsNullOrEmpty(text) 
                    ? UssdRequestType.Initiation 
                    : UssdRequestType.Response;

                // Process the USSD request
                var response = await _ussdService.ProcessUssdRequestAsync(
                    sessionId, 
                    phoneNumber, 
                    text, 
                    requestType);

                _logger.LogInformation("USSD Response - Session: {SessionId}, Response: {Response}", 
                    sessionId, response);

                // Return response as plain text (Africa's Talking format)
                return Content(response, "text/plain");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing USSD request - Session: {SessionId}", sessionId);
                return Content("END An error occurred. Please try again later.", "text/plain");
            }
        }

        /// <summary>
        /// Test endpoint to simulate USSD interaction (for development)
        /// </summary>
        [HttpGet("test")]
        public async Task<IActionResult> TestUssd(
            [FromQuery] string sessionId = "test-session-001",
            [FromQuery] string phoneNumber = "+2348012345678",
            [FromQuery] string text = "")
        {
            try
            {
                var requestType = string.IsNullOrEmpty(text) 
                    ? UssdRequestType.Initiation 
                    : UssdRequestType.Response;

                var response = await _ussdService.ProcessUssdRequestAsync(
                    sessionId, 
                    phoneNumber, 
                    text, 
                    requestType);

                return Ok(new
                {
                    sessionId,
                    phoneNumber,
                    input = text,
                    response,
                    message = "USSD test successful. In production, this would be displayed on the user's phone."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in USSD test endpoint");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get active USSD sessions (admin only)
        /// </summary>
        [HttpGet("sessions")]
        public IActionResult GetActiveSessions()
        {
            // This would typically require admin authentication
            return Ok(new { message = "Feature coming soon - admin authentication required" });
        }

        /// <summary>
        /// Get USSD statistics
        /// </summary>
        [HttpGet("statistics")]
        public IActionResult GetStatistics()
        {
            return Ok(new
            {
                message = "USSD Statistics",
                features = new[]
                {
                    "Report Disaster via USSD",
                    "View Active Alerts",
                    "Find Nearby Shelters",
                    "Get Emergency Contacts",
                    "Access Safety Tips",
                    "Get Evacuation Information"
                },
                status = "Service Active",
                provider = "Africa's Talking"
            });
        }
    }
}
