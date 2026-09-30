using System;
using System.Threading.Tasks;
using TradePlatform.Api.DTOs.Chat;
using TradePlatform.Api.DTOs.Invoices;
using TradePlatform.Api.DTOs.Jobs;
using TradePlatform.Api.DTOs.subscription;
using TradePlatform.Api.Enums;
using TradePlatform.Api.Models.Email;

namespace TradePlatform.Api.Services.Emails;

public interface IEmailService
{
	Task SendAsync(string to, string subject, string body);

	Task SendJobPostedEmail(JobPostEmailNotifyDto job_email_notify);

	Task SendJobPuchasedEmail(Guid user_id, PurchaseJobResultDto pjob_res);

	Task SendJobPurchaseChatNotificationEmailAsync(MessageEmailNotifyReq msg_email_req);

	Task SendJobDisputeChatNotificationEmailAsync(MessageEmailNotifyReq msg_email_req);

	Task NotificationEmailForReviewRequested(Guid review_request_id);

	Task NotificationEmailForReviewPosted(Guid review_id);

	Task NotificationEmailForReviewReplied(Guid review_id, int review_reply_id);

	Task NotificationEmailForJobDisputeReceived(NotifyEmailDataModel notify_email);

	Task NotificationEmailForJobDisputeAction(NotifyEmailDataModel notify_email);

	Task NotificationEmailForInvoicePaidAction(InvEventEmailNotifyDto InvoicePaidNotify);

	Task NotificationEmailForSubscriptionAutoRenewOn(SubscriptionViewDto subscriptionData, EventType anyevent);

	Task NotificationEmailForSubscriptionAutoRenewOff(SubscriptionViewDto subscriptionData, EventType anyevent);

	Task NotificationEmailForSubscriptionUpdated(SubscriptionViewDto subscriptionData, EventType anyevent);

	Task NotificationEmailForSubscriptionCancelled(SubscriptionViewDto subscriptionData);

	Task NotificationEmailForScheduleConfirmation(subscriptionpending_view subscriptionSchedule, EventType anyevent);

	Task NotificationEmailForScheduleCancellation(subscriptionpending_view subscriptionSchedule, EventType anyevent);

	Task NotificationEmailForScheduleExpiring(subscriptionpending_view subscriptionSchedule, EventType anyevent);

	Task NotificationEmailForRefundSucceeded(Guid refund_id, Guid entity_id, int entity_type_id, EventType anyevent);
}
