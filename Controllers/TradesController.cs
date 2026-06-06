using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.Repositories;
using TradePlatform.Api.Repositories.Implementations;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TradesController : BaseController
    {
        private readonly ITradesRepository _repo;

        public TradesController(ITradesRepository repo,
         IHttpContextAccessor http
        ) : base(http)
        {
            _repo = repo;
        }

        // GET /api/trades
        [HttpGet]
        public async Task<IActionResult> GetTrades()
        {
            var trades = await _repo.GetTradesAsync(null);
            return ApiOk(trades);
        }

        // GET /api/trades/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetByTrade(int id)
        {
            var trades = await _repo.GetTradesAsync(id);
            return ApiOk(trades);
        }
    }
}
