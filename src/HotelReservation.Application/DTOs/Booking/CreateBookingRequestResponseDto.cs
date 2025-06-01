// src/HotelReservation.Application/DTOs/Booking/CreateBookingRequestResponseDto.cs
using System;

namespace HotelReservation.Application.DTOs.Booking; // <<<--- دقت کنید namespace صحیح باشد

public class CreateBookingRequestResponseDto // <<<--- دقت کنید نام کلاس دقیقاً همین باشد
{
    public Guid Id { get; set; }
    public string TrackingCode { get; set; }
    public string Message { get; set; } = "درخواست رزرو با موفقیت ثبت شد.";
}