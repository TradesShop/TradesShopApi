using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TradePlatform.Api.DTOs.Chat;
using TradePlatform.Api.DTOs.Invoices;
using TradePlatform.Api.DTOs.Jobs;
using TradePlatform.Api.DTOs.subscription;
using TradePlatform.Api.Enums;
using TradePlatform.Api.Models;
using TradePlatform.Api.Models.Email;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services.Emails.Models;
using TradePlatform.Api.Services.MagicUrl;

namespace TradePlatform.Api.Services.Emails;

public class SmtpEmailService : IEmailService
{
	private readonly EmailSettings _settings;

	private readonly IMagicUrlService _magicUrlService;

	private readonly IEmailTemplateService _emailTemplateService;

	private readonly IEmailSender _emailSender;

	private readonly IChatRepository _chatRepo;

	private readonly IJobsRepository _jobsRepo;

	private readonly IReviewsRepository _reviewRepo;

	private readonly ILogger<SmtpEmailService> _logger;

	private readonly ITdsInvoiceRepository _invoiceRepo;

	private readonly IRefundRepository _refundRepo;

	public SmtpEmailService(IOptions<EmailSettings> settings, 
		IMagicUrlService magicUrlService, 
		IEmailTemplateService emailTemplateService, 
		IEmailSender emailSender, 
		IChatRepository chatRepo, 
		IJobsRepository jobsRepo, 
		IReviewsRepository reviewRepo, 
		ILogger<SmtpEmailService> logger, 
		ITdsInvoiceRepository invoiceRepo, 
		IRefundRepository refundRepo)
	{
		_settings = settings.Value;
		_magicUrlService = magicUrlService;
		_emailTemplateService = emailTemplateService;
		_emailSender = emailSender;
		_chatRepo = chatRepo;
		_jobsRepo = jobsRepo;
		_logger = logger;
		_reviewRepo = reviewRepo;
		_invoiceRepo = invoiceRepo;
		_refundRepo = refundRepo;
	}

	public static string FormatEmailDate(DateTime? dt)
	{
		return dt?.ToString("dd MMMM yyyy 'at' HH:mm") ?? "";
	}

	public static string FormatEmailDateOnly(DateTime? dt)
	{
		return dt?.ToString("dd MMMM yyyy") ?? "";
	}

	public static string TruncateWords(string text, int maxLength)
	{
		if (string.IsNullOrEmpty(text))
		{
			return "";
		}
		if (text.Length <= maxLength)
		{
			return text;
		}
		string truncated = text.Substring(0, maxLength);
		int lastSpace = truncated.LastIndexOf(' ');
		if (lastSpace > 0)
		{
			truncated = truncated.Substring(0, lastSpace);
		}
		return truncated + "...";
	}

	public async Task SendAsync(string to, string subject, string body)
	{
		using SmtpClient client = new SmtpClient(_settings.Host)
		{
			Port = _settings.Port,
			Credentials = new NetworkCredential(_settings.Username, _settings.Password),
			EnableSsl = true
		};
		MailMessage mail = new MailMessage(_settings.From, to, subject, body);
		mail.IsBodyHtml = true;
		await client.SendMailAsync(mail);
	}

