using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.DTOs.Common;
using TradePlatform.Api.DTOs.users;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services.users;

namespace TradePlatform.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : BaseController
{
	private readonly IUsersRepository _repo;

	private readonly IUsersService _usersService;

	private readonly IEmailVerificationRepository _verificationRepo;

	public UsersController(IUsersRepository repo, IUsersService usersService, IEmailVerificationRepository verificationRepo)
	{
		_repo = repo;
		_usersService = usersService;
		_verificationRepo = verificationRepo;
	}

	[HttpGet("account")]
	public async Task<IActionResult> UserAccountGetAsync()
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		var (callerId, _) = identity;
		_ = identity.userType;
		return ApiOk(await _usersService.UserAccountGetAsync(callerId));
	}

	[HttpPost("change-password")]
	public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		var (callerId, _) = identity;
		_ = identity.userType;
		CommonResponseDto anyresult = await _usersService.ChangePasswordAsync(callerId, dto.old_password, dto.new_password);
		if (!anyresult.success)
		{
			return ApiError(anyresult);
		}
		return ApiOk(anyresult);
	}

	[HttpGet("checkuser")]
	public async Task<IActionResult> CheckUser([FromQuery] string email)
	{
		return ApiOk(await _repo.GetByEmailAsync(email, 1));
	}

	[HttpGet("checktradeuser")]
	public async Task<IActionResult> CheckTradesPerson([FromQuery] string email)
	{
		return ApiOk(await _repo.GetByEmailAsync(email, 2));
	}

	[HttpGet("me")]
	public async Task<IActionResult> GetCurrentUser()
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		var (callerId, _) = identity;
		_ = identity.userType;
		return ApiOk(await _repo.GetUserByIdAsync(callerId));
	}

	[HttpPost("upsert")]
	public async Task<IActionResult> UpdateAnyUserAsync(UserDto uDto)
	{
		if (string.IsNullOrWhiteSpace(uDto.email) || string.IsNullOrWhiteSpace(uDto.verifycode))
		{
			return ApiError(new
			{
				verified = false,
				message = "Email and verifycation code are required."
			});
		}
		if (!(await _verificationRepo.VerifyCodeAsync(uDto.email, uDto.verifycode)))
		{
			var BadRequest = new
			{
				verified = false,
				message = "Invalid or expired code. Please resend the verification code and try again."
			};
			return ApiError(BadRequest);
		}
		(Guid userId, UserType userType) identity = GetIdentity();
		var (user_id, _) = identity;
		_ = identity.userType;
		uDto.id = user_id;
		User anyresult = await _usersService.UpdateAnyUserAsync(uDto);
		if (anyresult == null)
		{
			return ApiError(anyresult);
		}
		return ApiOk(anyresult);
	}

	[HttpPost("tradespeople")]
	public async Task<IActionResult> GetTradesPeopleListAsync([FromBody] TradesPeopleSearchDto tps_dto)
	{
		return ApiOk(await _repo.GetTradesPeopleListAsync(tps_dto));
	}

	[HttpGet("{user_id}")]
	public async Task<IActionResult> GetUserBasicInfoById(Guid user_id)
	{
		return ApiOk(await _repo.GetUserBasicInfoById(user_id));
	}
}
