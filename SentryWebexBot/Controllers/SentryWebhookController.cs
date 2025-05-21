using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SentryWebexBot.Models;
using SentryWebexBot.Services;

namespace SentryWebexBot.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SentryWebhookController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SentryWebhookController> _logger;
        private readonly IWebexService _webexService;

        public SentryWebhookController(
            IConfiguration configuration, 
            ILogger<SentryWebhookController> logger,
            IWebexService webexService)
        {
            _configuration = configuration;
            _logger = logger;
            _webexService = webexService;
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            try
            {
                var sentryWebhookSecret = _configuration["Sentry:WebhookSecret"];
                if (string.IsNullOrEmpty(sentryWebhookSecret))
                {
                    return Problem("Webhook secret not configured");
                }
                
                if (!Request.Headers.TryGetValue("Sentry-Hook-Signature", out var signatureValues))
                {
                    return BadRequest("Sentry-Hook-Signature header not present");
                }

                var signature = signatureValues.FirstOrDefault();
                if (string.IsNullOrEmpty(signature))
                {
                    return BadRequest("Signature header was found, but was empty");
                }

                // enable buffering so the request can be read twice: once for signature validation,
                // once for deserialization
                Request.EnableBuffering();
                string requestBody;
                using (var reader = new StreamReader(
                    Request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    leaveOpen: true))
                {
                    requestBody = await reader.ReadToEndAsync();
                }
                Request.Body.Position = 0;
                
                if (!ValidateSignature(requestBody, signature, sentryWebhookSecret))
                {
                    _logger.LogWarning("Invalid signature");
                    return BadRequest("Invalid signature");
                }

                var payload = await JsonSerializer.DeserializeAsync<SentryWebhookPayload>(Request.Body);
                if (payload == null)
                {
                    _logger.LogWarning("Failed to deserialize webhook payload");
                    return BadRequest("Invalid payload");
                }

                _logger.LogInformation($"Processing Sentry webhook - Action: {payload.Action}, Issue: {payload.Data?.Issue?.Title}");
                
                var success = await _webexService.SendSentryEventAsync(payload);
                if (!success)
                {
                    _logger.LogWarning("Failed to send message to Webex");
                    return StatusCode(500, "Failed to forward to Webex");
                }
                
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured during webhook processing");
                return Problem($"Unexpected error: {ex.Message}", statusCode: 500);
            }
        }
        private bool ValidateSignature(string payload, string signature, string secret)
        {
            try
            {
                if (string.IsNullOrEmpty(payload) || string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(secret))
                {
                    return false;
                }

                using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                var computedSignature = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                
                return string.Equals(computedSignature, signature, StringComparison.Ordinal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Sentry webhook signature");
                return false;
            }
        }
    }
}
