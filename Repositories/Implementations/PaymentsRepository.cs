using Dapper;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System.Data;
using TradePlatform.Api.Data;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services;

namespace TradePlatform.Api.Repositories.Implementations
{
    public class PaymentsRepository:IPaymentsRepository
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
                throw new Exception("Only tradesperson or admin users can access billing features");
        }

        private Guid ResolveEffectiveUserId(Guid callerId, UserType callerType, Guid? targetUserId)
        {
            if (callerType == UserType.admin && targetUserId.HasValue)
                return targetUserId.Value;

            return callerId;
        }

        public async Task<string> CreateSetupIntentAsync(Guid callerId, UserType callerType, Guid? targetUserId)
        {
            EnsureBillingAccess(callerType);
            var effectiveUserId = ResolveEffectiveUserId(callerId, callerType, targetUserId);
            return await _stripeService.CreateSetupIntentAsync(effectiveUserId);
        }

        public async Task<object> AttachPaymentMethodAsync(Guid effectiveUserId, UserType callerType, string payment_method_id)
        {
            EnsureBillingAccess(callerType);

            var method = await _stripeService.AttachPaymentMethodToCustomerAsync(effectiveUserId, payment_method_id);

            return new
            {
                success = true,
                payment_method = method
            };
        }
      
        public async Task<object> SubscribeAsync(Guid callerId, UserType callerType, string priceId, string paymentMethodId, Guid? targetUserId)
        {
            EnsureBillingAccess(callerType);
            var effectiveUserId = ResolveEffectiveUserId(callerId, callerType, targetUserId);

            var subscription = await _stripeService.CreateOrUpdateSubscriptionAsync(
                effectiveUserId,
                priceId,
                paymentMethodId
            );

            return new
            {
                success = true,
                subscription
            };
        }

        public async Task CancelSubscriptionAsync(Guid callerId, UserType callerType, string stripe_subscription_id, Guid? targetUserId)
        {
            EnsureBillingAccess(callerType);
            var effectiveUserId = ResolveEffectiveUserId(callerId, callerType, targetUserId);

            await _stripeService.CancelSubscriptionAsync(stripe_subscription_id);
        }
        


       
        public async Task MarkSucceededAsync(string stripe_payment_intent_id)
        {
            using var conn = _context.CreateOpenConnection();

            await conn.ExecuteAsync(
                "usp_payments_mark_succeeded",
                new { stripe_payment_intent_id },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task MarkFailedAsync(string stripe_payment_intent_id)
        {
            using var conn = _context.CreateOpenConnection();

            await conn.ExecuteAsync(
                "usp_payments_mark_failed",
                new { stripe_payment_intent_id },
                commandType: CommandType.StoredProcedure
            );
        }
        public async Task MarkRefundedAsync(Guid payment_id, decimal amount, string stripe_refund_id)
        {
            const string sql = @"
                UPDATE payments
                SET 
                    status = 'refunded',
                    refunded_amount = @amount,
                    stripe_refund_id = @stripe_refund_id,
                    refunded_at = NOW()
                WHERE id = @payment_id;";

            using var connection = _context.CreateOpenConnection();
            await connection.ExecuteAsync(sql, new
            {
                payment_id,
                amount,
                stripe_refund_id
            });
        }

        public async Task<PaymentsM?> GetByIdAsync(Guid id)
        {
            using var conn = _context.CreateOpenConnection();

            return await conn.QueryFirstOrDefaultAsync<PaymentsM>(
                "usp_payments_get_by_id",
                new { id },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<PaymentsM>> GetByInvoiceIdAsync(Guid invoice_id)
        {
            using var conn = _context.CreateOpenConnection();

            return await conn.QueryAsync<PaymentsM>(
                "usp_payments_get_by_invoice_id",
                new { invoice_id },
                commandType: CommandType.StoredProcedure
            );
        }
       
        public async Task<IEnumerable<PaymentsM>> GetByUserIdAsync(Guid user_id)
        {
            using var conn = _context.CreateOpenConnection();

            return await conn.QueryAsync<PaymentsM>(
                "usp_payments_get_by_user_id",
                new { user_id },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<PaymentsM?> GetByStripePaymentIntentIdAsync(string stripe_payment_intent_id)
        {
            using var conn = _context.CreateOpenConnection();

            return await conn.QueryFirstOrDefaultAsync<PaymentsM>(
                "usp_payments_get_by_stripe_payment_intent_id",
                new { stripe_payment_intent_id },
                commandType: CommandType.StoredProcedure
            );
        }
        public async Task InsertPaymentAsync(PaymentsM payment)
        {
            using var conn = _context.CreateOpenConnection();

            await conn.ExecuteAsync(
                "usp_payments_insert",
                new
                {                    
                    payment.user_id,
                    payment.invoice_id,
                    payment.stripe_payment_intent_id,
                    payment.stripe_charge_id,
                    payment.amount,
                    payment.currency,
                    payment.status                  
                },
                commandType: CommandType.StoredProcedure
            );

           
        }

        public async Task CreateAsync(PaymentsM payment)
        {
            using var conn = _context.CreateOpenConnection();

            await conn.ExecuteAsync(
                "usp_payments_insert",
                new
                {
                    payment.user_id,
                    payment.invoice_id,
                    payment.stripe_payment_intent_id,
                    payment.stripe_charge_id,
                    payment.amount,
                    payment.currency,
                    payment.status
                },
                commandType: CommandType.StoredProcedure
            );
        }
    }
}
