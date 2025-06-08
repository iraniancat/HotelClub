// src/HotelReservation.Application/DTOs/UserManagement/AssignRoleToUserDto.cs
using System;
using System.ComponentModel.DataAnnotations; // برای Required

namespace HotelReservation.Application.DTOs.UserManagement;

public class AssignRoleToUserDto
{
    [Required(ErrorMessage = "شناسه نقش الزامی است.")]
    public Guid RoleId { get; set; }

    public Guid? HotelId { get; set; }

    public string? ProvinceCode { get; set; } // <<-- اضافه شد
}