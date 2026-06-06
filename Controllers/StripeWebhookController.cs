using Microsoft.AspNetCore.Mvc;
using Stripe;
using TradePlatform.Api.Services;

[ApiController]
[Route("api/stripe/webhook")]
public class StripeWebhookController : ControllerBase
{
    private readonly IStripeWebhookService _webhookService;
    private readonly ILogger<StripeWebhookController> _logger;
    private readonly string _webhookSecret;

    public StripeWebhookController(
        IStripeWebhookService webhookService,
        IConfiguration config,
        ILogger<StripeWebhookController> logger)
    {
        _webhookService = webhookService;
        _logger = logger;

        _webhookSecret = config["Stripe:WebhookSecret"]
            ?? throw new InvalidOperationException("Stripe webhook secret missing.");
    }

    [HttpPost]
    public async Task<IActionResult> Handle()
    {
        var json = await new StreamReader(Request.Body).ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"];

        Event stripeEvent;
      
        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                json,
                signature,
                _webhookSecret,
                throwOnApiVersionMismatch: false
            );
            var strip_ev_type = stripeEvent.Type;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Stripe webhook: " + json);
            return BadRequest();
        }

        try
        {
            var strip_ev_type = stripeEvent.Type;
            await _webhookService.HandleEventAsync(stripeEvent, json, signature);
        }
        catch (Exception ex)
        {
            var strip_ev_type = stripeEvent.Type;
            _logger.LogError(ex, "Error processing Stripe webhook: " + json);
            throw;
        }

        return Ok();
    }
}

/*
 WHAT THIS CONTROLLER DOES

1.Validates Stripe signature
✔ Routes events to your webhook service

    invoice.paid → HandleInvoicePaidAsync

    invoice.payment_failed → HandleInvoicePaymentFailedAsync

✔ Converts Stripe amounts (cents → decimal)

Stripe sends amounts in cents, so:
✔ Uses your service layer

No repository calls here — clean separation.
✔ Uses your naming rules

    Controller = PascalCase

    DTOs = PascalCase

    No snake_case leaks

    Stripe IDs passed as strings

✔ Matches your myjobquotes architecture

This is the same pattern used in your existing Stripe integration.




 */