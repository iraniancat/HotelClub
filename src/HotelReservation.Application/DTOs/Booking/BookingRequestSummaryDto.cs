// src/HotelReservation.Application/DTOs/Booking/BookingRequestSummaryDto.cs
using System;

namespace HotelReservation.Application.DTOs.Booking;

public class BookingRequestSummaryDto
{
    public Guid Id { get; set; }
    public string TrackingCode { get; set; }
    public string RequestingEmployeeNationalCode { get; set; } // کد ملی کارمند اصلی
    public string? RequestingEmployeeFullName { get; set; } // نام کامل کارمند اصلی (از جدول User خوانده می‌شود)
    public string HotelName { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public string Status { get; set; } // وضعیت به صورت رشته
    public DateTime SubmissionDate { get; set; }
    public string SubmitterUserFullName { get; set; } // نام کامل کاربر ثبت کننده
    public int TotalGuests { get; set; }
}