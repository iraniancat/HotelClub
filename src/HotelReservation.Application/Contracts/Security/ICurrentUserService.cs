// src/HotelReservation.Application/Contracts/Security/ICurrentUserService.cs
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace HotelReservation.Application.Contracts.Security;

public interface ICurrentUserService
{
    bool IsAuthenticated { get; }
    Guid? UserId { get; } // شناسه داخلی کاربر (Guid)
    string? SystemUserId { get; } // نام کاربری (SystemUserId)
    string? FullName { get; }
    string? UserRole { get; } // نام نقش
    
    // Claim های سفارشی
    string? ProvinceCode { get; }
    Guid? HotelId { get; }
    string? DepartmentCode { get; }

    ClaimsPrincipal? GetUserPrincipal(); // برای موارد پیشرفته‌تر که به ClaimsPrincipal کامل نیاز است
    bool IsInRole(string roleName);
    string? GetClaimValue(string claimType);
}