	public async Task SendJobPostedEmail(JobPostEmailNotifyDto job_email_notify)
	{
		foreach (TraderDto anytrader in await _jobsRepo.JobPostNotifyEmailDetailsForTraders(job_email_notify.job_id))
		{
			string emailtemplate = "JobPostedTraderNotify.html";
			EmailModelBase turls = _magicUrlService.Build(anytrader.trader_id, anytrader.email, job_email_notify.job_id, is_customer: false, emailtemplate);
			string redirectPath = $"/my-account/jobs?jobid={anytrader.PostedJob?.job_id}&tab=new";
			anytrader.PostedJob.job_url = turls.BaseMagicUrl + "&redirect=" + Uri.EscapeDataString(redirectPath);
			foreach (RecentJobDto anyrecentjob in anytrader.RecentJobs)
			{
				string redirectUrl = $"/my-account/jobs?jobid={anyrecentjob?.job_id}&tab=new";
				anyrecentjob.job_url = turls.BaseMagicUrl + "&redirect=" + Uri.EscapeDataString(redirectUrl);
			}
			JobPostedTraderNotifyModel tmodel = new JobPostedTraderNotifyModel
			{
				TraderName = anytrader.firstname,
				PostedJob = anytrader.PostedJob,
				RecentJobs = anytrader.RecentJobs,
				BaseMagicUrl = turls.BaseMagicUrl,
				ManageJobUrl = turls.ManageJobUrl,
				PostJobUrl = turls.PostJobUrl,
				AccountUrl = turls.AccountUrl,
				SettingsUrl = turls.SettingsUrl,
				MyJobsUrl = turls.MyJobsUrl,
				HomeUrl = turls.HomeUrl,
				UnsubscribeUrl = turls.UnsubscribeUrl
			};
			string thtml = await _emailTemplateService.RenderAsync(emailtemplate, tmodel);
			await _emailSender.SendAsync(anytrader.email, "New Job Posted Near You - " + anytrader.PostedJob?.title, thtml);
		}
		string anytemplate = "JobPostedCustomerNotify.html";
		EmailModelBase curls = _magicUrlService.Build(job_email_notify.user_id, job_email_notify.customer_email, job_email_notify.job_id, is_customer: true, anytemplate);
		JobPostedCustomerNotifyModel cmodel = new JobPostedCustomerNotifyModel
		{
			CustomerName = job_email_notify.customer_name,
			ManageJobUrl = curls.ManageJobUrl,
			PostJobUrl = curls.PostJobUrl,
			CloseYourJobUrl = curls.ManageJobUrl,
			AccountUrl = curls.AccountUrl,
			MyJobsUrl = curls.MyJobsUrl,
			SettingsUrl = curls.SettingsUrl,
			HomeUrl = curls.HomeUrl,
			UnsubscribeUrl = curls.UnsubscribeUrl
		};
		string chtml = await _emailTemplateService.RenderAsync(anytemplate, cmodel);
		await _emailSender.SendAsync(job_email_notify.customer_email, "Thanks for Posting Your Job", chtml);
	}

	private async Task SendEmailAsync(NotifyEmailDataModel notifyEmail, EntityType entityType, EventType eventType, string templateName, string subject, bool isCustomer = true)
	{
		if (!notifyEmail.receive_important)
		{
			return;
		}
		EmailModelBase urls = _magicUrlService.BuildUrl(notifyEmail, entityType, eventType, isCustomer, templateName);
		NotifyEmailModel model = new NotifyEmailModel
		{
			ReceiverName = notifyEmail.receiver_name,
			SenderName = (notifyEmail.sender_name ?? ""),
			SenderPhone = (notifyEmail.sender_phone ?? ""),
			SenderEmail = (notifyEmail.sender_email ?? ""),
			BusinessName = (notifyEmail.business_name ?? ""),
			JobCategory = (notifyEmail.job_category ?? ""),
			JobTitle = (notifyEmail.job_title ?? ""),
			Postcode = (notifyEmail.postcode ?? ""),
			JobCreatedAt = FormatEmailDate(notifyEmail.job_created_at),
			MessageText = (notifyEmail.message_text ?? ""),
			JobId = notifyEmail.job_id.ToString(),
			JobRef = notifyEmail.job_ref,
			ManageJobUrl = urls.ManageJobUrl,
			PostJobUrl = urls.PostJobUrl,
			CloseYourJobUrl = urls.ManageJobUrl,
			AccountUrl = urls.AccountUrl,
			MyJobsUrl = urls.MyJobsUrl,
			SettingsUrl = urls.SettingsUrl,
			HomeUrl = urls.HomeUrl,
			UnsubscribeUrl = urls.UnsubscribeUrl,
			is_customer = isCustomer,
			ViewProfileUrl = urls.ViewProfileUrl,
			ContactUsUrl = urls.ContactUsUrl,
			ConversationUrl = urls.ConversationUrl,
			ViewMyReviewUrl = urls.ViewMyReviewUrl
		};
		if (entityType == EntityType.JobReview)
		{
			model.ReviewRating = HtmlRatingRenderer.RenderRatingStarsHtml(notifyEmail.review_rating.GetValueOrDefault());
			model.ReviewTitle = notifyEmail.review_title ?? "";
			model.ReviewComment = TruncateWords(notifyEmail.review_comment ?? "", 50);
			model.ReviewReply = TruncateWords(notifyEmail.review_reply ?? "", 25);
		}
		else
		{
			EntityType t = entityType;
			if (t == EntityType.JobPurchase)
			{
				model.IsJobPurchase = true;
			}
			else
			{
				EntityType t2 = entityType;
				if (t2 == EntityType.JobDispute)
				{
					model.IsJobDispute = true;
					model.DisputeDecision = notifyEmail.dispute_decision ?? "";
					model.DisputeCreatedAt = FormatEmailDate(notifyEmail.dispute_created_at);
					model.RefundCredits = notifyEmail.refund_credits.GetValueOrDefault();
					model.AdminComments = notifyEmail.resolution_notes ?? "";
				}
			}
		}
		string html = await _emailTemplateService.RenderAsync(templateName, model);
		await _emailSender.SendAsync(notifyEmail.receiver_email, subject, html);
	}

