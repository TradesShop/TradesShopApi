namespace TradePlatform.Api.Services.Emails.Models;

public class NotifyBillingEmailModel : EmailModelBase
{
	public string? InvoiceNumber { get; set; }

	public string ReceiverName { get; set; }

	public string? PurchaseType { get; set; }

	public string? PlanPrice { get; set; }

	public string PlanType { get; set; }

	public string PlanName { get; set; }

	public string? CreatedDate { get; set; }

	public string? CancellationDate { get; set; }

	public string? Currency { get; set; }

	public string? AmountPaid { get; set; }

	public string? BillingPeriod { get; set; }

	public string? SubscriptionStartDate { get; set; }

	public string? SubscriptionEndDate { get; set; }

	public bool? IsAutoRenew { get; set; }

	public int? CreditsAdded { get; set; }

	public int? TotalCredits { get; set; }

	public int? CreditsAdjusted { get; set; }

	public bool HasMembershipBenefit { get; set; }

	public bool HasCreditsBenefit { get; set; }

	public bool HasLocationBenefit { get; set; }

	public bool HasCategoryBenefit { get; set; }

	public bool IsSubscription { get; set; }

	public bool IsCreditBundle { get; set; }

	public string? Status { get; set; }

	public string? EffectiveDate { get; set; }

	public string? NewPlanPrice { get; set; }

	public string? NewPlanType { get; set; }

	public string? NewPlanName { get; set; }

	public string? NewBillingPeriod { get; set; }
}
