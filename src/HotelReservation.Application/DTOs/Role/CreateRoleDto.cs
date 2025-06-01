// src/HotelReservation.Application/DTOs/Role/CreateRoleDto.cs
using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Application.DTOs.Role;

public class CreateRoleDto
{
    [Required(ErrorMessage = "نام نقش الزامی است.")]
    [MaxLength(50, ErrorMessage = "نام نقش نمی‌تواند بیشتر از ۵۰ کاراکتر باشد.")]
    public string Name { get; set; }

    [MaxLength(250, ErrorMessage = "توضیحات نقش نمی‌تواند بیشتر از ۲۵۰ کاراکتر باشد.")]
    public string? Description { get; set; }
}