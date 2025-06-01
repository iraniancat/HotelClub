// src/HotelReservation.Application/DTOs/Hotel/CreateHotelDto.cs
using System.ComponentModel.DataAnnotations; // برای Data Annotations (اختیاری، می‌توان از FluentValidation استفاده کرد)

namespace HotelReservation.Application.DTOs.Hotel;

public class CreateHotelDto
{
    [Required(ErrorMessage = "نام هتل الزامی است.")]
    [MaxLength(150, ErrorMessage = "نام هتل نمی‌تواند بیشتر از ۱۵۰ کاراکتر باشد.")]
    public string Name { get; set; }

    [Required(ErrorMessage = "آدرس هتل الزامی است.")]
    [MaxLength(500, ErrorMessage = "آدرس هتل نمی‌تواند بیشتر از ۵۰۰ کاراکتر باشد.")]
    public string Address { get; set; }

    [MaxLength(100, ErrorMessage = "نام فرد رابط نمی‌تواند بیشتر از ۱۰۰ کاراکتر باشد.")]
    public string? ContactPerson { get; set; } // اختیاری

    [MaxLength(20, ErrorMessage = "شماره تلفن نمی‌تواند بیشتر از ۲۰ کاراکتر باشد.")]
    public string? PhoneNumber { get; set; } // اختیاری
}