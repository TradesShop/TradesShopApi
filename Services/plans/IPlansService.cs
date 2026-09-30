using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradePlatform.Api.Models;
using TradePlatform.Api.Models.PlanPrices;

namespace TradePlatform.Api.Services.plans;

public interface IPlansService
{
	Task<List<Plan>> GetActivePlansAsync(string plan_type, Guid user_id);

	Task<IEnumerable<PlansMeta>> GetPlansListAsync(string? searchname, string? type);

	Task<IEnumerable<PlansMeta>> PlansExistsAsync(string? name, Guid? exclude_id);

	Task<IEnumerable<PlanPricesMeta>> GetPlanPricesByPlanIdAsync(Guid plan_id, bool active_only = false);

	Task<PlanPricesMeta> PlanPricesUpsertAsync(PlanPricesMeta anyprice);
}
