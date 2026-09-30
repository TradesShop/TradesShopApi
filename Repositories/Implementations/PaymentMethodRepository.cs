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

public class PaymentMethodRepository : IPaymentMethodRepository
{
	private readonly DapperContext _context;

	private readonly IIdentityService _identity;

	public PaymentMethodRepository(DapperContext context, IIdentityService identity)
	{
		_context = context;
		_identity = identity;
	}

	public async Task<IEnumerable<PaymentMethod_db>> GetPaymentMethodsAsync(Guid userId)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new
		{
			user_id = userId
		};
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryAsync<PaymentMethod_db>("usp_payment_methods_get_by_user", param, null, null, commandType);
	}

	public async Task<Guid> AddPaymentMethodAsync(PaymentMethod_db model)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		DynamicParameters parameters = new DynamicParameters();
		parameters.Add("@user_id", model.user_id);
		parameters.Add("@stripe_payment_method_id", model.stripe_payment_method_id);
		parameters.Add("@brand", model.brand);
		parameters.Add("@last4", model.last4);
		parameters.Add("@exp_month", model.exp_month);
		parameters.Add("@exp_year", model.exp_year);
		parameters.Add("@is_default", model.is_default);
		parameters.Add("@name_on_card", model.name_on_card);
		parameters.Add("@updated_by", _identity.GetUserId());
		parameters.Add("@new_id", null, DbType.Guid, ParameterDirection.Output);
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("usp_payment_methods_insert", parameters, null, null, commandType);
		return parameters.Get<Guid>("@new_id");
	}

	public async Task SetDefaultPaymentMethodAsync(Guid userId, string stripe_paymentmethod_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new
		{
			user_id = userId,
			stripe_payment_method_id = stripe_paymentmethod_id,
			updated_by = _identity.GetUserId()
		};
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("usp_payment_methods_set_default", param, null, null, commandType);
	}

	public async Task SoftDeletePaymentMethodAsync(Guid userId, string stripe_payment_method_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		Guid updated_by = _identity.GetUserId();
		var param = new
		{
			user_id = userId,
			stripe_payment_method_id = stripe_payment_method_id,
			updated_by = updated_by
		};
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("usp_payment_methods_soft_delete", param, null, null, commandType);
	}

	public async Task UpdatePaymentMethodAsync(string stripe_payment_method_id, string? name_on_card, int exp_month, int exp_year, Guid effectiveUserId)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		DynamicParameters parameters = new DynamicParameters();
		parameters.Add("@user_id", effectiveUserId);
		parameters.Add("@stripe_payment_method_id", stripe_payment_method_id);
		parameters.Add("@name_on_card", name_on_card);
		parameters.Add("@exp_month", exp_month);
		parameters.Add("@exp_year", exp_year);
		parameters.Add("@updated_by", _identity.GetUserId());
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("usp_payment_method_update", parameters, null, null, commandType);
	}

	public async Task<PaymentMethod_db> GetDefaultPaymentMethodAsync(Guid user_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { user_id };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<PaymentMethod_db>("usp_payment_methods_get_default", param, null, null, commandType);
	}
}
