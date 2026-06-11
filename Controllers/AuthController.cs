using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TradePlatform.Api.DTOs;
using TradePlatform.Api.Identity;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services;

namespace TradePlatform.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseController
    {


        private readonly IAuthService _auth;
        private readonly IUsersRepository _urepo;
        private readonly IEmailVerificationRepository _verificationRepo;
        private readonly IEmailService _emailService;
        private readonly IIdentityService _identity;
        public AuthController(IAuthService auth
            , IUsersRepository urepo
            , IEmailVerificationRepository verificationRepo
            , IEmailService emailService
            , IIdentityService identity  
            ,IHttpContextAccessor http
        ) : base(http)
        {
            _urepo = urepo;
            _auth = auth;
            _verificationRepo = verificationRepo;
            _emailService = emailService;
            _identity = identity;
        }        

        [HttpPost("send-email-code")]
        public async Task<IActionResult> SendEmailCode([FromBody] SendEmailCodeDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.email))
                return ApiError(new { message = "Email is required." });

            var email = dto.email.Trim().ToLower();

            // Generate 6-digit OTP
            var hasRecentCode = await _verificationRepo.HasRecentCodeAsync(email);
            if (hasRecentCode)
            {
                return ApiError(new { verified = false, message = "Please wait before requesting another code." },429);               
            };
            var verifycode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            var expiresAt = DateTime.UtcNow.AddMinutes(10);

            // Save OTP using stored procedure
            await _verificationRepo.SaveCodeAsync(email, verifycode, expiresAt);

            // Send email
            await _emailService.SendAsync(
                email,
                "Verify your email",
                $"<div><h2>Verify your email.</h2><div><p>\r\nHere is your email verification code:</p><br/><h4>{verifycode}</h4><div><br/><p>Just a heads up, this code will expire in 10 minutes for security reasons</p></div></div>"
            );
            return ApiOk(new { userExists = false, sent = true });
        }

        private async Task BuildAuthResponseCookieAsync(string access_token, string refresh_token)
        {
            Response.Cookies.Append("auth_token", access_token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/"
            });

            Response.Cookies.Append("refresh_token", refresh_token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/"
            });
        }

        // ---------------------------------------------------------
        // VERIFY EMAIL CODE
        // ---------------------------------------------------------
        [HttpPost("verify-email-code")]
        public async Task<IActionResult> VerifyEmailCode([FromBody] RegisterDto rgdtos)
        {
            if (string.IsNullOrWhiteSpace(rgdtos.email) || string.IsNullOrWhiteSpace(rgdtos.verifycode))
                return ApiError(new { verified = false, message="Email and verifycation code are required."});
            //Console.WriteLine("DTO RECEIVED: " + JsonConvert.SerializeObject(rgdtos));
            rgdtos.public_slug = GenerateSlug(rgdtos.business_name);
            var verified = await _verificationRepo.VerifyCodeAsync(rgdtos.email, rgdtos.verifycode);

            if (!verified)
            {
                var  BadRequest=new
                {
                    verified = false,
                    message = "Invalid or expired code. Please resend the verification code and try again."
                };
                return ApiError(BadRequest);
            };
            switch (rgdtos.account_type?.ToLower())
            {
                case "tradesperson":
                    rgdtos.user_type = (int)UserType.tradesperson;
                    //rgdtos.address_type_id = 3;/*tuser business address*/
                    break;

                case "customer":
                    rgdtos.user_type = (int)UserType.customer;
                    //rgdtos.address_type_id = 1;/*customer home address*/
                    break;
                default:
                    return ApiError(new { message = "Invalid account type." });
            }

            var anyresult = await _auth.UserUpsertAsync(rgdtos);
            await BuildAuthResponseCookieAsync(anyresult.token, anyresult.refresh_token);
            return ApiOk(anyresult);
           
        }
        // ---------------------------------------------------------
        // VERIFY EMAIL CODE
        // ---------------------------------------------------------
        [HttpPost("customer/verify-email-code")]
        public async Task<IActionResult> CustomerVerifyEmailCode([FromBody] RegisterDto rgdtos)
        {
            if (string.IsNullOrWhiteSpace(rgdtos.email) || string.IsNullOrWhiteSpace(rgdtos.verifycode))
                return ApiError(new { verified = false, message = "Email and code are required." });
           
            //Console.WriteLine("DTO RECEIVED: " + JsonConvert.SerializeObject(rgdtos));

            var verified = await _verificationRepo.VerifyCodeAsync(rgdtos.email, rgdtos.verifycode);

            if (!verified)
            {
                return ApiError(new { verified = false, message = "Code verification failed" });
               
            };
            //rgdtos.address_type_id = 2;  /*customer work place*/
            rgdtos.user_type = (int)UserType.customer;
            var anyresult = await _auth.UserUpsertAsync(rgdtos);
            await BuildAuthResponseCookieAsync(anyresult.token, anyresult.refresh_token);
            return ApiOk(anyresult);

        }

        [HttpPost("refresh")]        
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies["refresh_token"];
            if (string.IsNullOrEmpty(refreshToken))
                return ApiError(new { message = "Missing refresh token" });

            var result = await _auth.RefreshTokensAsync(refreshToken);

            if (!result.Success)
                return ApiError(new { message = "Invalid refresh token" });

            await BuildAuthResponseCookieAsync(result.AccessToken, result.RefreshToken);            

            return ApiOk(result);
        }

        [Authorize] // Requires JWT
        [HttpGet("me")]
        public IActionResult Me()
        {
            // Extract user ID from JWT claims
            var userId = _identity.GetUserId();
            var user = new
            {
                id = userId,
                email = User.FindFirst(ClaimTypes.Email)?.Value,
                user_type = User.FindFirst(ClaimTypes.Role)?.Value
            };
            return Ok(user);
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto lgdto)
        {
            var anyresult=await _auth.LoginAsync(lgdto);
            if (anyresult.token==null)
            {
                return ApiError(anyresult);
            }
            await BuildAuthResponseCookieAsync(anyresult.token, anyresult.refresh_token);
            return ApiOk(anyresult);
            
        }
    }
}