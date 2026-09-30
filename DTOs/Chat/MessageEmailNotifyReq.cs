using System;

namespace TradePlatform.Api.DTOs.Chat;

public class MessageEmailNotifyReq
{
	public Guid? message_id { get; set; }

	public Guid? conversation_id { get; set; }

	public Guid sender_user_id { get; set; }

	public Guid recipient_user_id { get; set; }

	public int entity_type_id { get; set; }

	public int sender_user_type { get; set; }

	public int recipient_user_type { get; set; }
}
