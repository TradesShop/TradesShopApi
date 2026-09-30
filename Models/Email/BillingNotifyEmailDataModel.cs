using System;

namespace TradePlatform.Api.Models.Email;

public class BillingNotifyEmailDataModel
{
	public string receiver_name { get; set; }

	public string receiver_email { get; set; }

	public Guid receiver_id { get; set; }

	public string? invoice_number { get; set; }

	public string? purchase_type { get; set; }

	public string? billing_interval { get; set; }

	public string? slug { get; set; }

	public Guid? invoice_id { get; set; }

	public Guid? entity_id { get; set; }

	public int? entity_type_id { get; set; }

	public decimal? plan_price { get; set; }

	public string? plan_type { get; set; }

	public string? plan_name { get; set; }

	public DateTime? created_at { get; set; }

	public DateTime? cancelled_at { get; set; }

	public decimal? amount_paid { get; set; }

	public string? currency { get; set; }

	public string? status { get; set; }

	public DateTime? subscription_startdate { get; set; }

	public DateTime? subscription_enddate { get; set; }

	public bool auto_renew { get; set; }

	public int purchased_credits { get; set; }

	public int? purchased_categories { get; set; }

	public int? purchased_locations { get; set; }

	public int? total_credits { get; set; }

	public int? credit_adjusted { get; set; }

	public bool HasCreditsBenefit { get; set; }

	public bool HasLocationBenefit { get; set; }

	public bool HasCategoryBenefit { get; set; }

	public bool HasMembershipBenefit { get; set; }

	public bool IsSubscription { get; set; }

	public bool IsCreditBundle { get; set; }

	public decimal? new_plan_price { get; set; }

	public string? new_plan_type { get; set; }

	public string? new_plan_name { get; set; }

	public string? new_billing_interval { get; set; }

	public DateTime? effective_date { get; set; }
}
