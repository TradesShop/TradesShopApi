using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Stripe;

namespace TradePlatform.Api.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (StripeException stripeEx)
            {
                await WriteStripeError(context, stripeEx);
            }
            catch (Exception ex)
            {
                await WriteServerError(context,ex);
            }
        }

        private async Task WriteStripeError(HttpContext context, StripeException ex)
        {
            _logger.LogError(ex, "Stripe error occurred");

            if (context.Response.HasStarted)
                return;

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            var payload = new
            {
                success = false,
                message = ex.Message,
                type = "stripe_error"
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }

        private async Task WriteServerError(HttpContext context, Exception ex)
        {
            _logger.LogError(ex, "Unhandled server exception");

            if (context.Response.HasStarted)
                return;

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var payload = new
            {
                success = false,
                message = ex.Message,
                type = "server_error"
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
    }
}
