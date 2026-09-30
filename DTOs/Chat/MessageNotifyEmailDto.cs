using System;

namespace TradePlatform.Api.DTOs.Chat;

public class MessageNotifyEmailDto
{
	public Guid? message_id { get; set; }

	public Guid? conversation_id { get; set; }

	public string? message_text { get; set; }

	public string? message_type { get; set; }

	public DateTime? created_at { get; set; }

	public string? sender_name { get; set; }

	public string? sender_email { get; set; }

	public string? receiver_name { get; set; }

	public string? receiver_email { get; set; }

	public Guid? job_purchase_id { get; set; }

	public Guid job_id { get; set; }

	public Guid receiver_user_id { get; set; }

	public bool is_customer { get; set; }

	public string? job_category { get; set; }

	public string? job_title { get; set; }

	public DateTime? job_created_at { get; set; }

	public string business_name { get; set; }
}
