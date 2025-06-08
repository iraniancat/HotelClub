// src/HotelReservation.Client/Services/Authentication/AuthService.cs
using HotelReservation.Application.DTOs.Authentication;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization; // برای AuthenticationStateProvider
using MudBlazor;
using System;
using System.Net.Http.Json; // برای خواندن خطا از پاسخ (اگر لازم باشد)
using System.Threading.Tasks;
using HotelReservation.Client.Auth; // برای کست به CustomAuthenticationStateProvider

namespace HotelReservation.Client.Services.Authentication;

public class AuthService : IAuthService
{
    private readonly IApiClientService _apiClient;
    private readonly NavigationManager _navigationManager;
    private readonly ILocalStorageService _localStorage; // برای پاک کردن اولیه یا موارد خاص
    private readonly ISnackbar _snackbar;
    private readonly CustomAuthenticationStateProvider _customAuthProvider; // تغییر نوع به کلاس مشخص

    // کلید ذخیره‌سازی را می‌توان در CustomAuthenticationStateProvider متمرکز کرد
    private const string UserAuthInfoStorageKey = "userAuthInfo";

    public event Action? AuthenticationStateChanged;

    public AuthService(
        IApiClientService apiClient,
        NavigationManager navigationManager,
        ILocalStorageService localStorage,
        ISnackbar snackbar,
        AuthenticationStateProvider authenticationStateProvider) // تزریق به صورت پایه
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
        _localStorage = localStorage; // می‌تواند null باشد اگر فقط Provider مدیریت می‌کند
        _snackbar = snackbar ?? throw new ArgumentNullException(nameof(snackbar));
        
        // کست کردن به نوع مشخص ما برای فراخوانی متدهای سفارشی
        if (authenticationStateProvider is CustomAuthenticationStateProvider provider)
        {
            _customAuthProvider = provider;
        }
        else
        {
            throw new InvalidOperationException("AuthenticationStateProvider is not of type CustomAuthenticationStateProvider");
        }
    }

    public async Task<bool> LoginAsync(LoginModel loginModel)
    {
        var requestDto = new LoginRequestDto
        {
            SystemUserId = loginModel.SystemUserId,
            Password = loginModel.Password
        };

        try
        {
            var response = await _apiClient.PostAsync<LoginRequestDto, LoginResponseDto>("/api/auth/login", requestDto);

            if (response != null && !string.IsNullOrEmpty(response.Token))
            {
                // ذخیره اطلاعات و توکن در LocalStorage و تنظیم هدر توسط MarkUserAsAuthenticatedAsync انجام خواهد شد
                await _customAuthProvider.MarkUserAsAuthenticatedAsync(response);

                _navigationManager.NavigateTo("/");
                _snackbar.Add("ورود با موفقیت انجام شد!", Severity.Success, config => { config.DuplicatesBehavior = SnackbarDuplicatesBehavior.Prevent;});
                return true;
            }
            else
            {
                // اگر PostAsync در صورت خطا null برگرداند
                _snackbar.Add("نام کاربری یا رمز عبور نامعتبر است (کد کلاینت).", Severity.Error, config => { config.DuplicatesBehavior = SnackbarDuplicatesBehavior.Prevent;});
                return false;
            }
        }
        catch (ApplicationException ex) // خطاهای مورد انتظار از API (مثلاً 400 با پیام از سمت سرور که ApiClientService آن را پرتاب می‌کند)
        {
            Console.WriteLine($"Login failed (ApplicationException): {ex.Message}");
            _snackbar.Add($"خطا در ورود: {ex.Message}", Severity.Error, config => { config.DuplicatesBehavior = SnackbarDuplicatesBehavior.Prevent;});
            return false;
        }
        catch (Exception ex) // خطاهای غیرمنتظره دیگر
        {
            Console.WriteLine($"Unexpected login error: {ex.Message} - StackTrace: {ex.StackTrace}");
            _snackbar.Add("خطای پیش‌بینی نشده در هنگام ورود رخ داد. لطفاً کنسول را بررسی کنید.", Severity.Error, config => { config.DuplicatesBehavior = SnackbarDuplicatesBehavior.Prevent;});
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        await _customAuthProvider.MarkUserAsLoggedOutAsync();
        _navigationManager.NavigateTo("/login"); // هدایت به صفحه لاگین پس از خروج
        _snackbar.Add("خروج با موفقیت انجام شد.", Severity.Info, config => { config.DuplicatesBehavior = SnackbarDuplicatesBehavior.Prevent;});
    }

    // این متدها بیشتر برای استفاده داخلی CustomAuthenticationStateProvider هستند
    // یا برای دسترسی مستقیم به توکن از سایر سرویس‌ها (اگرچه بهتر است از AuthenticationState استفاده شود)
    public async Task<string?> GetTokenAsync()
    {
        var authInfo = await _localStorage.GetItemAsync<LoginResponseDto?>(UserAuthInfoStorageKey);
        return authInfo?.Token;
    }

    public async Task<LoginResponseDto?> GetUserAuthInfoAsync()
    {
        try
        {
            return await _localStorage.GetItemAsync<LoginResponseDto?>(UserAuthInfoStorageKey);
        }
        catch (System.Text.Json.JsonException ex)
        {
            Console.WriteLine($"Error deserializing UserAuthInfo from LocalStorage: {ex.Message}");
            await _localStorage.RemoveItemAsync(UserAuthInfoStorageKey);
            return null;
        }
    }
}