// src/HotelReservation.Application/DTOs/UserManagement/UpdateUserDto.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Application.DTOs.UserManagement;

public class UpdateUserDto
{
    [Required(ErrorMessage = "نام کامل الزامی است.")]
    [MaxLength(200, ErrorMessage = "نام کامل نمی‌تواند بیشتر از ۲۰۰ کاراکتر باشد.")]
    public string FullName { get; set; }

     [MaxLength(20, ErrorMessage = "شماره تلفن نمی‌تواند بیشتر از ۲۰ کاراکتر باشد.")]
    public string? PhoneNumber { get; set; } // <<-- اضافه شد

    [Required(ErrorMessage = "وضعیت فعالیت الزامی است.")]
    public bool IsActive { get; set; }

    [Required(ErrorMessage = "شناسه نقش الزامی است.")]
    public Guid RoleId { get; set; }

    // فیلدهای اختیاری
    [MaxLength(10, ErrorMessage = "کد ملی نمی‌تواند بیشتر از ۱۰ کاراکتر باشد.")]
    public string? NationalCode { get; set; }

    [MaxLength(10)]
    public string? ProvinceCode { get; set; }
    // ProvinceName معمولاً از ProvinceCode استنتاج می‌شود و در DTO ورودی لازم نیست

    [MaxLength(20)]
    public string? DepartmentCode { get; set; }
    // DepartmentName معمولاً از DepartmentCode استنتاج می‌شود

    public Guid? HotelId { get; set; } // برای نقش کاربر هتل
}