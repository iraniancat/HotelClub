// src/HotelReservation.Infrastructure/Authentication/JwtTokenGeneratorService.cs
using HotelReservation.Application.Contracts.Infrastructure;
using HotelReservation.Application.Contracts.Security; // برای CustomClaimTypes
using HotelReservation.Application.DTOs.Authentication;
using HotelReservation.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging; // اضافه کردن لاگر
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HotelReservation.Infrastructure.Authentication;

public class JwtTokenGeneratorService : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtTokenGeneratorService> _logger;

    public JwtTokenGeneratorService(IConfiguration configuration, ILogger<JwtTokenGeneratorService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public LoginResponseDto GenerateToken(User user) // کاربر باید با Role, Province, AssignedHotel واکشی شده باشد
    {
        var issuer = _configuration["JwtSettings:Issuer"];
        var audience = _configuration["JwtSettings:Audience"];
        var key = _configuration["JwtSettings:Key"]; // این کلید باید بسیار امن و طولانی باشد
        var durationInMinutesSetting = _configuration["JwtSettings:DurationInMinutes"];

        if (string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience) || string.IsNullOrEmpty(key) || !int.TryParse(durationInMinutesSetting, out int durationInMinutes) || durationInMinutes <= 0)
        {
            _logger.LogError("JWT settings are not configured correctly in appsettings.json. Issuer, Audience, Key, or DurationInMinutes is missing or invalid.");
            throw new InvalidOperationException("تنظیمات JWT برای صدور توکن به درستی پیکربندی نشده است.");
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // var claims = new List<Claim>
        // {
        //     new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),      // Token ID
        //     new Claim(JwtRegisteredClaimNames.Sub, user.SystemUserId),              // Subject (Username)
        //     new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),             // User's unique ID
        //     new Claim(ClaimTypes.Name, user.SystemUserId),                        // Username (can also be FullName)
        //     new Claim(JwtRegisteredClaimNames.GivenName, user.FullName ?? string.Empty), // FullName
        //     // سایر Claimهای استاندارد مانند Email اگر وجود دارد:
        //     // new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
        // };
          var claims = new List<Claim>
           {
               new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
               // User.Id (Guid PK) را به عنوان NameIdentifier اصلی قرار دهید
               new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), 
               // SystemUserId (نام کاربری برای ورود) را به عنوان Subject و Name قرار دهید
               new Claim(JwtRegisteredClaimNames.Sub, user.SystemUserId), 
               new Claim(ClaimTypes.Name, user.SystemUserId), // این معمولاً برای User.Identity.Name استفاده می‌شود
               new Claim(JwtRegisteredClaimNames.GivenName, user.FullName ?? string.Empty),
           };

        if (user.Role != null && !string.IsNullOrEmpty(user.Role.Name))
        {
            claims.Add(new Claim(ClaimTypes.Role, user.Role.Name));
        }
        else
        {
            _logger.LogWarning("Role name is missing for user {UserId}. Role claim will not be added.", user.Id);
            // تصمیم بگیرید که آیا در این حالت باید خطا پرتاب شود یا خیر.
            // برای امنیت، اگر نقش حیاتی است، بهتر است خطا پرتاب شود.
            // throw new InvalidOperationException($"نقش برای کاربر '{user.SystemUserId}' به درستی تنظیم نشده است.");
        }

        if (!string.IsNullOrEmpty(user.ProvinceCode))
        {
            claims.Add(new Claim(CustomClaimTypes.ProvinceCode, user.ProvinceCode));
        }

        if (user.HotelId.HasValue && user.HotelId.Value != Guid.Empty)
        {
            claims.Add(new Claim(CustomClaimTypes.HotelId, user.HotelId.Value.ToString()));
        }
        
        if (!string.IsNullOrEmpty(user.DepartmentCode))
        {
            claims.Add(new Claim("department_code", user.DepartmentCode)); // نام Claim سفارشی برای کد دپارتمان
        }


        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(durationInMinutes),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(securityToken);

        _logger.LogInformation("JWT token generated for user {SystemUserId} with ID {UserId} and Role {RoleName}.", 
            user.SystemUserId, user.Id, user.Role?.Name ?? "N/A");

        return new LoginResponseDto
        {
            Token = tokenString,
            Expiration = tokenDescriptor.Expires.Value, // تاریخ انقضا به وقت UTC
            UserId = user.Id,
            SystemUserId = user.SystemUserId,
            FullName = user.FullName ?? string.Empty,
            Role = user.Role?.Name ?? "نقش نامشخص"
        };
    }
}