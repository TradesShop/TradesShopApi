using System;

namespace TradePlatform.Api.DTOs.TeleMetry;

public class audit_event
{
	public Guid user_id { get; set; }

	public string? event_type_code { get; set; }

	public string entity_type_code { get; set; }

	public Guid entity_id { get; set; }

	public string description { get; set; }

	public string? ip_address { get; set; }

	public string? user_agent { get; set; }
}
