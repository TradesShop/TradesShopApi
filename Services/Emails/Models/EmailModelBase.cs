namespace TradePlatform.Api.Services.Emails.Models;

public class EmailModelBase
{
	public string BaseMagicUrl { get; set; }

	public string ManageJobUrl { get; set; }

	public string PostJobUrl { get; set; }

	public string AccountUrl { get; set; }

	public string SettingsUrl { get; set; }

	public string MyJobsUrl { get; set; }

	public string HomeUrl { get; set; }

	public string CloseYourJobUrl { get; set; }

	public string UnsubscribeUrl { get; set; }

	public string? SubscriptionUrl { get; set; }

	public string? ConversationUrl { get; set; }

	public string? ViewProfileUrl { get; set; }

	public string? ContactUsUrl { get; set; }

	public string ViewMyReviewUrl { get; set; }

	public string Content { get; set; }
}
