using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.V2.Network;
using TradePlatform.Api.DTOs;
using TradePlatform.Api.DTOs.Business;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services.Business;

namespace TradePlatform.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessController : BaseController
    {
        private readonly IBusinessService _businessService;

        public BusinessController(
            IBusinessService businessService,
            IHttpContextAccessor http
        ) : base(http)
        {
            _businessService = businessService;
        }

        
        /*Busines Category and skill update*/
        [HttpGet("categories")]
        public async Task<IActionResult> BusinessCategoryForUserAsync()
        {
            var (callerId, callerType) = GetIdentity();

            var anyCategories = await _businessService.BusinessCategoryForUserAsync(callerId);
            return ApiOk(anyCategories);

            //return Ok(new { exists = 0 });
        }
        [HttpGet("categoryskills/{business_id}")]
        public async Task<IActionResult> GetBusinessCategorySkills(Guid business_id)
        {
            var result = await _businessService.GetBusinessCategorySkillsAsync(business_id);
            return ApiOk(result);
        }
        [HttpPost("updateskills")]
        public async Task<IActionResult> UpdateSkills([FromBody] BusinessSkillsUpdateDto dto)
        {
            await _businessService.BusinessSkillsUpdateAsync(dto);
            return ApiOk(new { success = true });
        }
        [HttpPost("profile")]
        public async Task<IActionResult> BusinessProfileForUserAsync()
        {
            var (callerId, callerType) = GetIdentity();

            var anyprofile = await _businessService.BusinessProfileForUserAsync(callerId);
            return ApiOk(anyprofile);

            //return Ok(new { exists = 0 });
        }
        [HttpPost("profile/upsert")]
        public async Task<IActionResult> BusinessProfileUpsertAsync([FromBody] BusinessProfileDto bpDto)
        {
            var (callerId, callerType) = GetIdentity();
            bpDto.public_slug = GenerateSlug(bpDto.name);
            var anyprofile = await _businessService.BusinessProfileUpsertAsync(bpDto);
            return ApiOk(anyprofile);

            //return Ok(new { exists = 0 });
        }
        [HttpPost("address/update")]
        public async Task<IActionResult> BusinessAdressUpdateAsync([FromBody] UserAddress model)
        {
            if (model == null)
                return BadRequest("Invalid address payload");

            var anyaddress = await _businessService.BusinessAdressUpdateAsync(model);
            if (!anyaddress.success)
                return ApiError(anyaddress);
            return ApiOk(anyaddress);
        }
        [HttpPost("addresses")]
        public async Task<IActionResult> BusinessAddressesForUserAsync()
        {
            var (callerId, callerType) = GetIdentity();

            var anyAddresses = await _businessService.BusinessAddressesForUserAsync(callerId);
            return ApiOk(anyAddresses);

            //return Ok(new { exists = 0 });
        }

        [HttpGet("webprofile/get/{business_id}")]
        public async Task<IActionResult> BusinessWebProfileForUserAsync(Guid business_id)
        {
            var anyprofile = await _businessService.BusinessWebProfileForUserAsync(business_id);
            return ApiOk(anyprofile);
          
        }
       
        [HttpPost("webprofile/upsert")]
        public async Task<IActionResult> BusinessWebProfileUpsert([FromBody] BusinessWebProfileDto bwpDto)
        {
            await _businessService.BusinessWebProfileUpsert(bwpDto);
            return ApiOk(new { message = "Social Media details updated successfully!" });
        }

    }
}
