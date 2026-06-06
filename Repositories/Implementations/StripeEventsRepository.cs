using Dapper;
using System.Data;
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
        using var conn = _context.CreateOpenConnection();

        return await conn.QueryFirstOrDefaultAsync<StripeEvents?>(
            "usp_stripe_events_get_by_stripe_eventid",
            new { stripe_eventid },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task InsertStripeEventAsync(StripeEvents entity)
    {
        using var conn = _context.CreateOpenConnection();

         await conn.ExecuteAsync(
            "usp_stripe_events_insert",
            new
            {
                entity.event_id,
                entity.event_type,
                entity.api_version,
                entity.livemode,
                entity.payload,
                entity.signature,
                entity.processed,
                entity.processed_at,
                entity.received_at
            },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task MarkStripeEventProcessedAsync(StripeEvents entity)
    {
        using var conn = _context.CreateOpenConnection();

        await conn.ExecuteAsync(
            "usp_stripe_events_mark_processed",
            new
            {
                event_id = entity.event_id   // ✔ FIXED
            },
            commandType: CommandType.StoredProcedure
        );
    }
}
