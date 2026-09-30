using System;

namespace TradePlatform.Api.Models.Email;

public class ChatMessageDataModel
{
	public Guid? message_id { get; set; }

	public Guid? conversation_id { get; set; }

	public string? message_text { get; set; }

	public string? message_type { get; set; }

	public DateTime? created_at { get; set; }
}