	private async Task SendBillingEmailAsync(BillingNotifyEmailDataModel notifyEmail, EntityType anyEntity, EventType eventType, string templateName, string subject)
	{
		EmailModelBase urls = _magicUrlService.BuildBillingUrl(notifyEmail, anyEntity, eventType, templateName);
		NotifyBillingEmailModel model = new NotifyBillingEmailModel
		{
			ReceiverName = notifyEmail.receiver_name,
			PurchaseType = notifyEmail.purchase_type,
			PlanPrice = CurrencyHelper.FormatAmount(notifyEmail.plan_price, notifyEmail.currency),
			PlanType = notifyEmail.plan_type,
			PlanName = notifyEmail.plan_name,
			NewPlanPrice = CurrencyHelper.FormatAmount(notifyEmail.new_plan_price, notifyEmail.currency),
			NewPlanType = notifyEmail.new_plan_type,
			NewPlanName = notifyEmail.new_plan_name,
			EffectiveDate = FormatEmailDate(notifyEmail.effective_date),
			NewBillingPeriod = notifyEmail.new_billing_interval,
			CreatedDate = FormatEmailDate(notifyEmail.created_at),
			CancellationDate = FormatEmailDate(notifyEmail.cancelled_at),
			AmountPaid = CurrencyHelper.FormatAmount(notifyEmail.amount_paid, notifyEmail.currency),
			BillingPeriod = notifyEmail.billing_interval,
			SubscriptionStartDate = FormatEmailDate(notifyEmail.subscription_startdate),
			SubscriptionEndDate = FormatEmailDate(notifyEmail.subscription_enddate),
			IsAutoRenew = notifyEmail.auto_renew,
			CreditsAdded = notifyEmail.purchased_credits,
			TotalCredits = notifyEmail.total_credits,
			HasCreditsBenefit = notifyEmail.HasCreditsBenefit,
			HasLocationBenefit = notifyEmail.HasLocationBenefit,
			HasCategoryBenefit = notifyEmail.HasCategoryBenefit,
			HasMembershipBenefit = notifyEmail.HasMembershipBenefit,
			Status = notifyEmail.status,
			CreditsAdjusted = notifyEmail.credit_adjusted,
			IsSubscription = notifyEmail.IsSubscription,
			IsCreditBundle = notifyEmail.IsCreditBundle,
			AccountUrl = urls.AccountUrl,
			SubscriptionUrl = urls.SubscriptionUrl,
			ContactUsUrl = urls.ContactUsUrl,
			SettingsUrl = urls.SettingsUrl,
			HomeUrl = urls.HomeUrl,
			UnsubscribeUrl = urls.UnsubscribeUrl,
			ViewProfileUrl = urls.ViewProfileUrl,
			ViewMyReviewUrl = urls.ViewMyReviewUrl
		};
		string html = await _emailTemplateService.RenderAsync(templateName, model);
		await _emailSender.SendAsync(notifyEmail.receiver_email, subject, html);
	}

