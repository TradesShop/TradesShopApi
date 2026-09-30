using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TradePlatform.Api.Enums;
using TradePlatform.Api.Models.Email;
using TradePlatform.Api.Services.Emails;

namespace TradePlatform.Api.Services.BackgroundEmailQueue;

public class EmailBackgroundService : BackgroundService
{
	private readonly IServiceProvider _serviceProvider;

	private readonly IBackgroundEmailQueue _queue;

	private readonly ILogger<EmailBackgroundService> _logger;

	public EmailBackgroundService(IServiceProvider serviceProvider, IBackgroundEmailQueue queue, ILogger<EmailBackgroundService> logger)
	{
		_serviceProvider = serviceProvider;
		_queue = queue;
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				EmailQueueItem item = await _queue.DequeueAsync(stoppingToken);
				using IServiceScope scope = _serviceProvider.CreateScope();
				IEmailService emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
				EventType t = item.EventType;
				if (t == EventType.JobPurchased)
				{
					await emailService.SendJobPuchasedEmail(item.UserId, item.PurchaseResult);
					continue;
				}
				EventType t2 = t;
				if (t2 == EventType.JobPosted)
				{
					await emailService.SendJobPostedEmail(item.JobPostEmailNotify);
					continue;
				}
				EventType t3 = t;
				if (t3 == EventType.JobPurchasedMessage || t3 == EventType.CommonMessage)
				{
					await emailService.SendJobPurchaseChatNotificationEmailAsync(item.ChatMessageNotifyReq);
					continue;
				}
				EventType t4 = t;
				if (t4 == EventType.ReviewRequested)
				{
					await emailService.NotificationEmailForReviewRequested(item.review_request_id);
					continue;
				}
				EventType t5 = t;
				if (t5 == EventType.ReviewPosted)
				{
					await emailService.NotificationEmailForReviewPosted(item.review_id);
					continue;
				}
				EventType t6 = t;
				if (t6 == EventType.ReviewReplied)
				{
					await emailService.NotificationEmailForReviewReplied(item.review_id, item.review_reply_id);
					continue;
				}
				EventType t7 = t;
				if (t7 == EventType.JobDisputeSubmit)
				{
					await emailService.NotificationEmailForJobDisputeReceived(item.NotifyEmailDataModel);
					continue;
				}
				EventType t8 = t;
				if (t8 == EventType.JobDisputedMessage)
				{
					await emailService.SendJobDisputeChatNotificationEmailAsync(item.ChatMessageNotifyReq);
					continue;
				}
				EventType t9 = t;
				if (t9 == EventType.JobDisputeAction)
				{
					await emailService.NotificationEmailForJobDisputeAction(item.NotifyEmailDataModel);
					continue;
				}
				EventType t10 = t;
				if (t10 == EventType.InvoicePaid)
				{
					await emailService.NotificationEmailForInvoicePaidAction(item.InvoicePaidNotify);
					continue;
				}
				EventType t11 = t;
				if (t11 == EventType.SubscriptionAutoNewOff)
				{
					await emailService.NotificationEmailForSubscriptionAutoRenewOff(item.SubscriptionData, EventType.SubscriptionAutoNewOff);
					continue;
				}
				EventType t12 = t;
				if (t12 == EventType.SubscriptionAutoNewOn)
				{
					await emailService.NotificationEmailForSubscriptionAutoRenewOn(item.SubscriptionData, EventType.SubscriptionAutoNewOn);
					continue;
				}
				EventType t13 = t;
				if (t13 == EventType.SubscriptionUpdated)
				{
					await emailService.NotificationEmailForSubscriptionUpdated(item.SubscriptionData, EventType.SubscriptionUpdated);
					continue;
				}
				EventType t14 = t;
				if (t14 == EventType.SubscriptionCancelled)
				{
					await emailService.NotificationEmailForSubscriptionCancelled(item.SubscriptionData);
					continue;
				}
				EventType t15 = t;
				if (t15 == EventType.SubscriptionScheduleCreated)
				{
					await emailService.NotificationEmailForScheduleConfirmation(item.ScheduledSubscription, EventType.SubscriptionScheduleCreated);
					continue;
				}
				EventType t16 = t;
				if (t16 == EventType.SubscriptionScheduleUpdated)
				{
					await emailService.NotificationEmailForScheduleConfirmation(item.ScheduledSubscription, EventType.SubscriptionScheduleUpdated);
					continue;
				}
				EventType t17 = t;
				if (t17 == EventType.SubscriptionScheduleCancelled)
				{
					await emailService.NotificationEmailForScheduleCancellation(item.ScheduledSubscription, EventType.SubscriptionScheduleUpdated);
					continue;
				}
				EventType t18 = t;
				if (t18 == EventType.SubscriptionScheduleExpiring)
				{
					await emailService.NotificationEmailForScheduleExpiring(item.ScheduledSubscription, EventType.SubscriptionScheduleExpiring);
					continue;
				}
				EventType t19 = t;
				if (t19 == EventType.RefundSucceeded)
				{
					await emailService.NotificationEmailForRefundSucceeded(item.RefundId, item.EntityId, item.EntityTypeId, EventType.RefundSucceeded);
					continue;
				}
				_logger.LogWarning("Unknown event type: {EventType}", item.EventType.Code);
			}
			catch (OperationCanceledException)
			{
				break;
			}
			catch (Exception exception)
			{
				_logger.LogError(exception, "Error sending queued email.");
			}
		}
	}
}
