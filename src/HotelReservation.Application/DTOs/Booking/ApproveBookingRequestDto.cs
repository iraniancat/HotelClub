// src/HotelReservation.Application/DTOs/Booking/ApproveBookingRequestDto.cs
using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Application.DTOs.Booking;

public class ApproveBookingRequestDto
{
    [MaxLength(500, ErrorMessage = "نظرات تأیید نمی‌تواند بیشتر از ۵۰۰ کاراکتر باشد.")]
    public string? Comments { get; set; } // نظرات اختیاری کاربر هتل هنگام تأیید
}