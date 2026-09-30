using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using TradePlatform.Api.DTOs.Chat;
using TradePlatform.Api.DTOs.Invoices;
using TradePlatform.Api.DTOs.Jobs;
using TradePlatform.Api.DTOs.subscription;
using TradePlatform.Api.Enums;
using TradePlatform.Api.Models.Email;
using TradePlatform.Api.Services.BackgroundEmailQueue;

public class BackgroundEmailQueue : IBackgroundEmailQueue
{
	private readonly Channel<EmailQueueItem> _queue = Channel.CreateUnbounded<EmailQueueItem>();

	public async ValueTask<EmailQueueItem> DequeueAsync(CancellationToken cancellationToken)
	{
		return await _queue.Reader.ReadAsync(cancellationToken);
	}

	public async ValueTask QueueJobPostedEmailAsync(JobPostEmailNotifyDto JobPostEmailNotify)
	{
		await _queue.Writer.WriteAsync(new EmailQueueItem
		{
			EventType = EventType.JobPosted,
			JobPostEmailNotify = JobPostEmailNotify
		});
	}

	public async ValueTask QueuePurchaseEmailAsync(Guid userId, PurchaseJobResultDto purchaseResult)
	{
		await _queue.Writer.WriteAsync(new EmailQueueItem
		{
			EventType = EventType.JobPurchased,
			UserId = userId,
			PurchaseResult = purchaseResult
		});
	}

	public async ValueTask QueueJobMessageEmailAsync(MessageEmailNotifyReq ChatMessageNotifyReq)
	{
		EventType eventType = ChatMessageNotifyReq.entity_type_id switch
		{
			14 => EventType.JobPurchasedMessage, 
			12 => EventType.JobDisputedMessage, 
			_ => EventType.CommonMessage, 
		};
		await _queue.Writer.WriteAsync(new EmailQueueItem
		{
			EventType = eventType,
			ChatMessageNotifyReq = ChatMessageNotifyReq
		});
	}

	public async ValueTask QueueNotifyEmailForReviewRequested(Guid review_request_id)
	{
		await _queue.Writer.WriteAsync(new EmailQueueItem
		{
			EventType = EventType.ReviewRequested,
			review_request_id = review_request_id
		});
	}

	public async ValueTask QueueNotifyEmailForReviewPosted(Guid review_id)
	{
		await _queue.Writer.WriteAsync(new EmailQueueItem
		{
			EventType = EventType.ReviewPosted,
			review_id = review_id
		});
	}

	public async ValueTask QueueNotifyEmailForReviewReplied(Guid review_id, int review_reply_id)
	{
		await _queue.Writer.WriteAsync(new EmailQueueItem
		{
			EventType = EventType.ReviewReplied,
			review_id = review_id,
			review_reply_id = review_reply_id
		});
	}

	public async ValueTask QueueNotifyEmailForJobDisputeReceived(NotifyEmailDataModel NotifyEmailDataModel)
	{
		await _queue.Writer.WriteAsync(new EmailQueueItem
		{
			EventType = EventType.JobDisputeSubmit,
			NotifyEmailDataModel = NotifyEmailDataModel
		});
	}

	public async ValueTask QueueNotifyEmailForJobDisputeAction(NotifyEmailDataModel NotifyEmailDataModel)
	{
		await _queue.Writer.WriteAsync(new EmailQueueItem
		{
			EventType = EventType.JobDisputeAction,
			NotifyEmailDataModel = NotifyEmailDataModel
		});
	}

	public async ValueTask QueueInvoicePaidSuccessfullEmail(InvEventEmailNotifyDto InvoicePaidNotify)
	{
		await _queue.Writer.WriteAsync(new EmailQueueItem
		{
			EventType = EventType.InvoicePaid,
			InvoicePaidNotify = InvoicePaidNotify
		});
	}

	public async ValueTask SendRefundConfirmationEmailAsync(Guid refund_id, Guid entity_id, int entity_type_id)
	{
		await _queue.Writer.WriteAsync(new EmailQueueItem
		{
			EventType = EventType.RefundSucceeded,
			RefundId = refund_id,
			EntityId = entity_id,
			EntityTypeId = entity_type_id
		});
	}

	public async ValueTask SendSubscriptionAutoRenewalOffEmailAsync(SubscriptionViewDto existingSubscription, EventType anyEvent)
	{
		await _queue.Writer.WriteAsync(new EmailQueueItem
		{
			EventType = anyEvent,
			SubscriptionData = existingSubscription
		});
	}

	public async ValueTask SendSubscriptionAutoRenewalOnEmailAsync(SubscriptionViewDto existingSubscription, EventType anyEvent)
	{
		await _queue.Writer.WriteAsync(new EmailQueueItem
		{
			EventType = anyEvent,
			SubscriptionData = existingSubscription
		});
	}

	public async ValueTask SendSubsCancellationConfirmationEmail(SubscriptionViewDto updatedSubscription)
	{
		await _queue.Writer.WriteAsync(new EmailQueueItem
		{
			EventType = EventType.SubscriptionCancelled,
			SubscriptionData = updatedSubscription
		});
	}

	public async ValueTask SendScheduleConfirmationEmailAsync(subscriptionpending_view scheduledSubscription, EventType anyEvent)
	{
		await _queue.Writer.WriteAsync(new EmailQueueItem
		{
			EventType = anyEvent,
			ScheduledSubscription = scheduledSubscription
		});
	}

	public async ValueTask SendScheduleCanceledEmailAsync(subscriptionpending_view scheduledSubscription, EventType anyEvent)
	{
		await _queue.Writer.WriteAsync(new EmailQueueItem
		{
			EventType = anyEvent,
			ScheduledSubscription = scheduledSubscription
		});
	}

	public async ValueTask SendScheduleExpiringEmailAsync(subscriptionpending_view scheduledSubscription, EventType anyEvent)
	{
		await _queue.Writer.WriteAsync(new EmailQueueItem
		{
			EventType = anyEvent,
			ScheduledSubscription = scheduledSubscription
		});
	}
}
