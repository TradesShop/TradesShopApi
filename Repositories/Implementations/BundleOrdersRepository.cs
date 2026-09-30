using System.Data;
using System.Threading.Tasks;
using Dapper;
using TradePlatform.Api.Data;
using TradePlatform.Api.DTOs.Bundles;
using TradePlatform.Api.Models.BundleCredit;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Repositories.Implementations;

public class BundleOrdersRepository : IBundleOrdersRepository
{
	private readonly DapperContext _context;

	public BundleOrdersRepository(DapperContext context)
	{
		_context = context;
	}

	public async Task<BundleOrders> CreateAsync(BundleOrders order)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { 
			  order.user_id
			, order.bundle_price_id			
			, order.stripe_price_id
			, order.amount
			, order.currency 
		};
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<BundleOrders>("usp_bundle_order_create", param, null, null, commandType);
	}

	public async Task CreditBundlePurchaseCompletedAsync(BundlePurchaseCompletedDto dto)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		DynamicParameters p = new DynamicParameters();
		p.Add("@bundle_order_id", dto.bundle_order_id);
		p.Add("@bundle_price_id", dto.bundle_price_id);
		p.Add("@user_id", dto.user_id);
		p.Add("@stripe_payment_intent_id", dto.stripe_payment_intent_id);
		p.Add("@stripe_charge_id", dto.stripe_charge_id);
		p.Add("@stripe_invoice_id", dto.stripe_invoice_id);
		p.Add("@status", dto.status);
		p.Add("@billing_email", dto.billing_email);
		p.Add("@amount_total", dto.amount_total);
		p.Add("@amount_subtotal", dto.amount_subtotal);
		p.Add("@amount_vat", dto.amount_vat);
		p.Add("@currency", dto.currency);
		p.Add("@stripe_event_id", dto.stripe_event_id);
		p.Add("@entity_type_id", dto.entity_type_id);
		p.Add("@entity_id", dto.entity_id);
		p.Add("@invoice_url", dto.invoice_url);
		p.Add("@paid_at", dto.paid_at);
		p.Add("@entity_type", dto.entity_type);
		p.Add("@metadata_json", dto.metadataJson);
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("[dbo].[usp_bundle_purchase_completed]", p, null, null, commandType);
	}

	public async Task BundleOrderMarkFailedAsync(BundleCheckoutFailedDto dto)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		DynamicParameters p = new DynamicParameters();
		p.Add("@bundle_order_id", dto.bundle_order_id);
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("usp_bundle_order_mark_failed", p, null, null, commandType);
	}

	public async Task MarkPaidAsync(string stripe_session_id, string stripe_payment_intent_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { stripe_session_id, stripe_payment_intent_id };
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("usp_bundle_order_mark_paid", param, null, null, commandType);
	}

	public async Task MarkRefundedAsync(string stripe_payment_intent_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { stripe_payment_intent_id };
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("usp_bundle_order_mark_refunded", param, null, null, commandType);
	}

	public async Task<BundleOrders?> GetByStripeSessionIdAsync(string stripe_session_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		string sql = "SELECT * FROM bundle_orders WHERE stripe_session_id = @stripe_session_id";
		return await conn.QueryFirstOrDefaultAsync<BundleOrders>(sql, new { stripe_session_id });
	}

	public async Task<BundleOrders?> GetByPaymentIntentIdAsync(string stripe_payment_intent_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		string sql = "SELECT * FROM bundle_orders WHERE stripe_payment_intent_id = @stripe_payment_intent_id";
		return await conn.QueryFirstOrDefaultAsync<BundleOrders>(sql, new { stripe_payment_intent_id });
	}
}
