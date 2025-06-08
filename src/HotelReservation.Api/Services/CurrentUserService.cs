// src/HotelReservation.Api/Services/CurrentUserService.cs
using HotelReservation.Application.Contracts.Security;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq; // برای Any()
using System.Security.Claims;
using Microsoft.Extensions.Logging; // <<-- اضافه کردن using برای ILogger

namespace HotelReservation.Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CurrentUserService> _logger; // <<-- اضافه کردن فیلد لاگر

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, ILogger<CurrentUserService> logger) // <<-- تزریق لاگر
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger)); // <<-- مقداردهی لاگر
        
        // لاگ کردن وضعیت اولیه هنگام ایجاد سرویس
        _logger.LogInformation("CurrentUserService instantiated. HttpContext is {HttpContextStatus}. User is {UserStatus}.",
            _httpContextAccessor.HttpContext == null ? "NULL" : "NOT NULL",
            User == null ? "NULL" : "NOT NULL");
        LogAllClaims("Constructor"); // لاگ کردن تمام Claimها هنگام ایجاد
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated
    {
        get
        {
            var isAuthenticated = User?.Identity?.IsAuthenticated ?? false;
            // _logger.LogInformation("CurrentUserService - IsAuthenticated check: {IsAuthenticatedValue}", isAuthenticated);
            return isAuthenticated;
        }
    }

    public Guid? UserId
    {
         get
        {
            if (User == null) return null;

            // ابتدا به دنبال Claim با نوع NameIdentifier بگردید که مقدارش یک Guid معتبر باشد.
            var guidNameIdentifierClaim = User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier && Guid.TryParse(c.Value, out _));

            if (guidNameIdentifierClaim != null)
            {
                // _logger.LogInformation("CurrentUserService - Found valid Guid NameIdentifier claim for UserId: '{ClaimValue}'", guidNameIdentifierClaim.Value);
                return Guid.Parse(guidNameIdentifierClaim.Value); // اینجا مطمئن هستیم که Parse موفقیت‌آمیز است
            }

            _logger.LogWarning("CurrentUserService - No valid Guid found for NameIdentifier claim. All NameIdentifier claims: [{AllNameIdClaims}]", 
                string.Join(", ", User.FindAll(ClaimTypes.NameIdentifier).Select(c => c.Value)));
            return null;
        }
    }

    public string? SystemUserId => User?.FindFirstValue(ClaimTypes.Name);

    public string? FullName => User?.FindFirstValue(ClaimTypes.GivenName);

    public string? UserRole
    {
        get
        {
            var roleClaimValue = User?.FindFirstValue(ClaimTypes.Role);
            // _logger.LogInformation("CurrentUserService - UserRole check. Role Claim: '{UserRoleClaimValue}'", roleClaimValue);
            return roleClaimValue;
        }
    }

    public string? ProvinceCode => User?.FindFirstValue(CustomClaimTypes.ProvinceCode);

    public Guid? HotelId
    {
        get
        {
            var hotelIdClaim = User?.FindFirstValue(CustomClaimTypes.HotelId);
            return Guid.TryParse(hotelIdClaim, out var id) ? id : null;
        }
    }
    
    public string? DepartmentCode => User?.FindFirstValue("department_code");

    public ClaimsPrincipal? GetUserPrincipal() => User;

    public bool IsInRole(string roleName) => User?.IsInRole(roleName) ?? false;

    public string? GetClaimValue(string claimType) => User?.FindFirstValue(claimType);

    // متد کمکی برای لاگ کردن تمام Claimها
    private void LogAllClaims(string contextMessage)
    {
        if (User != null && User.Claims.Any())
        {
            _logger.LogInformation("CurrentUserService ({Context}): All claims for the current user:", contextMessage);
            foreach (var claim in User.Claims)
            {
                _logger.LogInformation("CurrentUserService ({Context}): Claim Type: [{ClaimType}], Claim Value: [{ClaimValue}]", contextMessage, claim.Type, claim.Value);
            }
        }
        else
        {
            _logger.LogWarning("CurrentUserService ({Context}): No claims found for the current user or User principal is null.", contextMessage);
        }
        // لاگ کردن مقادیر کلیدی پس از بررسی Claimها
        _logger.LogInformation("CurrentUserService ({Context}): IsAuthenticated: {IsAuthenticated}, UserId: {UserId}, UserRole: {UserRole}", 
            contextMessage, IsAuthenticated, UserId, UserRole);
    }
}