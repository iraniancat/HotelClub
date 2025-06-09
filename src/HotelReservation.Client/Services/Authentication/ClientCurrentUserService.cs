using HotelReservation.Application.Contracts.Security;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HotelReservation.Client.Services.Authentication;

public class ClientCurrentUserService : ICurrentUserService
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private ClaimsPrincipal? _currentUser;

    public ClientCurrentUserService(AuthenticationStateProvider authenticationStateProvider)
    {
        _authenticationStateProvider = authenticationStateProvider;
        // به تغییرات وضعیت احراز هویت گوش می‌دهیم تا کاربر فعلی را به‌روز کنیم
        _authenticationStateProvider.AuthenticationStateChanged += async (authStateTask) =>
        {
            var authState = await authStateTask;
            _currentUser = authState.User;
        };
    }

    // یک متد برای مقداردهی اولیه، چون سازنده نمی‌تواند async باشد
    public async Task InitializeAsync()
    {
        if (_currentUser == null)
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            _currentUser = authState.User;
        }
    }

    private ClaimsPrincipal User => _currentUser ?? new ClaimsPrincipal(new ClaimsIdentity());

    public bool IsAuthenticated => User.Identity?.IsAuthenticated ?? false;

    public Guid? UserId
    {
        get
        {
            var userIdClaimValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdClaimValue, out var id) ? id : null;
        }
    }

    public string? SystemUserId => User.FindFirstValue(ClaimTypes.Name);
    public string? FullName => User.FindFirstValue(ClaimTypes.GivenName);
    public string? UserRole => User.FindFirstValue(ClaimTypes.Role);
    public string? ProvinceCode => User.FindFirstValue(CustomClaimTypes.ProvinceCode);
    public Guid? HotelId
    {
        get
        {
            var hotelIdClaim = User.FindFirstValue(CustomClaimTypes.HotelId);
            return Guid.TryParse(hotelIdClaim, out var id) ? id : null;
        }
    }
    public string? DepartmentCode => User.FindFirstValue("department_code");

    public ClaimsPrincipal? GetUserPrincipal() => User;
    public bool IsInRole(string roleName) => User.IsInRole(roleName);
    public string? GetClaimValue(string claimType) => User.FindFirstValue(claimType);
}
