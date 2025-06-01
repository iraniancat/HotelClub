// src/HotelReservation.Api/Services/CurrentUserService.cs
using HotelReservation.Application.Contracts.Security; // برای ICurrentUserService و CustomClaimTypes
using Microsoft.AspNetCore.Http; // برای IHttpContextAccessor
using System;
using System.Linq;
using System.Security.Claims;

namespace HotelReservation.Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId
    {
        get
        {
            var userIdClaim = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdClaim, out var id) ? id : null;
        }
    }

    public string? SystemUserId => User?.FindFirstValue(ClaimTypes.Name); // یا JwtRegisteredClaimNames.Sub

    public string? FullName => User?.FindFirstValue(ClaimTypes.GivenName);

    public string? UserRole => User?.FindFirstValue(ClaimTypes.Role);

    public string? ProvinceCode => User?.FindFirstValue(CustomClaimTypes.ProvinceCode);

    public Guid? HotelId
    {
        get
        {
            var hotelIdClaim = User?.FindFirstValue(CustomClaimTypes.HotelId);
            return Guid.TryParse(hotelIdClaim, out var id) ? id : null;
        }
    }
    
    public string? DepartmentCode => User?.FindFirstValue("department_code"); // استفاده از نام Claim سفارشی

    public ClaimsPrincipal? GetUserPrincipal() => User;

    public bool IsInRole(string roleName) => User?.IsInRole(roleName) ?? false;

    public string? GetClaimValue(string claimType) => User?.FindFirstValue(claimType);
}