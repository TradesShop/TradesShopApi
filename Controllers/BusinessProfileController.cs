using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.DTOs.users;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BusinessProfileController : BaseController
{
	private readonly IBusinessProfileRepository _repo;

	public BusinessProfileController(IBusinessProfileRepository businessProfileRepository)
	{
		_repo = businessProfileRepository;
	}

	[HttpGet("me")]
	public async Task<ActionResult<BusinessProfile?>> GetMyProfile()
	{
		Guid user_id = GetUserId();
		return Ok(await _repo.GetByUserIdAsync(user_id));
	}

	[HttpPost("intro_msg_update")]
	public async Task<IActionResult> business_intro_msg_update_async(IntroMessageUpdateReqDto introMsgDto)
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		var (callerId, _) = identity;
		_ = identity.userType;
		introMsgDto.user_id = callerId;
		return ApiOk(await _repo.business_intro_msg_update_async(introMsgDto));
	}
}
