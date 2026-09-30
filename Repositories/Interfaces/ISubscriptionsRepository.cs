using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradePlatform.Api.DTOs.Common;
using TradePlatform.Api.DTOs.subscription;
using TradePlatform.Api.Models;

namespace TradePlatform.Api.Repositories.Interfaces;

public interface ISubscriptionsRepository
{
	Task<SubscriptionViewDto?> GetActiveSubscriptionForUserAsync(Guid user_id, string plan_type);

	Task SubscriptionEventProcessUpdateAsync(SubscriptionEventProcessDto model);

	Task<SubscriptionViewDto?> GetByStripeSubscriptionIdAsync(string stripe_subscriptionid);

	Task<Subscriptions> SubscriptionUpdatePriceAsync(string stripe_subscription_id, string stripe_price_id, DateTime current_period_start, DateTime current_period_end);

	Task<IEnumerable<SubscriptionsListRes>> SubscriptionsListAsync(CommonSearchReq csreq);

	Task<SubscriptionsHistoryRes> SubscriptionsHistoryAsync(CommonSearchReq csreq);

	Task MarkActiveAsync(string stripe_subscription_id);

	Task MarkPastDueAsync(string stripe_subscription_id);

	Task MarkCanceledAsync(string stripe_subscription_id);

	Task UpdatePeriodAsync(string stripe_subscription_id, DateTime start, DateTime end, string status, bool cancelAtPeriodEnd);

	Task<subscriptionpending_view?> subscriptionpending_update_async(subscriptionpending_upd spdto);

	Task<subscriptionpending_view?> subscriptionpending_upsert_async(subscriptionpending_upsert spdto);

	Task<IEnumerable<subscriptionpending_view>> ScheduledSubscriptionsAllAsync(subscriptionpending_req scpreq);

	Task<subscriptionpending_view?> ScheduledSubscriptionsViewAsync(subscriptionpending_req scpreq);

	Task SubscriptionUpsertAsync(SubscriptionEventProcessDto model);

	Task SubscriptionCancelAsync(SubscriptionEventProcessDto model);
}
