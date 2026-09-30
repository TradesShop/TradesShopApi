using System.Threading.Tasks;
using TradePlatform.Api.DTOs.TeleMetry;

namespace TradePlatform.Api.Repositories.Interfaces;

public interface ITelemetryErrorsRepository
{
	Task telemetry_error_insert_async(telemetry_error error);

	Task audit_event_insert_async(audit_event anyevent);
}
