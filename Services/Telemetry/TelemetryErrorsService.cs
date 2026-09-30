using System.Threading.Tasks;
using TradePlatform.Api.DTOs.TeleMetry;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Services.Telemetry;

public class TelemetryErrorsService : ITelemetryErrorsService
{
	private readonly ITelemetryErrorsRepository _repo;

	public TelemetryErrorsService(ITelemetryErrorsRepository repo)
	{
		_repo = repo;
	}

	public async Task telemetry_error_insert_async(telemetry_error error)
	{
		await _repo.telemetry_error_insert_async(error);
	}

	public async Task audit_event_insert_async(audit_event anyevent)
	{
		await _repo.audit_event_insert_async(anyevent);
	}
}
