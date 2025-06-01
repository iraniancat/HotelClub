// src/HotelReservation.Application/Contracts/Infrastructure/IJwtTokenGenerator.cs
using HotelReservation.Application.DTOs.Authentication; // برای LoginResponseDto
using HotelReservation.Domain.Entities; // برای User

namespace HotelReservation.Application.Contracts.Infrastructure;

public interface IJwtTokenGenerator
{
    // این متد اطلاعات کاربر را گرفته و LoginResponseDto شامل توکن و سایر جزئیات را برمی‌گرداند
    LoginResponseDto GenerateToken(User user); 
}