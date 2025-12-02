using GlobalDisasterManagement.Services.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace GlobalDisasterManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WhatsAppWebhookController : ControllerBase
    {
        private readonly IWhatsAppService _whatsAppService;
        private readonly ILogger<WhatsAppWebhookController> _logger;

        public WhatsAppWebhookController(IWhatsAppService whatsAppService, ILogger<WhatsAppWebhookController> logger)
        {
            _whatsAppService = whatsAppService;
            _logger = logger;
        }

        /// <summary>
        /// Webhook endpoint for incoming WhatsApp messages from Twilio
        /// </summary>
        [HttpPost("incoming")]
        public async Task<IActionResult> IncomingMessage(
            [FromForm] string From,
            [FromForm] string To,
            [FromForm] string Body,
            [FromForm] string MessageSid,
            [FromForm] string MediaUrl0 = null,
            [FromForm] string MediaContentType0 = null,
            [FromForm] int NumMedia = 0)
        {
            try
            {
                _logger.LogInformation("Received WhatsApp message from {From}: {Body}", From, Body);

                await _whatsAppService.ProcessInboundMessageAsync(
                    from: From,
                    to: To,
                    body: Body,
                    messageSid: MessageSid,
                    mediaUrl: MediaUrl0,
                    mediaType: MediaContentType0
                );

                // Return TwiML response (empty response is fine for WhatsApp)
                return Content("<?xml version=\"1.0\" encoding=\"UTF-8\"?><Response></Response>", "application/xml");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing incoming WhatsApp message");
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Webhook endpoint for WhatsApp message status updates from Twilio
        /// </summary>
        [HttpPost("status")]
        public async Task<IActionResult> StatusCallback(
            [FromForm] string MessageSid,
            [FromForm] string MessageStatus,
            [FromForm] string ErrorCode = null,
            [FromForm] string ErrorMessage = null)
        {
            try
            {
                _logger.LogInformation("WhatsApp message status update: {MessageSid} -> {Status}", 
                    MessageSid, MessageStatus);

                await _whatsAppService.ProcessMessageStatusAsync(
                    messageSid: MessageSid,
                    status: MessageStatus,
                    errorCode: ErrorCode,
                    errorMessage: ErrorMessage
                );

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing WhatsApp status callback");
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Test endpoint to verify webhook is working
        /// </summary>
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new
            {
                status = "WhatsApp webhook is working",
                timestamp = DateTime.UtcNow,
                endpoints = new[]
                {
                    "/api/whatsappwebhook/incoming - For incoming messages",
                    "/api/whatsappwebhook/status - For status updates"
                }
            });
        }
    }
}
