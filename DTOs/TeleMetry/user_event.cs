using System;

namespace TradePlatform.Api.DTOs.TeleMetry;

public class user_event
{
	public Guid? user_id { get; set; }

	public int? entity_type_id { get; set; }

	public Guid? entity_id { get; set; }

	public int? event_type_id { get; set; }

	public int? event_action_id { get; set; }

	public string source { get; set; }

	public object payload { get; set; }
}
