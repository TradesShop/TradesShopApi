using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services;

namespace TradePlatform.Api.Controllers
{
    [Route("api/billing/[controller]")]
    [ApiController]
    public class PlansController : ControllerBase
    {
        private readonly PlansService _plansService;

        public PlansController(PlansService plansService)
        {
            _plansService = plansService;
        }

        [HttpGet]
        public async Task<IActionResult> GetActivePlans()
        {
            var plans = await _plansService.GetActivePlansAsync();

            return Ok(new
            {
                success = true,
                data = plans
            });
        }
    }
}
