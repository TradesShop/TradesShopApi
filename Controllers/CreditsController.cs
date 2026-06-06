using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.Repositories;
using TradePlatform.Api.Services.Credits;

namespace TradePlatform.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CreditsController : BaseController
    {
        private readonly ICreditService _creditService;

        public CreditsController(
         ICreditService creditService,
        IHttpContextAccessor http
        ) : base(http)
        {
            _creditService = creditService;
        }


        // GET /api/categories/5
        [HttpGet("my")]
        public async Task<IActionResult> MyCreditsAsync()
        {
            var (callerId, callerType) = GetIdentity();
            Guid user_id = callerId;
            var anycredit = await _creditService.GetBalanceAsync(user_id);
            return ApiOk(anycredit);
        }
    }
}
