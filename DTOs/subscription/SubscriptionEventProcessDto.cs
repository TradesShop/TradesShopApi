using System;

namespace TradePlatform.Api.DTOs.subscription;

public class SubscriptionCancelReqDto
{
    public Guid id { get; set; }    
    public Guid? user_id { get; set; }
    public Guid? target_user_id { get; set; }
	public bool is_refund_requested { get; set; } = false;
    public string? cancellation_reason { get; set; }
    public string? actor { get; set; } = "user";
    public string? source { get; set; } = "web_portal";
    public string? metadata_json { get; set; }
    public string? stripe_event_id { get; set; }
}
    public class SubscriptionEventProcessDto
{
	public Guid? subscription_id { get; set; }

	public string? stripe_subscription_id { get; set; }
    public string? stripe_price_id { get; set; }

    public Guid user_id { get; set; }

	public Guid? plan_price_id { get; set; }

	public string status { get; set; }

	public DateTime? current_period_start { get; set; }

	public DateTime? current_period_end { get; set; }

	public bool? cancel_at_period_end { get; set; }

	public DateTime? canceled_at { get; set; }

	public DateTime? trial_start { get; set; }

	public DateTime? trial_end { get; set; }

	public string? cancellation_reason { get; set; }

	public DateTime? billing_cycle_anchor { get; set; }

	public string? event_type { get; set; }

	public string metadata_json { get; set; }

	public string? stripe_event_id { get; set; }

	public string actor { get; set; }

	public string source { get; set; }
}
