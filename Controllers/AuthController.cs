using System;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.DTOs;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services.Cookies;
using TradePlatform.Api.Services.Emails;

namespace TradePlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseController
{
	private readonly IAuthService _auth;

	private readonly IUsersRepository _urepo;

	private readonly IEmailVerificationRepository _verificationRepo;

	private readonly IEmailService _emailService;

	private readonly ICookieService _cookieService;

	public AuthController(IAuthService auth, IUsersRepository urepo, IEmailVerificationRepository verificationRepo, IEmailService emailService, ICookieService cookieService)
	{
		_urepo = urepo;
		_auth = auth;
		_verificationRepo = verificationRepo;
		_emailService = emailService;
		_cookieService = cookieService;
	}

	[HttpGet("test")]
	public IActionResult Test()
	{
		Console.WriteLine("Authenticated: " + User.Identity?.IsAuthenticated);
		foreach (Claim claim in User.Claims)
		{
			Console.WriteLine(claim.Type + " = " + claim.Value);
		}
		return Ok();
	}
    
    [HttpPost("send-email-code")]
	public async Task<IActionResult> SendEmailCode([FromBody] SendEmailCodeDto dto)
	{
		if (string.IsNullOrWhiteSpace(dto.email))
		{
			return ApiError(new
			{
				message = "Email is required."
			});
		}
		string email = dto.email.Trim().ToLower();
		if (await _verificationRepo.HasRecentCodeAsync(email))
		{
			return ApiError(new
			{
				verified = false,
				message = "Please wait before requesting another code."
			}, "Please wait before requesting another code.", 429);
		}
		string verifycode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
		DateTime expiresAt = DateTime.UtcNow.AddMinutes(10.0);
		await _verificationRepo.SaveCodeAsync(email, verifycode, expiresAt);
		await _emailService.SendAsync(email, "Verify your email", "<div><h2>Verify your email.</h2><div><p>\r\nHere is your email verification code:</p><br/><h4>" + verifycode + "</h4><div><br/><p>Just a heads up, this code will expire in 10 minutes for security reasons</p></div></div>");
		return ApiOk(new
		{
			userExists = false,
			sent = true
		});
	}

	[HttpPost("verify-email-code")]
	public async Task<IActionResult> VerifyEmailCode([FromBody] RegisterDto rgdtos)
	{
		if (string.IsNullOrWhiteSpace(rgdtos.email) || string.IsNullOrWhiteSpace(rgdtos.verifycode))
		{
			return ApiError(new
			{
				verified = false,
				message = "Email and verification code are required."
			});
		}
		if (!(await _verificationRepo.VerifyCodeAsync(rgdtos.email, rgdtos.verifycode)))
		{
			return ApiError(new
			{
				verified = false,
				message = "Invalid or expired code. Please resend the verification code and try again."
			});
		}
		switch (rgdtos.account_type?.ToLower())
		{
		case "tradesperson":
			rgdtos.user_type = 2;
			rgdtos.public_slug = GenerateSlug(rgdtos.business_name);
			break;
		case "customer":
			rgdtos.user_type = 1;
			break;
		case "admin":
			rgdtos.user_type = 9;
			break;
		default:
			return ApiError(new
			{
				message = "Invalid account type."
			});
		}
		RegisterResponse anyresult = await _auth.UserUpsertAsync(rgdtos);
		_cookieService.SetAuthCookies(Response, anyresult.token, anyresult.refresh_token);
		return ApiOk(anyresult);
	}

	[HttpPost("customer/verify-email-code")]
	public async Task<IActionResult> CustomerVerifyEmailCode([FromBody] RegisterDto rgdtos)
	{
		if (string.IsNullOrWhiteSpace(rgdtos.email) || string.IsNullOrWhiteSpace(rgdtos.verifycode))
		{
			return ApiError(new
			{
				verified = false,
				message = "Email and code are required."
			});
		}
		if (!(await _verificationRepo.VerifyCodeAsync(rgdtos.email, rgdtos.verifycode)))
		{
			return ApiError(new
			{
				verified = false,
				message = "Code verification failed"
			});
		}
		rgdtos.user_type = 1;
		RegisterResponse anyresult = await _auth.UserUpsertAsync(rgdtos);
		_cookieService.SetAuthCookies(Response, anyresult.token, anyresult.refresh_token);
		return ApiOk(anyresult);
	}

	[HttpPost("refresh")]
	public async Task<IActionResult> Refresh()
	{
		string refreshToken = Request.Cookies["refresh_token"];
		if (string.IsNullOrEmpty(refreshToken))
		{
			return ApiError(new
			{
				message = "Missing refresh token"
			});
		}
		RefreshResult result = await _auth.RefreshTokensAsync(refreshToken);
		if (!result.success)
		{
			return ApiError(new
			{
				message = "Invalid refresh token"
			});
		}
		_cookieService.SetAuthCookies(Response, result.token, result.refresh_token);
		return ApiOk(result);
	}

	[Authorize]
	[HttpGet("me")]
	public IActionResult Me()
	{
		Guid userId = GetUserId();
		var user = new
		{
			id = userId,
			email = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value,
			user_type = User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value
		};
		return ApiOk(user);
	}

	[HttpPost("login")]
	public async Task<IActionResult> Login(LoginDto lgdto)
	{
		RegisterResponse anyresult = await _auth.LoginAsync(lgdto);
		if (anyresult.token == null)
		{
			return ApiError(anyresult, "Invalid email or password", 401);
		}
		_cookieService.SetAuthCookies(Response, anyresult.token, anyresult.refresh_token);
		return ApiOk(anyresult);
	}

	[HttpPost("logout")]
	[AllowAnonymous]
	public async Task<IActionResult> Logout()
	{
		if (Request.Cookies.TryGetValue("refresh_token", out string refreshToken) && !string.IsNullOrWhiteSpace(refreshToken))
		{
			Guid userId = ((User.Identity?.IsAuthenticated ?? false) ? GetUserId() : Guid.Empty);
			RefreshToken anyRefreshToken = new RefreshToken
			{
				token = refreshToken,
				user_id = userId
			};
			try
			{
				await _auth.RevokeRefreshTokenByToken(anyRefreshToken);
			}
			catch
			{
			}
		}
		_cookieService.ClearAuthCookies(Response);
		return ApiOk(new
		{
			logged_out = true
		});
	}
}
