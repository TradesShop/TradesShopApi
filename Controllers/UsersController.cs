using BCrypt.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using TradePlatform.Api.DTOs.users;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories;
using TradePlatform.Api.Repositories.Implementations;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services.users;

namespace TradePlatform.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : BaseController
    {
        private readonly IUsersRepository _repo;
        private readonly IUsersService _usersService;
        private readonly IEmailVerificationRepository _verificationRepo;
        public UsersController(IUsersRepository repo,
             IUsersService usersService
            , IEmailVerificationRepository verificationRepo
            , IHttpContextAccessor http
        ) : base(http)
        {
            _repo = repo;
            _usersService = usersService;
            _verificationRepo = verificationRepo;
        }

        [HttpGet("account")]
        public async Task<IActionResult> UserAccountGetAsync()
        {
            var (callerId, callerType) = GetIdentity();           
            var anyaccount = await _usersService.UserAccountGetAsync(callerId);
            return ApiOk(anyaccount);

        }
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            var (callerId, callerType) = GetIdentity();

            var anyresult = await _usersService.ChangePasswordAsync(callerId, dto.old_password,dto.new_password);
            if (!anyresult.success)
                return ApiError(anyresult);
            return ApiOk(anyresult);
            
        }
        [HttpGet("checkuser")]
        public async Task<IActionResult> CheckUser([FromQuery] string email)
        {
            
            var anyuser = await _repo.GetByEmailAsync(email, (int)UserType.customer);
            return ApiOk(anyuser);
            
        }
        [HttpGet("checktradeuser")]
        public async Task<IActionResult> CheckTradesPerson([FromQuery] string email)
        {
            var anyuser = await _repo.GetByEmailAsync(email, (int)UserType.tradesperson);
            return ApiOk(anyuser);

            //return Ok(new { exists = 0 });
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var (callerId, callerType) = GetIdentity();
            var anyuser = await _repo.GetUserByIdAsync(callerId);
            return ApiOk(anyuser);
            
        }

        [HttpPost("upsert")]
        public async Task<IActionResult> UpdateAnyUserAsync(UserDto uDto)
        {
            if (string.IsNullOrWhiteSpace(uDto.email) || string.IsNullOrWhiteSpace(uDto.verifycode))
                return ApiError(new { verified = false, message = "Email and verifycation code are required." });
            var verified = await _verificationRepo.VerifyCodeAsync(uDto.email, uDto.verifycode);
            if (!verified)
            {
                var BadRequest = new
                {
                    verified = false,
                    message = "Invalid or expired code. Please resend the verification code and try again."
                };
                return ApiError(BadRequest);
            }
            ;
            var (user_id, callerType) = GetIdentity();
            uDto.id = user_id;
            var anyresult = await _usersService.UpdateAnyUserAsync(uDto);
            if (anyresult==null)
               return ApiError(anyresult);
            return ApiOk(anyresult);

        }
    }
}
