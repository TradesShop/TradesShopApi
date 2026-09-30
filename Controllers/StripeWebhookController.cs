using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Stripe;
using TradePlatform.Api.Services;

[ApiController]
[Route("api/stripe/webhook")]
public class StripeWebhookController : ControllerBase
{
	private readonly IStripeWebhookService _webhookService;

	private readonly ILogger<StripeWebhookController> _logger;

	private readonly string _webhookSecret;

	public StripeWebhookController(IStripeWebhookService webhookService, IConfiguration config, ILogger<StripeWebhookController> logger)
	{
		_webhookService = webhookService;
		_logger = logger;
		_webhookSecret = config["Stripe:WebhookSecret"] ?? throw new InvalidOperationException("Stripe webhook secret missing.");
	}

	[HttpPost]
	public async Task<IActionResult> Handle()
	{
		string json = await new StreamReader(Request.Body).ReadToEndAsync();
		StringValues signature = Request.Headers["Stripe-Signature"];
		HttpContext.Request.EnableBuffering();
		using (new StreamReader(HttpContext.Request.Body, null, detectEncodingFromByteOrderMarks: true, -1, leaveOpen: true))
		{
			HttpContext.Request.Body.Position = 0L;
			Request.Headers.TryGetValue("Stripe-Signature", out var signatureHeader);
			_logger.LogInformation("=== STRIPE DEBUG START ===");
			_logger.LogInformation("Incoming Header: {Header}", signatureHeader.ToString());
			_logger.LogInformation("Configured Secret in App: {Secret}", _webhookSecret);
			_logger.LogInformation("Body Length: {Length}", json?.Length ?? 0);
			_logger.LogInformation("=== STRIPE DEBUG END ===");
			Event stripeEvent;
			try
			{
				stripeEvent = EventUtility.ConstructEvent(json, signature, _webhookSecret, 300L, throwOnApiVersionMismatch: false);
				_logger.LogInformation("Signature validation passed. Event type: {Type}", stripeEvent.Type);
			}
			catch (Exception exception)
			{
				_logger.LogError(exception, "Signature validation FAILED. Raw body: {Body}", json);
				return BadRequest();
			}
			try
			{
				_logger.LogInformation("Processing event type: {Type}", stripeEvent.Type);
				await _webhookService.HandleEventAsync(stripeEvent, json, signature);
			}
			catch (Exception exception2)
			{
				_logger.LogError(exception2, "Webhook handler failed for event {EventId}", stripeEvent.Id);
				throw;
			}
			return Ok();
		}
	}
}
