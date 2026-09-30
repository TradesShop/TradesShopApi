using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Stripe;
using TradePlatform.Api.DTOs.TeleMetry;
using TradePlatform.Api.Exceptions;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Middleware;

public class ErrorHandlingMiddleware
{
	private readonly RequestDelegate _next;

	private readonly ILogger<ErrorHandlingMiddleware> _logger;

	private readonly IHostEnvironment _env;

	public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, IHostEnvironment env)
	{
		_next = next;
		_logger = logger;
		_env = env;
	}

	public async Task Invoke(HttpContext context, ITelemetryErrorsRepository telemetryRepo)
	{
		try
		{
			await _next(context);
		}
		catch (BusinessException ex)
		{
			await LogTelemetryAsync(telemetryRepo, context, ex, "BusinessException", "Warning", 400);
			await WriteBusinessError(context, ex);
		}
		catch (StripeException ex2)
		{
			await LogTelemetryAsync(telemetryRepo, context, ex2, "StripeException", "Error", 400);
			await WriteStripeError(context, ex2);
		}
		catch (UnauthorizedAccessException ex3)
		{
			await LogTelemetryAsync(telemetryRepo, context, ex3, "UnauthorizedAccessException", "Warning", 401);
			await WriteUnauthorizedError(context, ex3);
		}
		catch (Exception ex4)
		{
			await LogTelemetryAsync(telemetryRepo, context, ex4, "UnhandledException", "Critical", 500);
			await WriteServerError(context, ex4);
		}
	}

	private async Task LogTelemetryAsync(ITelemetryErrorsRepository telemetryRepo, HttpContext context, Exception ex, string errorType, string severity, int statusCode)
	{
		try
		{
			Guid? userId = null;
			string userIdClaim = context.User?.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
			if (Guid.TryParse(userIdClaim, out var parsedUserId))
			{
				userId = parsedUserId;
			}
			string rawTraceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
			string traceId = null;
			if (!string.IsNullOrWhiteSpace(rawTraceId))
			{
				string cleanedTrace = rawTraceId.Replace("-", "");
				traceId = cleanedTrace.Substring(0, Math.Min(cleanedTrace.Length, 32));
			}
			if (context.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim() == null)
			{
				context.Connection.RemoteIpAddress?.ToString();
			}
			telemetry_error errorLog = new telemetry_error
			{
				error_type = errorType,
				severity = severity,
				message = ex.Message,
				stacktrace = ex.StackTrace,
				url = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}",
				user_id = userId,
				service_name = "TradePlatform.Api",
				environment = _env.EnvironmentName,
				trace_id = traceId,
				http_method = context.Request.Method,
				status_code = statusCode,
				user_agent = context.Request.Headers["User-Agent"].ToString(),
				ip_address = context.Connection.RemoteIpAddress?.ToString(),
				device_info = null,
				additional_data = JsonSerializer.Serialize(new
				{
					ExceptionType = ex.GetType().FullName,
					InnerException = ex.InnerException?.ToString(),
					QueryParameters = context.Request.Query.ToDictionary((KeyValuePair<string, StringValues> k) => k.Key, (KeyValuePair<string, StringValues> v) => v.Value.ToString())
				})
			};
			await telemetryRepo.telemetry_error_insert_async(errorLog);
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Failed to persist error telemetry to database.");
		}
	}

	private async Task WriteUnauthorizedError(HttpContext context, Exception ex)
	{
		if (!context.Response.HasStarted)
		{
			context.Response.Clear();
			context.Response.StatusCode = 401;
			context.Response.ContentType = "application/json";
			await context.Response.WriteAsync(JsonSerializer.Serialize(new
			{
				success = false,
				message = ex.Message,
				type = "unauthorized"
			}));
		}
	}

	private async Task WriteBusinessError(HttpContext context, BusinessException ex)
	{
		_logger.LogWarning(ex, "Business validation error");
		if (!context.Response.HasStarted)
		{
			context.Response.Clear();
			context.Response.StatusCode = 400;
			context.Response.ContentType = "application/json";
			var payload = new
			{
				success = false,
				message = ex.Message,
				type = "business_error"
			};
			await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
		}
	}

	private async Task WriteStripeError(HttpContext context, StripeException ex)
	{
		_logger.LogError(ex, "Stripe error occurred");
		if (!context.Response.HasStarted)
		{
			context.Response.Clear();
			context.Response.StatusCode = 400;
			context.Response.ContentType = "application/json";
			var payload = new
			{
				success = false,
				message = ex.Message,
				type = "stripe_error"
			};
			await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
		}
	}

	private async Task WriteServerError(HttpContext context, Exception ex)
	{
		_logger.LogError(ex, "Unhandled server exception");
		if (!context.Response.HasStarted)
		{
			context.Response.Clear();
			context.Response.StatusCode = 500;
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
