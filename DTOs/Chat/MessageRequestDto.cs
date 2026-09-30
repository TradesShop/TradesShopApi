using System;

namespace TradePlatform.Api.DTOs.Chat;

public class MessageRequestDto
{
	public int entity_type_id { get; set; }

	public Guid entity_id { get; set; }

	public string message_text { get; set; }

	public Guid recipient_user_id { get; set; }

	public Guid sender_user_id { get; set; }

	public int sender_user_type { get; set; }

	public int recipient_user_type { get; set; }

	public bool has_attachments { get; set; }
}