	public async Task SendJobPuchasedEmail(Guid user_id, PurchaseJobResultDto pjob_res)
	{
		NotifyEmailDataModel notify_email = await _jobsRepo.JobPurchaseNotifyEmailDetails(pjob_res);
		if (notify_email == null || string.IsNullOrEmpty(notify_email.receiver_email))
		{
			_logger.LogInformation("JobPurchaseNotifyEmailDetails returned null. UserId: {user_id}, JobId: {job_id},JobPurchaseId:{job_purcghaseid}", user_id, pjob_res?.job_id, pjob_res?.job_purchase_id);
			return;
		}
		string subject = "[New Tradesperson] " + notify_email.business_name + " Are Interested in Your Job";
		await SendEmailAsync(notify_email, EntityType.JobPurchase, EventType.JobPurchased, "job_purchase_owner_notification.html", subject);
		string customerName = notify_email.receiver_name;
		string customerEmail = notify_email.receiver_email;
		string customerPhone = notify_email.receiver_phone;
		notify_email.receive_important = true;
		notify_email.receiver_id = user_id;
		notify_email.receiver_name = notify_email.sender_name;
		notify_email.receiver_email = notify_email.sender_email;
		notify_email.receiver_phone = notify_email.sender_phone;
		notify_email.sender_name = customerName;
		notify_email.sender_email = customerEmail;
		notify_email.sender_phone = customerPhone;
		string tsubject = "Job Accepted Confirmation - " + notify_email.job_title;
		await SendEmailAsync(notify_email, EntityType.JobPurchase, EventType.JobPurchased, "job_purchase_confirmation.html", tsubject, isCustomer: false);
	}

	public async Task SendJobPurchaseChatNotificationEmailAsync(MessageEmailNotifyReq msg_email_req)
	{
		NotifyEmailDataModel notify_email = await _chatRepo.message_notification_email_details(msg_email_req);
		string subject = (notify_email.is_customer ? (notify_email.sender_name + " from " + notify_email.business_name + " has sent you a new message") : ("Customer - " + notify_email.sender_name + " has sent you a new message"));
		await SendEmailAsync(notify_email, EntityType.JobPurchase, EventType.JobPurchasedMessage, "message_sent_notification.html", subject, notify_email.is_customer);
	}

	public async Task SendJobDisputeChatNotificationEmailAsync(MessageEmailNotifyReq msg_email_req)
	{
		bool isAdmin = msg_email_req.recipient_user_type == 9;
		NotifyEmailDataModel notify_email = await _chatRepo.message_notification_email_details(msg_email_req);
		string subject = (isAdmin ? (notify_email.sender_name + " from " + notify_email.business_name + " has sent you a new message") : ("MyTradesShop team - " + notify_email.sender_name + " has sent you a new message"));
		await SendEmailAsync(notify_email, EntityType.JobDispute, EventType.JobDisputedMessage, "message_sent_notification.html", subject, isCustomer: false);
	}

	public async Task NotificationEmailForReviewRequested(Guid review_request_id)
	{
		NotifyEmailDataModel notify_email = await _reviewRepo.notify_email_review_requested(review_request_id);
		string subject = $"MyTradesShop:{notify_email.sender_name} from {notify_email.business_name} has requested your feedback";
		await SendEmailAsync(notify_email, EntityType.JobReview, EventType.ReviewRequested, "NotifyEmailReviewRequested.html", subject);
	}

