using System.Data;
using System.Threading.Tasks;
using Dapper;
using TradePlatform.Api.Data;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;

public class StripeEventsRepository : IStripeEventsRepository
{
	private readonly DapperContext _context;

	public StripeEventsRepository(DapperContext context)
	{
		_context = context;
	}

	public async Task<StripeEvents?> GetByStripeEventIdAsync(string stripe_eventid)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { stripe_eventid };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<StripeEvents>("usp_stripe_events_get_by_stripe_eventid", param, null, null, commandType);
	}

	public async Task InsertStripeEventAsync(StripeEvents entity)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { entity.event_id, entity.event_type, entity.api_version, entity.livemode, entity.payload, entity.signature, entity.processed, entity.processed_at, entity.received_at };
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("usp_stripe_events_insert", param, null, null, commandType);
	}

	public async Task MarkStripeEventProcessedAsync(StripeEvents entity)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { entity.event_id };
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("usp_stripe_events_mark_processed", param, null, null, commandType);
	}
}
