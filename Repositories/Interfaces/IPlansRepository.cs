using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradePlatform.Api.Models;
using TradePlatform.Api.Models.PlanPrices;
using TradePlatform.Api.Models.Plans;

namespace TradePlatform.Api.Repositories.Interfaces;

public interface IPlansRepository
{
	Task<IEnumerable<Plan>> GetAllPlansAsync();

	Task<PlanPriceByPriceId> GetPlanPriceByPriceId(Guid plan_price_id);

	Task<Plan> GetPlanByIdAsync(Guid plan_id);

	Task<PlanPrice> GetPlanPriceByIdAsync(Guid plan_price_id);

	Task<IEnumerable<PlanPrice>> GetPlanPricesAsync(Guid planId);

	Task<List<Plan>> GetActivePlansAsync(string plan_type, Guid user_id);

	Task<IEnumerable<PlansMeta>> GetPlansListAsync(string? searchname, string? type);

	Task<IEnumerable<PlansMeta>> PlansExistsAsync(string? name, Guid? exclude_id);

	Task<PlansMeta> PlansUpsertAsync(PlansMeta anyplan);

	Task<IEnumerable<PlanPricesMeta>> GetPlanPricesByPlanIdAsync(Guid plan_id, bool active_only = false);

	Task<PlanPricesMeta> PlanPricesUpsertAsync(PlanPricesMeta anyprice);
}