	public async Task NotificationEmailForReviewPosted(Guid review_id)
	{
		NotifyEmailDataModel notify_email = await _reviewRepo.notify_email_review_posted(review_id);
		string subject = "MyTradesShop: You've Received a New Review from " + notify_email.sender_name;
		await SendEmailAsync(notify_email, EntityType.JobReview, EventType.ReviewPosted, "NotifyEmailReviewPosted.html", subject, isCustomer: false);
	}

	public async Task NotificationEmailForReviewReplied(Guid review_id, int review_reply_id)
	{
		NotifyEmailDataModel notify_email = await _reviewRepo.notify_email_review_replied(review_id, review_reply_id);
		string subject = $"MyTradesShop:{notify_email.sender_name} from {notify_email.business_name} replied to your review";
		await SendEmailAsync(notify_email, EntityType.JobReview, EventType.ReviewReplied, "NotifyEmailReviewReplied.html", subject);
	}

	public async Task NotificationEmailForJobDisputeReceived(NotifyEmailDataModel notify_email)
	{
		string subject = "MyTradesShop:Update regarding your dispute request";
		await SendEmailAsync(notify_email, EntityType.JobDispute, EventType.JobDisputeSubmit, "JobDisputeReceivedNotify.html", subject, isCustomer: false);
	}

	public async Task NotificationEmailForJobDisputeAction(NotifyEmailDataModel notify_email)
	{
		string subject = "MyTradesShop:Update regarding your dispute request";
		await SendEmailAsync(notify_email, EntityType.JobDispute, EventType.JobDisputeAction, "JobDisputeFinalDecision.html", subject, isCustomer: false);
	}

	public async Task NotificationEmailForInvoicePaidAction(InvEventEmailNotifyDto InvoicePaidNotify)
	{
		BillingNotifyEmailDataModel notify_email = await _invoiceRepo.InvoicePaidNotifyEmailDetails(InvoicePaidNotify);
		notify_email.entity_id = notify_email.invoice_id;
		notify_email.entity_type_id = EntityType.Invoice.Id;
		string subject = "MyTradesShop:Payment Confirmed: Your purchase summary and account details";
		await SendBillingEmailAsync(notify_email, EntityType.Invoice, EventType.InvoicePaid, "SubscriptionCreditPurchaseConfirmation.html", subject);
	}

	private static BillingNotifyEmailDataModel MapToBillingNotifyEmailDataModel(SubscriptionViewDto dto)
	{
		return new BillingNotifyEmailDataModel
		{
			entity_id = dto.id,
			entity_type_id = EntityType.Subscription.Id,
			receiver_email = dto.receiver_email,
			receiver_id = dto.user_id,
			auto_renew = dto.auto_renew,
			plan_type = dto.plan_type,
			plan_name = dto.plan_name,
			billing_interval = dto.billing_interval,
			cancelled_at = dto.cancelled_at,
			subscription_startdate = dto.current_period_start,
			subscription_enddate = dto.current_period_end
		};
	}

	public async Task NotificationEmailForSubscriptionAutoRenewOn(SubscriptionViewDto subscriptionData, EventType anyevent)
	{
		BillingNotifyEmailDataModel notify_email = MapToBillingNotifyEmailDataModel(subscriptionData);
		string subject = "MyTradesShop:Subscription Updated: Auto-renewal settings confirmed";
		await SendBillingEmailAsync(notify_email, EntityType.Subscription, anyevent, "SubscriptionAutoRenewUpdated.html", subject);
	}

	public async Task NotificationEmailForSubscriptionAutoRenewOff(SubscriptionViewDto subscriptionData, EventType anyevent)
	{
		BillingNotifyEmailDataModel notify_email = MapToBillingNotifyEmailDataModel(subscriptionData);
		string subject = "MyTradesShop:Subscription Updated: Auto-renewal settings confirmed";
		await SendBillingEmailAsync(notify_email, EntityType.Subscription, anyevent, "SubscriptionAutoRenewUpdated.html", subject);
	}

