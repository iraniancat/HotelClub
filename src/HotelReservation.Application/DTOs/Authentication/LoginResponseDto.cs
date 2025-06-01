// src/HotelReservation.Application/DTOs/Authentication/LoginResponseDto.cs
using System;

namespace HotelReservation.Application.DTOs.Authentication;

public class LoginResponseDto
{
    public string Token { get; set; }
    public DateTime Expiration { get; set; } // تاریخ انقضای توکن
    public Guid UserId { get; set; } // شناسه داخلی کاربر در سیستم
    public string SystemUserId { get; set; } // نام کاربری
    public string FullName { get; set; }
    public string Role { get; set; } // نام نقش کاربر
    // می‌توان اطلاعات دیگری مانند ProvinceCode یا HotelId را هم در صورت نیاز اینجا اضافه کرد
}