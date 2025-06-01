// src/HotelReservation.Application/DTOs/Authentication/LoginRequestDto.cs
using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Application.DTOs.Authentication;

public class LoginRequestDto
{
    [Required(ErrorMessage = "شناسه کاربری سیستم (نام کاربری) الزامی است.")]
    public string SystemUserId { get; set; }

    [Required(ErrorMessage = "رمز عبور الزامی است.")]
    public string Password { get; set; }
}