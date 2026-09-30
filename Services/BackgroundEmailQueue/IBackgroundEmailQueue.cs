using System;
using System.Threading;
using System.Threading.Tasks;
using TradePlatform.Api.DTOs.Chat;
using TradePlatform.Api.DTOs.Invoices;
using TradePlatform.Api.DTOs.Jobs;
using TradePlatform.Api.DTOs.subscription;
using TradePlatform.Api.Enums;
using TradePlatform.Api.Models.Email;

namespace TradePlatform.Api.Services.BackgroundEmailQueue;

public interface IBackgroundEmailQueue
{
	ValueTask<EmailQueueItem> DequeueAsync(CancellationToken cancellationToken);

	ValueTask QueuePurchaseEmailAsync(Guid userId, PurchaseJobResultDto purchaseResult);

	ValueTask QueueJobPostedEmailAsync(JobPostEmailNotifyDto JobPostEmailNotify);

	ValueTask QueueJobMessageEmailAsync(MessageEmailNotifyReq ChatMessageNotifyReq);

	ValueTask QueueNotifyEmailForReviewRequested(Guid review_request_id);

	ValueTask QueueNotifyEmailForReviewPosted(Guid review_id);

	ValueTask QueueNotifyEmailForReviewReplied(Guid review_id, int review_reply_id);

	ValueTask QueueNotifyEmailForJobDisputeReceived(NotifyEmailDataModel NotifyEmailDataModel);

	ValueTask QueueNotifyEmailForJobDisputeAction(NotifyEmailDataModel NotifyEmailDataModel);

	ValueTask QueueInvoicePaidSuccessfullEmail(InvEventEmailNotifyDto InvoicePaidNotify);

	ValueTask SendSubscriptionAutoRenewalOffEmailAsync(SubscriptionViewDto existingSubscription, EventType anyEvent);

	ValueTask SendSubscriptionAutoRenewalOnEmailAsync(SubscriptionViewDto existingSubscription, EventType anyEvent);

	ValueTask SendSubsCancellationConfirmationEmail(SubscriptionViewDto updatedSubscription);

	ValueTask SendScheduleConfirmationEmailAsync(subscriptionpending_view scheduledSubscription, EventType anyEvent);

	ValueTask SendScheduleCanceledEmailAsync(subscriptionpending_view scheduledSubscription, EventType anyEvent);

	ValueTask SendScheduleExpiringEmailAsync(subscriptionpending_view scheduledSubscription, EventType anyEvent);

	ValueTask SendRefundConfirmationEmailAsync(Guid refund_id, Guid entity_id, int entity_type_id);
}
