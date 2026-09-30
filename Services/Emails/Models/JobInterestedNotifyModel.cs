namespace TradePlatform.Api.Services.Emails.Models;

public class JobInterestedNotifyModel : EmailModelBase
{
	public string CustomerName { get; set; }

	public string CustomerEmail { get; set; }

	public string CustomerPhone { get; set; }

	public string TraderName { get; set; }

	public string TraderPhone { get; set; }

	public string TraderEmail { get; set; }

	public string JobTitle { get; set; }

	public string Postcode { get; set; }

	public string JobDescription { get; set; }

	public string JobPostedDate { get; set; }

	public string BusinessName { get; set; }

	public new string ViewProfileUrl { get; set; }

	public string PurchasedAt { get; set; }
}
