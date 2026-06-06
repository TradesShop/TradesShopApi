using TradePlatform.Api.Models;
using TradePlatform.Api.Models.Plans;

namespace TradePlatform.Api.Repositories.Interfaces
{
    public interface IPlansRepository
    {
        Task<IEnumerable<Plan>> GetAllPlansAsync();
        Task<PlanPriceByPriceId> GetPlanPriceByPriceId(Guid plan_price_id);
        Task<Plan> GetPlanByIdAsync(Guid plan_id);
        Task<PlanPrice> GetPlanPriceByIdAsync(Guid plan_price_id);
        Task<IEnumerable<PlanPrice>> GetPlanPricesAsync(Guid planId);
        Task<List<Plan>> GetActivePlansAsync();
    }
}
