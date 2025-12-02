using GlobalDisasterManagement.Data;
using GlobalDisasterManagement.Models;
using GlobalDisasterManagement.Services.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace GlobalDisasterManagement.Controllers
{
    [Route("api/voicewebhook")]
    [ApiController]
    public class VoiceWebhookController : ControllerBase
    {
        private readonly IVoiceService _voiceService;
        private readonly DisasterDbContext _context;
        private readonly ILogger<VoiceWebhookController> _logger;

        public VoiceWebhookController(IVoiceService voiceService, DisasterDbContext context, ILogger<VoiceWebhookController> logger)
        {
            _voiceService = voiceService;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Handle incoming voice calls
        /// </summary>
        [HttpPost("incoming")]
        public async Task<ContentResult> HandleIncomingCall([FromForm] string From, [FromForm] string To, [FromForm] string CallSid)
        {
            try
            {
                _logger.LogInformation("Incoming call from {From} to {To}, CallSid: {CallSid}", From, To, CallSid);
                
                var twiml = await _voiceService.ProcessInboundCallAsync(From, To, CallSid);
                
                return Content(twiml, "text/xml");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing incoming call");
                return Content("<Response><Say>We're sorry, an error occurred. Please try again later.</Say><Hangup/></Response>", "text/xml");
            }
        }

        /// <summary>
        /// Handle call status updates
        /// </summary>
        [HttpPost("status")]
        public async Task<IActionResult> HandleCallStatus(
            [FromForm] string CallSid,
            [FromForm] string CallStatus,
            [FromForm] int? CallDuration = null,
            [FromForm] string? ErrorCode = null,
            [FromForm] string? ErrorMessage = null)
        {
            try
            {
                _logger.LogInformation("Call status update for {CallSid}: {Status}", CallSid, CallStatus);
                
                await _voiceService.ProcessCallStatusAsync(CallSid, CallStatus, CallDuration, ErrorCode, ErrorMessage);
                
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing call status update");
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Handle IVR menu input
        /// </summary>
        [HttpPost("ivr-input")]
        public async Task<ContentResult> HandleIVRInput(
            [FromForm] string CallSid,
            [FromForm] string Digits)
        {
            try
            {
                _logger.LogInformation("IVR input from {CallSid}: {Digits}", CallSid, Digits);
                
                var twiml = await _voiceService.ProcessIVRInputAsync(CallSid, Digits, IVRMenuState.MainMenu);
                
                return Content(twiml, "text/xml");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing IVR input");
                return Content("<Response><Say>Invalid input. Returning to main menu.</Say><Redirect>/api/voicewebhook/incoming</Redirect></Response>", "text/xml");
            }
        }

        /// <summary>
        /// Generate TwiML for disaster alert
        /// </summary>
        [HttpGet("alert/{alertId}")]
        [HttpPost("alert/{alertId}")]
        public async Task<ContentResult> GetAlertTwiML(Guid alertId)
        {
            try
            {
                var alert = await _context.DisasterAlerts.FindAsync(alertId);

                if (alert == null)
                {
                    return Content("<Response><Say>Alert not found.</Say><Hangup/></Response>", "text/xml");
                }

                var twiml = await _voiceService.GenerateAlertTwiMLAsync(alert);
                return Content(twiml, "text/xml");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating alert TwiML");
                return Content("<Response><Say>Error retrieving alert.</Say><Hangup/></Response>", "text/xml");
            }
        }

        /// <summary>
        /// Generate TwiML for IVR menu
        /// </summary>
        [HttpGet("ivr-menu")]
        [HttpPost("ivr-menu")]
        public async Task<ContentResult> GetIVRMenuTwiML()
        {
            try
            {
                var twiml = await _voiceService.GenerateIVRMenuTwiMLAsync(IVRMenuState.MainMenu);
                return Content(twiml, "text/xml");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating IVR menu TwiML");
                return Content("<Response><Say>Error loading menu.</Say><Hangup/></Response>", "text/xml");
            }
        }

        /// <summary>
        /// Handle recording callback
        /// </summary>
        [HttpPost("recording")]
        public async Task<IActionResult> HandleRecording(
            [FromForm] string CallSid,
            [FromForm] string RecordingUrl,
            [FromForm] int RecordingDuration)
        {
            try
            {
                _logger.LogInformation("Recording received for {CallSid}: {Url}, Duration: {Duration}s", 
                    CallSid, RecordingUrl, RecordingDuration);
                
                // Store recording URL in database
                // Implementation depends on your specific requirements
                
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing recording callback");
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new
            {
                status = "Voice webhook endpoint is working",
                timestamp = DateTime.UtcNow,
                endpoints = new[]
                {
                    "POST /api/voicewebhook/incoming",
                    "POST /api/voicewebhook/status",
                    "POST /api/voicewebhook/ivr-input",
                    "GET/POST /api/voicewebhook/alert/{alertId}",
                    "GET/POST /api/voicewebhook/ivr-menu",
                    "POST /api/voicewebhook/recording"
                }
            });
        }
    }
}
