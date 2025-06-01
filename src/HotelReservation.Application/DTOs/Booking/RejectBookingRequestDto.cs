// src/HotelReservation.Application/DTOs/Booking/RejectBookingRequestDto.cs
using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Application.DTOs.Booking;

public class RejectBookingRequestDto
{
    [Required(ErrorMessage = "دلیل رد درخواست الزامی است.")]
    [MaxLength(500, ErrorMessage = "دلیل رد درخواست نمی‌تواند بیشتر از ۵۰۰ کاراکتر باشد.")]
    public string RejectionReason { get; set; }
}