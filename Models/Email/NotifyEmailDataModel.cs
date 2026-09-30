using System;

namespace TradePlatform.Api.Models.Email;

public class NotifyEmailDataModel : ChatMessageDataModel
{
	public Guid job_id { get; set; }

	public Guid? job_purchase_id { get; set; }

	public Guid? job_dispute_id { get; set; }

	public string job_ref { get; set; }

	public Guid receiver_id { get; set; }

	public string receiver_name { get; set; }

	public string receiver_email { get; set; }

	public string receiver_phone { get; set; }

	public bool receive_important { get; set; } = true;

	public bool receive_promotional { get; set; } = true;

	public string? sender_name { get; set; }

	public string? sender_email { get; set; }

	public string? sender_phone { get; set; }

	public Guid? sender_id { get; set; }

	public string? business_name { get; set; }

	public string? slug { get; set; }

	public string? job_title { get; set; }

	public string? postcode { get; set; }

	public string? job_category { get; set; }

	public bool is_customer { get; set; }

	public DateTime? job_created_at { get; set; }

	public double? review_rating { get; set; }

	public string? review_title { get; set; }

	public string? review_comment { get; set; }

	public string? review_reply { get; set; }

	public DateTime? dispute_created_at { get; set; }

	public string? dispute_decision { get; set; }

	public int? refund_credits { get; set; }

	public string? resolution_notes { get; set; }
}
