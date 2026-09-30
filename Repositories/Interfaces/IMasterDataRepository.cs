using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradePlatform.Api.DTOs.Dashboard;
using TradePlatform.Api.DTOs.MasterData;

namespace TradePlatform.Api.Repositories.Interfaces;

public interface IMasterDataRepository
{
	Task<IEnumerable<MasterDataStatuses>> MasterDataStatusesAsync(string entity_type, bool? is_active);

	Task<TraderDashboardStatsDto> GetTraderDashboardStatsAsync(Guid user_id);
}
