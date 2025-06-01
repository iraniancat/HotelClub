// src/HotelReservation.Application/DTOs/Role/RoleDto.cs
using System;

namespace HotelReservation.Application.DTOs.Role;

public class RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    // می‌توان تعداد کاربران با این نقش را هم در آینده اضافه کرد (UserCount)
}