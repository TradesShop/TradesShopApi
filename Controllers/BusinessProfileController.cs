using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TradePlatform.Api.DTOs;
using TradePlatform.Api.DTOs.users;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class BusinessProfileController : BaseController
    {
        private readonly IBusinessProfileRepository _repo;

        public BusinessProfileController(IBusinessProfileRepository businessProfileRepository,
            IHttpContextAccessor http
        ) : base(http)
        {
            _repo = businessProfileRepository;
        }


        [HttpGet("me")]
        public async Task<ActionResult<BusinessProfile?>> GetMyProfile()
        {
            var user_id = GetCurrentUserId();
            var profile = await _repo.GetByUserIdAsync(user_id);
            return Ok(profile);
        }

        //[HttpPost("upsert")]
        //public async Task<ActionResult<BusinessProfile>> Upsert([FromBody] BusinessProfileUpsertDto dto)
        //{
        //    var user_id = GetCurrentUserId();
        //    var result = await _repo.UpsertAsync(
        //        dto.id,
        //        user_id,
        //        dto.name,
        //        dto.description,
        //        dto.active_since,
        //        dto.website_url,
        //        dto.business_type_id,
        //        dto.number_of_employees,
        //        dto.registration_number,
        //        dto.service_radius_km,
        //        dto.verified,
        //        user_id
        //    );
        //    return Ok(result);
        //}
        [HttpPost("intro_msg_update")]
        public async Task<IActionResult> business_intro_msg_update_async(IntroMessageUpdateReqDto introMsgDto)
        {
            var (callerId, callerType) = GetIdentity();
            introMsgDto.user_id = callerId;
            var anyaccount = await _repo.business_intro_msg_update_async(introMsgDto);
            return ApiOk(anyaccount);

        }
    }
}
