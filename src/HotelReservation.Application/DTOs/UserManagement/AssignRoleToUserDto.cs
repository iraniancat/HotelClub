// src/HotelReservation.Application/DTOs/UserManagement/AssignRoleToUserDto.cs
using System;
using System.ComponentModel.DataAnnotations; // برای Required

namespace HotelReservation.Application.DTOs.UserManagement;

public class AssignRoleToUserDto
{
    [Required(ErrorMessage = "شناسه نقش الزامی است.")]
    public Guid RoleId { get; set; }

    // شناسه هتل فقط زمانی لازم است که نقش تخصیص داده شده "کاربر هتل" باشد.
    // اعتبارسنجی این مورد در Command Handler یا Validator پیچیده‌تر انجام خواهد شد.
    public Guid? HotelId { get; set; } 
}