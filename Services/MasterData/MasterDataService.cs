using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradePlatform.Api.DTOs.Dashboard;
using TradePlatform.Api.DTOs.MasterData;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Services.MasterData;

public class MasterDataService : IMasterDataService
{
	private readonly IMasterDataRepository _repository;

	public MasterDataService(IMasterDataRepository repository)
	{
		_repository = repository;
	}

	public async Task<IEnumerable<MasterDataStatuses>> MasterDataStatusesAsync(string entity_type, bool? is_active)
	{
		return await _repository.MasterDataStatusesAsync(entity_type, is_active);
	}

	public async Task<TraderDashboardStatsDto> GetTraderDashboardStatsAsync(Guid user_id)
	{
		return await _repository.GetTraderDashboardStatsAsync(user_id);
	}
}
