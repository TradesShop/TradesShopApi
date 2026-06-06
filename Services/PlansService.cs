using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Services
{
    public class PlansService
    {
        private readonly IPlansRepository _plansRepository;

        public PlansService(IPlansRepository plansRepository)
        {
            _plansRepository = plansRepository;
        }

        public async Task<List<Plan>> GetActivePlansAsync()
        {
            // Get plans + prices from repository (1 DB call)
            return await _plansRepository.GetActivePlansAsync();

           
        }
    }
}
