// src/HotelReservation.Client/Auth/CustomAuthenticationStateProvider.cs
using Blazored.LocalStorage;
using HotelReservation.Client.Services;
using HotelReservation.Application.DTOs.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HotelReservation.Application.Contracts.Security; // برای CustomClaimTypes
using Microsoft.Extensions.Logging;
using System.Text.Json; // برای JsonSerializerOptions در صورت نیاز به سریالایز کردن دستی

namespace HotelReservation.Client.Auth;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly IApiClientService _apiClientService;
    private readonly ILogger<CustomAuthenticationStateProvider> _logger;
    private readonly ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());
    private const string UserAuthInfoStorageKey = "userAuthInfo";

    public CustomAuthenticationStateProvider(
        ILocalStorageService localStorage,
        IApiClientService apiClientService,
        ILogger<CustomAuthenticationStateProvider> logger)
    {
        _localStorage = localStorage;
        _apiClientService = apiClientService;
        _logger = logger;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        _logger.LogInformation("CustomAuthenticationStateProvider: GetAuthenticationStateAsync called.");
        LoginResponseDto? userAuthInfo = null;
        try
        {
            // خواندن LoginResponseDto که شامل توکن و تاریخ انقضای اولیه است
            userAuthInfo = await _localStorage.GetItemAsync<LoginResponseDto?>(UserAuthInfoStorageKey);
        }
        catch (JsonException jsonEx) // اگر داده ذخیره شده در LocalStorage معتبر نباشد
        {
            _logger.LogError(jsonEx, "Failed to deserialize UserAuthInfo from LocalStorage. Removing invalid item.");
            await _localStorage.RemoveItemAsync(UserAuthInfoStorageKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading UserAuthInfo from LocalStorage in GetAuthenticationStateAsync.");
        }

        if (userAuthInfo == null || string.IsNullOrWhiteSpace(userAuthInfo.Token))
        {
            _logger.LogInformation("CustomAuthenticationStateProvider: No valid user auth info or token found in local storage.");
            _apiClientService.ClearAuthorizationHeader();
            return new AuthenticationState(_anonymous);
        }

        // پارس کردن توکن و بررسی انقضای دقیق‌تر از خود توکن
        var tokenHandler = new JwtSecurityTokenHandler();
        if (!tokenHandler.CanReadToken(userAuthInfo.Token))
        {
            _logger.LogWarning("CustomAuthenticationStateProvider: Cannot read the token from local storage. Token: {Token}", userAuthInfo.Token);
            await ClearAuthDataAndNotify();
            return new AuthenticationState(_anonymous);
        }

        JwtSecurityToken jwtToken;
        try
        {
            jwtToken = tokenHandler.ReadJwtToken(userAuthInfo.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CustomAuthenticationStateProvider: Error reading/parsing JWT token. Token: {Token}", userAuthInfo.Token);
            await ClearAuthDataAndNotify();
            return new AuthenticationState(_anonymous);
        }

        if (jwtToken.ValidTo < DateTime.UtcNow)
        {
            _logger.LogInformation("CustomAuthenticationStateProvider: Token is expired (Expiry: {ExpiryDate}). Clearing auth data.", jwtToken.ValidTo);
            await ClearAuthDataAndNotify();
            return new AuthenticationState(_anonymous);
        }

        var claimsPrincipal = CreateClaimsPrincipalFromTokenObject(jwtToken);
        if (claimsPrincipal.Identity != null && claimsPrincipal.Identity.IsAuthenticated)
        {
            _logger.LogInformation("CustomAuthenticationStateProvider: User is authenticated. Name: {UserName}, Role: {UserRole}. Setting auth header.",
                claimsPrincipal.Identity.Name, claimsPrincipal.FindFirstValue(ClaimTypes.Role));
            _apiClientService.SetAuthorizationHeader(userAuthInfo.Token);
            return new AuthenticationState(claimsPrincipal);
        }

        _logger.LogWarning("CustomAuthenticationStateProvider: Token found but could not create authenticated principal. Clearing auth data.");
        await ClearAuthDataAndNotify(); // اگر نتوانستیم Principal معتبر بسازیم
        return new AuthenticationState(_anonymous);
    }

    public async Task MarkUserAsAuthenticatedAsync(LoginResponseDto authInfo)
    {
        if (string.IsNullOrWhiteSpace(authInfo?.Token))
        {
            _logger.LogWarning("MarkUserAsAuthenticatedAsync called with null or empty token.");
            await ClearAuthDataAndNotify(); // اگر اطلاعات ناقص است، لاگ اوت کن
            return;
        }

        JwtSecurityToken jwtToken;
        var tokenHandler = new JwtSecurityTokenHandler();
        if (!tokenHandler.CanReadToken(authInfo.Token))
        {
            _logger.LogWarning("MarkUserAsAuthenticatedAsync: Cannot read the provided token. Token: {Token}", authInfo.Token);
            await ClearAuthDataAndNotify();
            return;
        }
        try
        {
            jwtToken = tokenHandler.ReadJwtToken(authInfo.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MarkUserAsAuthenticatedAsync: Error reading/parsing JWT token. Token: {Token}", authInfo.Token);
            await ClearAuthDataAndNotify();
            return;
        }

        // ذخیره اطلاعات کامل پاسخ لاگین (شامل توکن و تاریخ انقضا از سرور)
        await _localStorage.SetItemAsync(UserAuthInfoStorageKey, authInfo);

        var claimsPrincipal = CreateClaimsPrincipalFromTokenObject(jwtToken);
        _apiClientService.SetAuthorizationHeader(authInfo.Token); // تنظیم هدر قبل از Notify

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
        _logger.LogInformation("User {SystemUserId} marked as authenticated. Notified auth state changed.", authInfo.SystemUserId);
    }

    public async Task MarkUserAsLoggedOutAsync()
    {
        await ClearAuthDataAndNotify();
    }

    private async Task ClearAuthDataAndNotify()
    {
        await _localStorage.RemoveItemAsync(UserAuthInfoStorageKey);
        _apiClientService.ClearAuthorizationHeader();
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
        _logger.LogInformation("Authentication data cleared and auth state changed to anonymous.");
    }

    // در CustomAuthenticationStateProvider.cs
    private ClaimsPrincipal CreateClaimsPrincipalFromTokenObject(JwtSecurityToken jwtToken)
    {
        if (jwtToken == null)
        {
            _logger.LogWarning("CreateClaimsPrincipalFromTokenObject: jwtToken is null, returning anonymous principal.");
            return _anonymous;
        }

        var rawClaimsFromJwt = jwtToken.Claims.ToList(); // Claimهای خام از توکن
        var identityClaims = new List<Claim>(); // لیستی برای Claimهای نهایی که به ClaimsIdentity پاس داده می‌شود

        _logger.LogInformation("CreateClaimsPrincipalFromTokenObject: Processing {ClaimCount} raw claims from JWT:", rawClaimsFromJwt.Count);
        foreach (var claim in rawClaimsFromJwt)
        {
            _logger.LogInformation("--> Raw Claim: Original Type: [{OriginalType}], Value: [{ClaimValue}]", claim.Type, claim.Value);
        }

        // ۱. نگاشت NameIdentifier (شناسه کاربر Guid)
        var userIdClaim = rawClaimsFromJwt.FirstOrDefault(c => c.Type == "nameid"); // توکن شما "nameid" را برای Guid Id دارد
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out _))
        {
            identityClaims.Add(new Claim(ClaimTypes.NameIdentifier, userIdClaim.Value, userIdClaim.ValueType, userIdClaim.Issuer, userIdClaim.OriginalIssuer, userIdClaim.Subject));
            _logger.LogInformation("Mapped UserID (NameIdentifier): {UserIDValue}", userIdClaim.Value);
        }
        else
        {
            _logger.LogWarning("UserID claim (type 'nameid' with Guid value) not found or invalid in token.");
        }

        // ۲. نگاشت Name (برای User.Identity.Name - معمولاً نام کاربری)
        // توکن شما "unique_name" و "sub" را برای SystemUserId (39207005) دارد.
        // ClaimTypes.Name معمولاً برای یک نام قابل نمایش استفاده می‌شود، اما می‌تواند نام کاربری هم باشد.
        var nameForIdentityClaim = rawClaimsFromJwt.FirstOrDefault(c => c.Type == "unique_name") ?? // این در توکن شما هست
                                   rawClaimsFromJwt.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
        if (nameForIdentityClaim != null)
        {
            identityClaims.Add(new Claim(ClaimTypes.Name, nameForIdentityClaim.Value, nameForIdentityClaim.ValueType, nameForIdentityClaim.Issuer, nameForIdentityClaim.OriginalIssuer, nameForIdentityClaim.Subject));
            _logger.LogInformation("Mapped Name for Identity (ClaimTypes.Name): {NameValue} from original type {OriginalType}", nameForIdentityClaim.Value, nameForIdentityClaim.Type);
        }
        else
        {
            _logger.LogWarning("Name claim (unique_name/sub) not found for Identity.Name.");
        }

        // ۳. نگاشت GivenName (نام کامل)
        var givenNameClaim = rawClaimsFromJwt.FirstOrDefault(c => c.Type == "given_name"); // توکن شما "given_name" را دارد
        if (givenNameClaim != null)
        {
            identityClaims.Add(new Claim(ClaimTypes.GivenName, givenNameClaim.Value, givenNameClaim.ValueType, givenNameClaim.Issuer, givenNameClaim.OriginalIssuer, givenNameClaim.Subject));
            _logger.LogInformation("Mapped GivenName (ClaimTypes.GivenName): {GivenNameValue}", givenNameClaim.Value);
        }
        else
        {
            _logger.LogWarning("GivenName claim (type 'given_name') not found.");
        }

        // ۴. نگاشت Role
        var roleClaimFromJwt = rawClaimsFromJwt.FirstOrDefault(c => c.Type == "role"); // توکن شما "role" را دارد
        if (roleClaimFromJwt != null && !string.IsNullOrEmpty(roleClaimFromJwt.Value))
        {
            // ایجاد یک Claim جدید با Type استاندارد ClaimTypes.Role
            identityClaims.Add(new Claim(ClaimTypes.Role, roleClaimFromJwt.Value, roleClaimFromJwt.ValueType, roleClaimFromJwt.Issuer, roleClaimFromJwt.OriginalIssuer, roleClaimFromJwt.Subject));
            _logger.LogInformation("Mapped Role (ClaimTypes.Role): {RoleValue} from original type 'role'", roleClaimFromJwt.Value);
        }
        else
        {
            _logger.LogWarning("Role claim (type 'role') not found or is empty in token.");
        }

        // ۵. افزودن سایر Claimهای سفارشی (مانند province_code, department_code, hotel_id) به همان صورتی که هستند
        var otherCustomClaims = rawClaimsFromJwt.Where(c =>
            c.Type != "jti" && c.Type != "nameid" && c.Type != "sub" && c.Type != "unique_name" &&
            c.Type != "given_name" && c.Type != "role" &&
            c.Type != JwtRegisteredClaimNames.Nbf && c.Type != JwtRegisteredClaimNames.Exp &&
            c.Type != JwtRegisteredClaimNames.Iat && c.Type != JwtRegisteredClaimNames.Iss &&
            c.Type != JwtRegisteredClaimNames.Aud
        ).ToList();

        identityClaims.AddRange(otherCustomClaims);
        _logger.LogInformation("Added {Count} other custom claims to identity.", otherCustomClaims.Count);


        // ایجاد ClaimsIdentity با لیست Claimهای پردازش شده و مشخص کردن صریح nameClaimType و roleClaimType
        var identity = new ClaimsIdentity(
            identityClaims,          // لیست Claimهای نهایی ما
            "jwtAuth",               // نوع احراز هویت
            ClaimTypes.Name,         // به ClaimsIdentity می‌گوید که برای User.Identity.Name از Claim با Type = ClaimTypes.Name استفاده کند
            ClaimTypes.Role          // به ClaimsIdentity می‌گوید که برای User.IsInRole() از Claim با Type = ClaimTypes.Role استفاده کند
        );

        _logger.LogInformation("Final ClaimsIdentity created. IsAuthenticated: {IsAuth}, Identity.Name: {IdentityName}, AuthType: {AuthType}",
            identity.IsAuthenticated, identity.Name, identity.AuthenticationType);

        _logger.LogInformation("Final Identity - FullName (from ClaimTypes.GivenName): {FullName}", identity.FindFirst(ClaimTypes.GivenName)?.Value ?? "(null)");
        _logger.LogInformation("Final Identity - Role (from ClaimTypes.Role): {Role}", identity.FindFirst(ClaimTypes.Role)?.Value ?? "(null)");
        _logger.LogInformation("Final Identity - Check for 'SuperAdmin' role: {HasRole}", identity.HasClaim(ClaimTypes.Role, "SuperAdmin"));


        if (!identity.IsAuthenticated && identityClaims.Any())
        {
            _logger.LogWarning("Identity has claims but IsAuthenticated is false. This is unexpected if authType is set and claims exist.");
        }

        return new ClaimsPrincipal(identity);
    }
}