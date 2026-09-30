using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using TradePlatform.Api.Data;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services;

namespace TradePlatform.Api.Repositories.Implementations;

public class PaymentsRepository : IPaymentsRepository
{
	private readonly IStripeService _stripeService;

	private readonly DapperContext _context;

	public PaymentsRepository(IStripeService stripeService, DapperContext context)
	{
		_stripeService = stripeService;
		_context = context;
	}

	private void EnsureBillingAccess(UserType userType)
	{
		if (userType != UserType.tradesperson && userType != UserType.admin)
		{
			throw new Exception("Only tradesperson or admin users can access billing features");
		}
	}

	private Guid ResolveEffectiveUserId(Guid callerId, UserType callerType, Guid? targetUserId)
	{
		if (callerType == UserType.admin && targetUserId.HasValue)
		{
			return targetUserId.Value;
		}
		return callerId;
	}

	public async Task<string> CreateSetupIntentAsync(Guid callerId, UserType callerType, Guid? targetUserId)
	{
		EnsureBillingAccess(callerType);
		Guid effectiveUserId = ResolveEffectiveUserId(callerId, callerType, targetUserId);
		return await _stripeService.CreateSetupIntentAsync(effectiveUserId);
	}

	public async Task<object> AttachPaymentMethodAsync(Guid effectiveUserId, UserType callerType, string payment_method_id)
	{
		EnsureBillingAccess(callerType);
		return new
		{
			success = true,
			payment_method = await _stripeService.AttachPaymentMethodToCustomerAsync(effectiveUserId, payment_method_id)
		};
	}

	public async Task<object> SubscribeAsync(Guid callerId, UserType callerType, string priceId, string paymentMethodId, Guid? targetUserId)
	{
		EnsureBillingAccess(callerType);
		Guid effectiveUserId = ResolveEffectiveUserId(callerId, callerType, targetUserId);
		return new
		{
			success = true,
			subscription = await _stripeService.CreateOrUpdateSubscriptionAsync(effectiveUserId, priceId, paymentMethodId)
		};
	}

	public async Task CancelSubscriptionAsync(Guid callerId, UserType callerType, string stripe_subscription_id, Guid? targetUserId)
	{
		EnsureBillingAccess(callerType);
		ResolveEffectiveUserId(callerId, callerType, targetUserId);
		await _stripeService.CancelSubscriptionAsync(stripe_subscription_id);
	}

	public async Task MarkSucceededAsync(string stripe_payment_intent_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { stripe_payment_intent_id };
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("usp_payments_mark_succeeded", param, null, null, commandType);
	}

	public async Task MarkFailedAsync(string stripe_payment_intent_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { stripe_payment_intent_id };
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("usp_payments_mark_failed", param, null, null, commandType);
	}

	public async Task MarkRefundedAsync(Guid payment_id, decimal amount, string stripe_refund_id)
	{
		using IDbConnection connection = _context.CreateOpenConnection();
		await connection.ExecuteAsync("\r\n                UPDATE payments\r\n                SET \r\n                    status = 'refunded',\r\n                    refunded_amount = @amount,\r\n                    stripe_refund_id = @stripe_refund_id,\r\n                    refunded_at = NOW()\r\n                WHERE id = @payment_id;", new { payment_id, amount, stripe_refund_id });
	}

	public async Task<PaymentsM?> GetByIdAsync(Guid id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { id };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<PaymentsM>("usp_payments_get_by_id", param, null, null, commandType);
	}

	public async Task<IEnumerable<PaymentsM>> GetByInvoiceIdAsync(Guid invoice_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { invoice_id };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryAsync<PaymentsM>("usp_payments_get_by_invoice_id", param, null, null, commandType);
	}

	public async Task<IEnumerable<PaymentsM>> GetByUserIdAsync(Guid user_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { user_id };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryAsync<PaymentsM>("usp_payments_get_by_user_id", param, null, null, commandType);
	}

	public async Task<PaymentsM?> GetByStripePaymentIntentIdAsync(string stripe_payment_intent_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { stripe_payment_intent_id };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<PaymentsM>("usp_payments_get_by_stripe_payment_intent_id", param, null, null, commandType);
	}

	public async Task InsertPaymentAsync(PaymentsM payment)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { payment.user_id, payment.invoice_id, payment.stripe_payment_intent_id, payment.stripe_charge_id, payment.amount, payment.currency, payment.status };
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("usp_payments_insert", param, null, null, commandType);
	}

	public async Task CreateAsync(PaymentsM payment)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { payment.user_id, payment.invoice_id, payment.stripe_payment_intent_id, payment.stripe_charge_id, payment.amount, payment.currency, payment.status };
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("usp_payments_insert", param, null, null, commandType);
	}
}