	public async Task NotificationEmailForSubscriptionUpdated(SubscriptionViewDto subscriptionData, EventType anyevent)
	{
		BillingNotifyEmailDataModel notify_email = MapToBillingNotifyEmailDataModel(subscriptionData);
		string subject = "MyTradesShop:Subscription Updated: Auto-renewal settings confirmed";
		await SendBillingEmailAsync(notify_email, EntityType.Subscription, anyevent, "SubscriptionAutoRenewUpdated.html", subject);
	}

	public async Task NotificationEmailForSubscriptionCancelled(SubscriptionViewDto subscriptionData)
	{
		BillingNotifyEmailDataModel notify_email = MapToBillingNotifyEmailDataModel(subscriptionData);
		string subject = "MyTradesShop: Subscription Canceled: Confirmation of your plan cancellation";
		await SendBillingEmailAsync(notify_email, EntityType.Subscription, EventType.SubscriptionCancelled, "SubscriptionCancellationConfirmation.html", subject);
	}

	private static BillingNotifyEmailDataModel MapToScheduleNotifyEmailDataModel(subscriptionpending_view dto)
	{
		return new BillingNotifyEmailDataModel
		{
			entity_id = dto.pending_id,
			entity_type_id = EntityType.SubscriptionPending.Id,
			receiver_email = dto.receiver_email,
			receiver_id = dto.user_id,
			billing_interval = dto.current_billing_interval,
			plan_price = dto.current_plan_price,
			plan_type = dto.current_plan_type,
			plan_name = dto.current_plan_name,
			new_plan_price = dto.new_plan_price,
			new_plan_type = dto.new_plan_type,
			new_plan_name = dto.new_plan_name,
			new_billing_interval = dto.new_billing_interval,
			effective_date = dto.effective_date,
			subscription_startdate = dto.current_period_start,
			subscription_enddate = dto.current_period_end
		};
	}

	public async Task NotificationEmailForScheduleConfirmation(subscriptionpending_view subscriptionSchedule, EventType anyevent)
	{
		BillingNotifyEmailDataModel notify_email = MapToScheduleNotifyEmailDataModel(subscriptionSchedule);
		string subject = "MyTradesShop: Confirmation of your scheduled plan change";
		await SendBillingEmailAsync(notify_email, EntityType.SubscriptionPending, anyevent, "SubsScheduleChangeConfirmation.html", subject);
	}

	public async Task NotificationEmailForScheduleCancellation(subscriptionpending_view subscriptionSchedule, EventType anyevent)
	{
		BillingNotifyEmailDataModel notify_email = MapToScheduleNotifyEmailDataModel(subscriptionSchedule);
		string subject = "MyTradesShop: Confirmation of canceled subscription change";
		await SendBillingEmailAsync(notify_email, EntityType.SubscriptionPending, anyevent, "SubsScheduleCancelledConfirmation.html", subject);
	}

	public async Task NotificationEmailForScheduleExpiring(subscriptionpending_view subscriptionSchedule, EventType anyevent)
	{
		BillingNotifyEmailDataModel notify_email = MapToScheduleNotifyEmailDataModel(subscriptionSchedule);
		string subject = "MyTradesShop: Upcoming subscription plan change on " + FormatEmailDateOnly(notify_email.effective_date);
		await SendBillingEmailAsync(notify_email, EntityType.SubscriptionPending, anyevent, "SubsScheduleExpiringNotification.html", subject);
	}

	public async Task NotificationEmailForRefundSucceeded(Guid refund_id, Guid entity_id, int entity_type_id, EventType anyevent)
	{
		BillingNotifyEmailDataModel notify_email = await _refundRepo.RefundSucceededEmailDetails(refund_id, entity_id, entity_type_id);
		notify_email.entity_id = refund_id;
		notify_email.entity_type_id = EntityType.Refund.Id;
		string subject = "MyTradesShop:Refund Confirmation: Your refund has been processed";
		await SendBillingEmailAsync(notify_email, EntityType.Refund, EventType.RefundSucceeded, "SubscriptionCreditRefundConfirmation.html", subject);
	}
}
