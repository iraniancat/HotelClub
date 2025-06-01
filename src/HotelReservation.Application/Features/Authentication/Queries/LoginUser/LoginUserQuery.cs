// src/HotelReservation.Application/Features/Authentication/Queries/LoginUser/LoginUserQuery.cs
using HotelReservation.Application.DTOs.Authentication; // برای LoginResponseDto
using MediatR;

namespace HotelReservation.Application.Features.Authentication.Queries.LoginUser;

public class LoginUserQuery : IRequest<LoginResponseDto> // در صورت موفقیت، LoginResponseDto را باز می‌گرداند
{
    public string SystemUserId { get; }
    public string Password { get; }

    public LoginUserQuery(string systemUserId, string password)
    {
        SystemUserId = systemUserId;
        Password = password;
    }
}