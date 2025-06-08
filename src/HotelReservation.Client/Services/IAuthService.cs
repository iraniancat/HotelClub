// src/HotelReservation.Client/Services/IAuthService.cs
using System.ComponentModel.DataAnnotations;
using HotelReservation.Application.DTOs.Authentication; // برای LoginRequestDto و LoginResponseDto از پروژه Application
                                                         // یا تعریف مدل‌های معادل در Client.Models

namespace HotelReservation.Client.Services.Authentication; // یا HotelReservation.Client.Services

// مدل برای فرم ورود در UI
public class LoginModel
{
    [Required(ErrorMessage = "شناسه کاربری الزامی است.")]
    public string SystemUserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "رمز عبور الزامی است.")]
    public string Password { get; set; } = string.Empty;
}

public interface IAuthService
{
    // رویداد برای اطلاع‌رسانی تغییر وضعیت احراز هویت
    event Action? AuthenticationStateChanged; 

    Task<bool> LoginAsync(LoginModel loginModel);
    Task LogoutAsync();
    Task<string?> GetTokenAsync(); // برای بازیابی توکن از LocalStorage
    Task<LoginResponseDto?> GetUserAuthInfoAsync(); // برای بازیابی اطلاعات کاربر از LocalStorage
   // void NotifyUserAuthentication(LoginResponseDto authInfo); // فراخوانی پس از ورود موفق
   // void NotifyUserLogout(); // فراخوانی پس از خروج
}