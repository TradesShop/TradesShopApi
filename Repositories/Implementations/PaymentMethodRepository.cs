using Dapper;
using Microsoft.VisualBasic;
using Stripe;
using System.Data;
using TradePlatform.Api.Data;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services;

namespace TradePlatform.Api.Repositories.Implementations
{
    public class PaymentMethodRepository : IPaymentMethodRepository
    {
        private readonly DapperContext _context;
        private readonly IIdentityService _identity;

        public PaymentMethodRepository(DapperContext context, IIdentityService identity)
        {
            _context = context;
            _identity = identity;
        }

        // ---------------------------------------------------------
        // GET PAYMENT METHODS
        // ---------------------------------------------------------
        public async Task<IEnumerable<PaymentMethod_db>> GetPaymentMethodsAsync(Guid userId)
        {
            using var conn = _context.CreateOpenConnection();
            var result = await conn.QueryAsync<PaymentMethod_db>(
                "usp_payment_methods_get_by_user",
                new { user_id = userId },
                commandType: CommandType.StoredProcedure
            );

            return result;
        }

        // ---------------------------------------------------------
        // ADD PAYMENT METHOD
        // ---------------------------------------------------------
        public async Task<Guid> AddPaymentMethodAsync(PaymentMethod_db model)
        {
            using var conn = _context.CreateOpenConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@user_id", model.user_id);
            parameters.Add("@stripe_payment_method_id", model.stripe_payment_method_id);
            parameters.Add("@brand", model.brand);
            parameters.Add("@last4", model.last4);
            parameters.Add("@exp_month", model.exp_month);
            parameters.Add("@exp_year", model.exp_year);
            parameters.Add("@is_default", model.is_default);
            parameters.Add("@name_on_card",model.name_on_card);
            parameters.Add("@updated_by", _identity.GetUserId());
            parameters.Add("@new_id", dbType: DbType.Guid, direction: ParameterDirection.Output);

            await conn.ExecuteAsync(
                "usp_payment_methods_insert",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return parameters.Get<Guid>("@new_id");
        }

        // ---------------------------------------------------------
        // SET DEFAULT PAYMENT METHOD
        // ---------------------------------------------------------
        public async Task SetDefaultPaymentMethodAsync(Guid userId, string stripe_paymentmethod_id)
        {
           
            using var conn = _context.CreateOpenConnection();
            await conn.ExecuteAsync(
                "usp_payment_methods_set_default",
                new
                {
                    user_id = userId,
                    stripe_payment_method_id = stripe_paymentmethod_id
                    ,updated_by = _identity.GetUserId()
                },
                commandType: CommandType.StoredProcedure
            );
        }

        // ---------------------------------------------------------
        // SOFT DELETE PAYMENT METHOD
        // ---------------------------------------------------------
        public async Task SoftDeletePaymentMethodAsync(Guid userId, string stripe_payment_method_id)
        {
            using var conn = _context.CreateOpenConnection();
            var updated_by = _identity.GetUserId();
            await conn.ExecuteAsync(
                "usp_payment_methods_soft_delete",
                new
                {
                    user_id = userId,
                    stripe_payment_method_id = stripe_payment_method_id,
                    updated_by= updated_by
                },
                commandType: CommandType.StoredProcedure
            );
        }
        public async Task UpdatePaymentMethodAsync(
          string stripe_payment_method_id,
          string? name_on_card,
          int exp_month,
          int exp_year,
          Guid effectiveUserId)
            {
                using var conn = _context.CreateOpenConnection();

                var parameters = new DynamicParameters();
                parameters.Add("@user_id", effectiveUserId);
                parameters.Add("@stripe_payment_method_id", stripe_payment_method_id);
                parameters.Add("@name_on_card", name_on_card);
                parameters.Add("@exp_month", exp_month);
                parameters.Add("@exp_year", exp_year);
                parameters.Add("@updated_by", _identity.GetUserId());

                await conn.ExecuteAsync(
                    "usp_payment_method_update",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }

        public async Task<PaymentMethod_db> GetDefaultPaymentMethodAsync(Guid user_id)
        {
            using var conn = _context.CreateOpenConnection();

            var anymethod= await conn.QueryFirstOrDefaultAsync<PaymentMethod_db>(
                "usp_payment_methods_get_default",
                new { user_id },
                commandType: CommandType.StoredProcedure
            );
            return anymethod;
        }
    }
}


