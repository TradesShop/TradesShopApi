using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stripe;
using TradePlatform.Api.DTOs.Common;
using TradePlatform.Api.DTOs.subscription;
using TradePlatform.Api.Models.Plans;

namespace TradePlatform.Api.Services.Subscriptions;

public interface IUserSubscriptionService
{
	Dictionary<string, string> BuildAuditMetadata(Guid subsription_id, Guid user_id, PlanPriceByPriceId plan);
	Task SubscriptionCancelRequestAsync(SubscriptionCancelReqDto model);

    Task<SubscriptionViewDto> GetActiveSubscriptionForUserAsync(Guid user_id, string plan_type);

	Task<SubscriptionSelectResponse> SelectSubscriptionAsync(Guid user_id, Guid plan_id, Guid plan_price_id);

	Task<SubscriptionCancelResponse> CancelSubscriptionEndOfPeriod(Guid effective_userid, Guid plan_price_id, string stripe_subscription_id);

	Task<SubscriptionCancelResponse> CancelSubscriptionImmediately(Guid effective_userid, Guid plan_price_id, string stripe_subscription_id);

	Task<SubscriptionCancelResponse> CancelScheduledSubscriptionOnly(Guid effective_userid, Guid plan_price_id, string stripe_subscription_id);

	Task<IEnumerable<SubscriptionsListRes>> SubscriptionsListAsync(CommonSearchReq csreq);

	Task<SubscriptionsHistoryRes> SubscriptionsHistoryAsync(CommonSearchReq csreq);

    Task<subscriptionpending_view> subsctionpending_update_status(Guid? pending_id, string status, Guid? updated_by);

	Task<subscriptionpending_view> ScheduledSubscriptionsViewAsync(subscriptionpending_req spreq);

	Task<IEnumerable<subscriptionpending_view>> ScheduledSubscriptionsAllAsync(subscriptionpending_req scpreq);

	Task SyncStripeCustomerAddressAsync(Guid user_id, string stripe_customer_id);

	Task SaveSubscriptionCancelAsync(Subscription? stripeSubscription, Dictionary<string, string> metadata, Guid? subscription_id, Event? stripeEvent, string? actor = "user", string? source = "api");

	Task<SubscriptionCancelResponse> SetSubscriptionAutoRenewal(Guid effective_userid, Guid plan_price_id, string stripe_subscription_id);

    Task SaveSubscriptionUpsertAsync(Subscription? subscription, Guid user_id, Guid plan_price_id, Dictionary<string, string> metadata, Guid? subscription_id, Event? stripeEvent, string? action = "created", string? actor = "user", string? source = "api");
    Task SubscriptionUpdateFromStripeWebhook(Subscription? subscription, Guid user_id,Dictionary<string, string> metadata, Event? stripeEvent);

}
