using System.Data;
using System.Threading.Tasks;
using Dapper;
using TradePlatform.Api.Data;
using TradePlatform.Api.DTOs.TeleMetry;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Repositories.Implementations;

public class TelemetryErrorsRepository : ITelemetryErrorsRepository
{
	private readonly DapperContext _context;

	public TelemetryErrorsRepository(DapperContext context)
	{
		_context = context;
	}

	public async Task telemetry_error_insert_async(telemetry_error error)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("[dbo].[usp_telemetry_errors_insert]", error, null, null, commandType);
	}

	public async Task audit_event_insert_async(audit_event anyevent)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("[dbo].[usp_audit_event_insert]", anyevent, null, null, commandType);
	}
}
