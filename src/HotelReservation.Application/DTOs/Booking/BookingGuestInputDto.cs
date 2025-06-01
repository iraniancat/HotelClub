// src/HotelReservation.Application/DTOs/Booking/BookingGuestInputDto.cs
using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Application.DTOs.Booking;

public class BookingGuestInputDto
{
    [Required(ErrorMessage = "نام کامل مهمان الزامی است.")]
    [MaxLength(200)]
    public string FullName { get; set; }

    [Required(ErrorMessage = "کد ملی مهمان الزامی است.")]
    [MaxLength(10)]
    [RegularExpression("^[0-9]{10}$", ErrorMessage = "کد ملی باید ۱۰ رقمی و فقط شامل اعداد باشد.")]
    public string NationalCode { get; set; }

    [Required(ErrorMessage = "نسبت مهمان با کارمند اصلی الزامی است.")]
    [MaxLength(50)]
    public string RelationshipToEmployee { get; set; } // مثلا: "خود کارمند", "همسر", "فرزند", "همراه"
}