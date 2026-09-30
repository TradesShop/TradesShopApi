using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradePlatform.Api.Models;
using TradePlatform.Api.Models.PlanPrices;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Services.plans;

public class PlansService : IPlansService
{
	private readonly IPlansRepository _plansRepository;

	public PlansService(IPlansRepository plansRepository)
	{
		_plansRepository = plansRepository;
	}

	public async Task<List<Plan>> GetActivePlansAsync(string plan_type, Guid user_id)
	{
		return await _plansRepository.GetActivePlansAsync(plan_type, user_id);
	}

	public async Task<PlansMeta> PlansUpsertAsync(PlansMeta anyplan)
	{
		return await _plansRepository.PlansUpsertAsync(anyplan);
	}

	public async Task<IEnumerable<PlansMeta>> GetPlansListAsync(string? searchname, string? type)
	{
		return await _plansRepository.GetPlansListAsync(searchname, type);
	}

	public async Task<IEnumerable<PlansMeta>> PlansExistsAsync(string? name, Guid? exclude_id)
	{
		return await _plansRepository.PlansExistsAsync(name, exclude_id);
	}

	public async Task<IEnumerable<PlanPricesMeta>> GetPlanPricesByPlanIdAsync(Guid plan_id, bool active_only = false)
	{
		return await _plansRepository.GetPlanPricesByPlanIdAsync(plan_id, active_only);
	}

	public async Task<PlanPricesMeta> PlanPricesUpsertAsync(PlanPricesMeta anyprice)
	{
		return await _plansRepository.PlanPricesUpsertAsync(anyprice);
	}
}
