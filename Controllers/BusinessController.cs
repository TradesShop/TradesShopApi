using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.DTOs.Business;
using TradePlatform.Api.Models;
using TradePlatform.Api.Services.Business;

namespace TradePlatform.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BusinessController : BaseController
{
	private readonly IBusinessService _businessService;

	public BusinessController(IBusinessService businessService)
	{
		_businessService = businessService;
	}

	[HttpGet("categories")]
	public async Task<IActionResult> BusinessCategoryForUserAsync([FromQuery] Guid? target_user_id)
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		Guid user_id = identity.userId;
		UserType user_type = identity.userType;
		Guid effectiveUserId = ResolveEffectiveUser(user_id, user_type, target_user_id);
		return ApiOk(await _businessService.BusinessCategoryForUserAsync(effectiveUserId));
	}

	[HttpGet("categoryskills/{business_id}")]
	public async Task<IActionResult> GetBusinessCategorySkills(Guid business_id)
	{
		return ApiOk(await _businessService.GetBusinessCategorySkillsAsync(business_id));
	}

	[HttpPost("updateskills")]
	public async Task<IActionResult> UpdateSkills([FromBody] BusinessSkillsUpdateDto dto)
	{
		await _businessService.BusinessSkillsUpdateAsync(dto);
		return ApiOk(new
		{
			success = true
		});
	}

	[HttpPost("profile")]
	public async Task<IActionResult> BusinessProfileForUserAsync()
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		var (callerId, _) = identity;
		_ = identity.userType;
		return ApiOk(await _businessService.BusinessProfileForUserAsync(callerId));
	}

	[HttpPost("profile/upsert")]
	public async Task<IActionResult> BusinessProfileUpsertAsync([FromBody] BusinessProfileDto bpDto)
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		_ = identity.userId;
		_ = identity.userType;
		bpDto.public_slug = GenerateSlug(bpDto.name);
		return ApiOk(await _businessService.BusinessProfileUpsertAsync(bpDto));
	}

	[HttpPost("address/update")]
	public async Task<IActionResult> BusinessAdressUpdateAsync([FromBody] UserAddress model)
	{
		if (model == null)
		{
			return BadRequest("Invalid address payload");
		}
		(Guid userId, UserType userType) identity = GetIdentity();
		var (user_id, _) = identity;
		_ = identity.userType;
		model.user_id = user_id;
		UserAddress anyaddress = await _businessService.BusinessAdressUpdateAsync(model);
		if (!anyaddress.success)
		{
			return ApiError(anyaddress);
		}
		return ApiOk(anyaddress);
	}

	[HttpPost("address/remove")]
	public async Task<IActionResult> BusinessAdressRemoveAsync([FromBody] AddressRemoveReq model)
	{
		if (model == null)
		{
			return BadRequest("Invalid address payload");
		}
		(Guid userId, UserType userType) identity = GetIdentity();
		var (user_id, _) = identity;
		_ = identity.userType;
		model.user_id = user_id;
		return ApiOk(await _businessService.BusinessAdressRemoveAsync(model));
	}

	[HttpPost("addresses")]
	public async Task<IActionResult> BusinessAddressesForUserAsync()
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		var (callerId, _) = identity;
		_ = identity.userType;
		return ApiOk(await _businessService.BusinessAddressesForUserAsync(callerId));
	}

	[HttpGet("webprofile/get/{business_id}")]
	public async Task<IActionResult> BusinessWebProfileForUserAsync(Guid business_id)
	{
		return ApiOk(await _businessService.BusinessWebProfileForUserAsync(business_id));
	}

	[HttpPost("webprofile/upsert")]
	public async Task<IActionResult> BusinessWebProfileUpsert([FromBody] BusinessWebProfileDto bwpDto)
	{
		await _businessService.BusinessWebProfileUpsert(bwpDto);
		return ApiOk(new
		{
			message = "Social Media details updated successfully!"
		});
	}

	[HttpGet("profile/{slug}")]
	public async Task<IActionResult> BusinessProfileForSlugAsync(string slug)
	{
		return ApiOk(await _businessService.BusinessProfileForSlugAsync(slug));
	}

	[HttpGet("maxcategories")]
	public async Task<IActionResult> BusinessMaxCategoriesForUserAsync([FromQuery] Guid? target_user_id)
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		Guid user_id = identity.userId;
		UserType user_type = identity.userType;
		user_id = ResolveEffectiveUser(user_id, user_type, target_user_id);
		return ApiOk(await _businessService.BusinessMaxCategoriesForUserAsync(user_id));
	}

	[HttpGet("maxlocations")]
	public async Task<IActionResult> BusinessMaxLocationsForUserAsync([FromQuery] Guid? target_user_id)
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		Guid user_id = identity.userId;
		UserType user_type = identity.userType;
		user_id = ResolveEffectiveUser(user_id, user_type, target_user_id);
		return ApiOk(await _businessService.BusinessMaxLocationsForUserAsync(user_id));
	}
}
