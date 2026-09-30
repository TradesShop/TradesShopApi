namespace TradePlatform.Api.Services.Emails.Models;

public class NotifyEmailModel : EmailModelBase
{
	public string JobId { get; set; }

	public string JobRef { get; set; }

	public string ReceiverName { get; set; }

	public string SenderName { get; set; }

	public string SenderPhone { get; set; }

	public string SenderEmail { get; set; }

	public string JobTitle { get; set; }

	public string JobCategory { get; set; }

	public string? Postcode { get; set; }

	public string JobCreatedAt { get; set; }

	public string? BusinessName { get; set; }

	public new string? ViewProfileUrl { get; set; }

	public string PurchasedAt { get; set; }

	public bool is_customer { get; set; }

	public string? ReviewRating { get; set; }

	public string? ReviewTitle { get; set; }

	public string? ReviewComment { get; set; }

	public string? ReviewReply { get; set; }

	public bool IsJobPurchase { get; set; }

	public bool IsJobDispute { get; set; }

	public string MessageText { get; set; }

	public new string? ViewMyReviewUrl { get; set; }

	public string? DisputeDecision { get; set; }

	public string? DisputeCreatedAt { get; set; }

	public int RefundCredits { get; set; }

	public string? AdminComments { get; set; }
}
