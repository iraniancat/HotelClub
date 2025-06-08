// src/HotelReservation.Application/DTOs/Booking/CancelBookingRequestDto.cs
using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Application.DTOs.Booking;

public class CancelBookingRequestDto
{
    [MaxLength(500, ErrorMessage = "دلیل لغو نمی‌تواند بیشتر از ۵۰۰ کاراکتر باشد.")]
    public string? CancellationReason { get; set; }
}