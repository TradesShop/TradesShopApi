using System;

namespace TradePlatform.Api.DTOs.TeleMetry;

public class telemetry_error
{
	public string error_type { get; set; }

	public string? severity { get; set; }

	public string message { get; set; }

	public string? stacktrace { get; set; }

	public string url { get; set; }

	public Guid? user_id { get; set; }

	public string? service_name { get; set; }

	public string? environment { get; set; }

	public string? trace_id { get; set; }

	public string? http_method { get; set; }

	public int? status_code { get; set; }

	public string? user_agent { get; set; }

	public string? ip_address { get; set; }

	public string? device_info { get; set; }

	public string? additional_data { get; set; }
}
