// src/HotelReservation.Application/DTOs/UserManagement/SetUserPasswordDto.cs
using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Application.DTOs.UserManagement;

public class SetUserPasswordDto
{
    [Required(ErrorMessage = "رمز عبور جدید الزامی است.")]
    [MinLength(6, ErrorMessage = "رمز عبور جدید باید حداقل ۶ کاراکتر باشد.")]
    public string NewPassword { get; set; }

    [Required(ErrorMessage = "تکرار رمز عبور جدید الزامی است.")]
    [Compare(nameof(NewPassword), ErrorMessage = "رمز عبور جدید و تکرار آن باید یکسان باشند.")]
    public string ConfirmPassword { get; set; }
}