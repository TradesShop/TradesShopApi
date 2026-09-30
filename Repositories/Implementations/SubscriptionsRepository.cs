using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using TradePlatform.Api.Data;
using TradePlatform.Api.DTOs.Common;
using TradePlatform.Api.DTOs.subscription;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services;

namespace TradePlatform.Api.Repositories.Implementations;

public class SubscriptionsRepository : ISubscriptionsRepository
{
	private readonly DapperContext _context;

	private readonly IIdentityService _identityService;

	public SubscriptionsRepository(DapperContext context, IIdentityService identityService)
	{
		_context = context;
		_identityService = identityService;
	}

	public async Task SubscriptionCancelAsync(SubscriptionEventProcessDto model)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		DynamicParameters parameters = new DynamicParameters();
		parameters.Add("@subscription_id", model.subscription_id);
		parameters.Add("@stripe_subscription_id", model.stripe_subscription_id);
		parameters.Add("@current_period_start", model.current_period_start);
		parameters.Add("@current_period_end", model.current_period_end);
		parameters.Add("@stripe_event_id", model.stripe_event_id);
		parameters.Add("@metadata_json", model.metadata_json);
		parameters.Add("@event_type", model.event_type);
		parameters.Add("@actor", model.actor);
		parameters.Add("@source", model.source);
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("[dbo].[usp_subscription_cancel]", parameters, null, null, commandType);
	}

	public async Task SubscriptionUpsertAsync(SubscriptionEventProcessDto model)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		DynamicParameters parameters = new DynamicParameters();
		parameters.Add("@subscription_id", model.subscription_id);
		parameters.Add("@stripe_subscription_id", model.stripe_subscription_id);
		parameters.Add("@plan_price_id", model.plan_price_id);
		parameters.Add("@status", model.status);
		parameters.Add("@user_id", model.user_id);
		parameters.Add("@trial_start", model.trial_start);
		parameters.Add("@trial_end", model.trial_end);
		parameters.Add("@current_period_start", model.current_period_start);
		parameters.Add("@current_period_end", model.current_period_end);
		parameters.Add("@billing_cycle_anchor", model.billing_cycle_anchor);
		parameters.Add("@cancel_at_period_end", model.cancel_at_period_end);
		parameters.Add("@metadata_json", model.metadata_json);
		parameters.Add("@stripe_event_id", model.stripe_event_id);
		parameters.Add("@event_type", model.event_type);
		parameters.Add("@actor", model.actor);
		parameters.Add("@source", model.source);
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("[dbo].[usp_subscription_upsert]", parameters, null, null, commandType);
	}

	public async Task SubscriptionEventProcessUpdateAsync(SubscriptionEventProcessDto model)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		DynamicParameters parameters = new DynamicParameters();
		parameters.Add("@subscription_id", model.subscription_id);
		parameters.Add("@stripe_subscription_id", model.stripe_subscription_id);
		parameters.Add("@user_id", model.user_id);
		parameters.Add("@plan_price_id", model.plan_price_id);
		parameters.Add("@current_period_start", model.current_period_start);
		parameters.Add("@current_period_end", model.current_period_end);
		parameters.Add("@status", model.status);
		parameters.Add("@cancel_at_period_end", model.cancel_at_period_end);
		parameters.Add("@billing_cycle_anchor", model.billing_cycle_anchor);
		parameters.Add("@trial_end", model.trial_end);
		parameters.Add("@metadata_json", model.metadata_json);
		parameters.Add("@stripe_event_id", model.stripe_event_id);
		parameters.Add("@event_type", model.event_type);
		parameters.Add("@actor", model.actor);
		parameters.Add("@source", model.source);
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("dbo.usp_subscription_event_process_update", parameters, null, null, commandType);
	}

	public async Task<SubscriptionViewDto?> GetActiveSubscriptionForUserAsync(Guid user_id, string plan_type)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { user_id, plan_type };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<SubscriptionViewDto>("usp_subscriptions_get_by_user", param, null, null, commandType);
	}

	public async Task<SubscriptionViewDto?> GetByStripeSubscriptionIdAsync(string stripe_subscriptionid)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new
		{
			stripe_subscription_id = stripe_subscriptionid
		};
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<SubscriptionViewDto>("usp_subscriptions_get_by_stripe_subscrition_id", param, null, null, commandType);
	}

	public async Task<IEnumerable<SubscriptionsListRes>> SubscriptionsListAsync(CommonSearchReq csreq)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new
		{
			user_id = csreq.target_user_id
		};
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryAsync<SubscriptionsListRes>("usp_subscriptions_list_for_user", param, null, null, commandType);
	}

	public async Task<SubscriptionsHistoryRes> SubscriptionsHistoryAsync(CommonSearchReq csreq)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new
		{
			user_id = csreq.target_user_id,
			page_number = csreq.page_number,
			page_size = csreq.page_size,
			sort_by = csreq.sort_by
		};
		CommandType? commandType = CommandType.StoredProcedure;
		SqlMapper.GridReader result = await conn.QueryMultipleAsync("usp_subscription_history_for_user", param, null, null, commandType);
		SubscriptionsHistoryRes subscriptions = new SubscriptionsHistoryRes();
		subscriptions.history = result.Read<SubscriptionsHistoryDto>().ToList();
		subscriptions.total_records = result.ReadSingle<int>();
		return subscriptions;
	}

	public async Task<Subscriptions> InsertSubscriptionAsync(Subscriptions model)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		DynamicParameters parameters = new DynamicParameters();
		parameters.Add("@user_id", model.user_id);
		parameters.Add("@plan_price_id", model.plan_price_id);
		parameters.Add("@status", model.status);
		parameters.Add("@current_period_start", model.current_period_start);
		parameters.Add("@current_period_end", model.current_period_end);
		parameters.Add("@auto_renew", model.auto_renew);
		parameters.Add("@stripe_customer_id", model.stripe_customer_id);
		parameters.Add("@stripe_subscription_id", model.stripe_subscription_id);
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstAsync<Subscriptions>("usp_subscriptions_insert", parameters, null, null, commandType);
	}

	public async Task<Subscriptions> SubscriptionUpdatePriceAsync(string stripe_subscription_id, string stripe_price_id, DateTime current_period_start, DateTime current_period_end)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { stripe_subscription_id, stripe_price_id, current_period_start, current_period_end };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<Subscriptions>("usp_subscriptions_update_price", param, null, null, commandType);
	}

	public async Task MarkActiveAsync(string stripe_subscription_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { stripe_subscription_id };
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("usp_subscriptions_mark_active", param, null, null, commandType);
	}

	public async Task MarkPastDueAsync(string stripe_subscription_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { stripe_subscription_id };
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("usp_subscriptions_mark_past_due", param, null, null, commandType);
	}

	public async Task MarkCanceledAsync(string stripe_subscription_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { stripe_subscription_id };
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("usp_subscriptions_mark_canceled", param, null, null, commandType);
	}

	public async Task UpdatePeriodAsync(string stripe_subscription_id, DateTime current_period_start, DateTime current_period_end, string status, bool cancel_at_period_end)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { stripe_subscription_id, current_period_start, current_period_end, status, cancel_at_period_end };
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("usp_subscriptions_update_period", param, null, null, commandType);
	}

	public async Task<subscriptionpending_view?> subscriptionpending_upsert_async(subscriptionpending_upsert spdto)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new
		{
			spdto.user_id, spdto.current_subscription_id, spdto.stripe_subscription_id, spdto.current_plan_price_id, spdto.new_plan_price_id, spdto.current_stripe_price_id, spdto.new_stripe_price_id, spdto.stripe_schedule_id, spdto.effective_date, spdto.status,
			spdto.created_by, spdto.actor
		};
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<subscriptionpending_view>("dbo.usp_subscription_pending_upsert", param, null, null, commandType);
	}

	public async Task<subscriptionpending_view?> subscriptionpending_update_async(subscriptionpending_upd spdto)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new
		{
			pending_id = spdto.pending_id,
			subscription_id = spdto.current_subscription_id,
			new_plan_price_id = spdto.new_plan_price_id,
			new_stripe_price_id = spdto.new_stripe_price_id,
			stripe_schedule_id = spdto.stripe_schedule_id,
			effective_date = spdto.effective_date,
			status = spdto.status,
			updated_by = spdto.updated_by
		};
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<subscriptionpending_view>("dbo.usp_subscription_pending_update", param, null, null, commandType);
	}

	public async Task<IEnumerable<subscriptionpending_view>> ScheduledSubscriptionsAllAsync(subscriptionpending_req scpreq)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { scpreq.search_mode, scpreq.user_id, scpreq.stripe_schedule_id, scpreq.pending_id, scpreq.current_subscription_id, scpreq.plan_type };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryAsync<subscriptionpending_view>("dbo.usp_subscriptionpending_get", param, null, null, commandType);
	}

	public async Task<subscriptionpending_view?> ScheduledSubscriptionsViewAsync(subscriptionpending_req scpreq)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { scpreq.search_mode, scpreq.user_id, scpreq.stripe_schedule_id, scpreq.pending_id, scpreq.current_subscription_id, scpreq.plan_type };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<subscriptionpending_view>("dbo.usp_subscriptionpending_get", param, null, null, commandType);
	}
}
