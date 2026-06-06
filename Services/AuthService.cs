using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json.Linq;
using System;
using System.Security.Claims;
using TradePlatform.Api.DTOs;
using TradePlatform.Api.DTOs.users;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Implementations;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Services
{
    

    public class AuthService : IAuthService
    {
        private readonly IUsersRepository _users;
        private readonly PasswordHashingService _passwordHashing;
        private readonly IUserAddressRepository _addresses;
        //private readonly ICustomersRepository _customers;
        private readonly ITradespersonsRepository _tradespersons;
        private readonly IPasswordHasher _hasher;
        private readonly IJwtTokenService _jwtService;
        private readonly IRefreshTokenRepository _refreshtoken;

        public AuthService(
            IUsersRepository users,
            IUserAddressRepository addresses,
            PasswordHashingService passwordHashing,
            ITradespersonsRepository tradespersons,
            IPasswordHasher hasher,
            IJwtTokenService jwt,
            IRefreshTokenRepository refreshtoken)
        {
            _users = users;
            _addresses = addresses;
            _passwordHashing = passwordHashing;
            _tradespersons = tradespersons;
            _hasher = hasher;
            _jwtService = jwt;
            _refreshtoken = refreshtoken;
        }
       

       
        private async Task<(string accessToken, string refreshToken)> GenerateAuthTokensAsync(User user)
        {
            var accessToken = _jwtService.GenerateToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            var refreshEntity = new RefreshToken
            {
                user_id = user.id,
                token = refreshToken,
                expires_at = DateTime.UtcNow.AddDays(7),
                isrevoked = false
            };

            await _refreshtoken.AddAsync(refreshEntity);

            return (accessToken, refreshToken);
        }
        private async Task<RegisterResponse> BuildAuthResponse(User? user, string action)
        {
            if(user == null)
            {
                return new RegisterResponse
                {
                    token = null,
                    refresh_token = null,
                    User = null,
                    message = action == "login" ? "Invalid email or password" : "Registration failed"
                };
            }
            var (accessToken, refreshToken) = await GenerateAuthTokensAsync(user);
            return new RegisterResponse
            {
                token = accessToken,
                refresh_token = refreshToken,
                User = new
                {
                    user.id,
                    user.email,
                    user.user_type,                   
                    user.phone,
                    user.firstname,
                    user.lastname
                },
                message = action == "login"?"Login successful" : "Registration successful"
            };
        }
        
        public async Task<RefreshResult> RefreshTokensAsync(string refreshToken)
        {
            // 1) Look up refresh token in DB
            var stored = await _refreshtoken.GetByTokenAsync(refreshToken);
            if (stored == null || stored.isrevoked || stored.expires_at <= DateTime.UtcNow)
                return RefreshResult.Fail();

            // 2) Get user
            var user = await _users.GetByIdAsync(stored.user_id);
            if (user == null)
                return RefreshResult.Fail();

            // 3) Generate new access + refresh tokens
            var tokens = await GenerateAuthTokensAsync(user);

            // 5) Revoke old one
            stored.isrevoked = true;
            await _refreshtoken.UpdateAsync(stored);

            return RefreshResult.Ok(tokens.accessToken, tokens.refreshToken);
        }

        public async Task<RegisterResponse> UserUpsertAsync(RegisterDto reg_dto)
        {
            //var existing = await _users.GetByEmailAsync(dto.email,dto.user_type);
            //if (existing != null)
              //  throw new Exception("Email already exists.");

            var user = new UserDto
            {
                firstname = reg_dto.firstname,
                lastname = reg_dto.lastname,
                email = reg_dto.email,
                password_hash = _passwordHashing.HashToBase64(reg_dto.password_hash),
                phone = reg_dto.phone,
                user_type = (int)(UserType)reg_dto.user_type               
            };
            //Console.WriteLine("Before RegisterAsync");
            var anyuser = await _users.UpdateAnyUserAsync(user);
           // Console.WriteLine("After RegisterAsync");
            anyuser.verified = true;
            reg_dto.user_id = anyuser.id;
           
            if (reg_dto.user_type == 1)
            {
                anyuser.customer_id=await _addresses.CreateCustomerProfileAsync(reg_dto);
            }
            else if (reg_dto.user_type == 2)
            {
                anyuser.business_id=await _addresses.CreateTradeUserBusinessAsync(reg_dto);
            }
            return await BuildAuthResponse(anyuser,"register");          
        }

        public async Task<RegisterResponse> LoginAsync(LoginDto dto)
        {
          
            var hashed = _passwordHashing.HashToBase64(dto.password); 
            var anyuser = await _users.LoginAsync(dto.email, hashed);
            return await BuildAuthResponse(anyuser, "login");

        }
    }
}
