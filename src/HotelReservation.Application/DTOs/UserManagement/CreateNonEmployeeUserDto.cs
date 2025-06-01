// src/HotelReservation.Application/DTOs/UserManagement/CreateNonEmployeeUserDto.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Application.DTOs.UserManagement;

public class CreateNonEmployeeUserDto
{
    [Required(ErrorMessage = "شناسه کاربری سیستم (نام کاربری) الزامی است.")]
    [MaxLength(100, ErrorMessage = "شناسه کاربری سیستم نمی‌تواند بیشتر از ۱۰۰ کاراکتر باشد.")]
    public string SystemUserId { get; set; }

    [Required(ErrorMessage = "نام کامل الزامی است.")]
    [MaxLength(200, ErrorMessage = "نام کامل نمی‌تواند بیشتر از ۲۰۰ کاراکتر باشد.")]
    public string FullName { get; set; }

    [Required(ErrorMessage = "رمز عبور الزامی است.")]
    [MinLength(6, ErrorMessage = "رمز عبور باید حداقل ۶ کاراکتر باشد.")] // نمونه‌ای از قانون برای رمز
    public string Password { get; set; }

    [Required(ErrorMessage = "شناسه نقش الزامی است.")]
    public Guid RoleId { get; set; }

    public bool IsActive { get; set; } = true; // پیش‌فرض فعال

    [MaxLength(10, ErrorMessage = "کد ملی نمی‌تواند بیشتر از ۱۰ کاراکتر باشد.")]
    public string? NationalCode { get; set; } // اختیاری
   
    [MaxLength(20, ErrorMessage = "شماره تلفن نمی‌تواند بیشتر از ۲۰ کاراکتر باشد.")]
    [RegularExpression(@"^[0-9\+\-\(\)\s]*$", ErrorMessage = "فرمت شماره تلفن معتبر نیست.")]
    public string? PhoneNumber { get; set; }
    
    [MaxLength(10)]
    public string? ProvinceCode { get; set; } // اختیاری

    [MaxLength(100)]
    public string? ProvinceName { get; set; } // اختیاری، برای راحتی، اگرچه از کد استان می‌توان دریافت کرد

    [MaxLength(20)]
    public string? DepartmentCode { get; set; } // اختیاری

    [MaxLength(150)]
    public string? DepartmentName { get; set; } // اختیاری

    public Guid? HotelId { get; set; } // اختیاری، اما برای نقش "کاربر هتل" ضروری خواهد بود
}