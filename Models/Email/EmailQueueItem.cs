using System;
using TradePlatform.Api.DTOs.Chat;
using TradePlatform.Api.DTOs.Invoices;
using TradePlatform.Api.DTOs.Jobs;
using TradePlatform.Api.DTOs.subscription;
using TradePlatform.Api.Enums;

namespace TradePlatform.Api.Models.Email;

public class EmailQueueItem
{
	public EventType EventType { get; set; }

	public Guid UserId { get; set; }

	public JobPostEmailNotifyDto? JobPostEmailNotify { get; set; }

	public PurchaseJobResultDto? PurchaseResult { get; set; }

	public MessageEmailNotifyReq? ChatMessageNotifyReq { get; set; }

	public NotifyEmailDataModel? NotifyEmailDataModel { get; set; }

	public InvEventEmailNotifyDto? InvoicePaidNotify { get; set; }

	public SubscriptionViewDto? SubscriptionData { get; set; }

	public subscriptionpending_view? ScheduledSubscription { get; set; }

	public Guid review_request_id { get; set; }

	public Guid review_id { get; set; }

	public int review_reply_id { get; set; }

	public Guid JobId { get; set; }

	public Guid RefundId { get; set; }

	public Guid EntityId { get; set; }

	public int EntityTypeId { get; set; }
}
