// src/HotelReservation.Application/DTOs/Booking/BookingRequestDetailsDto.cs
using HotelReservation.Application.DTOs.UserManagement; // برای UserSlimDto یا مشابه
using HotelReservation.Application.DTOs.Hotel; // برای HotelSlimDto یا مشابه
using System;
using System.Collections.Generic;

namespace HotelReservation.Application.DTOs.Booking;

public class BookingGuestDetailsDto // DTO برای نمایش مهمان در پاسخ
{
    public Guid Id { get; set; }
    public string FullName { get; set; }
    public string NationalCode { get; set; }
    public string RelationshipToEmployee { get; set; }
    public decimal DiscountPercentage { get; set; }
}

public class BookingRequestDetailsDto
{
    public Guid Id { get; set; }
    public string TrackingCode { get; set; }
    public string RequestingEmployeeNationalCode { get; set; }
    public string BookingPeriod { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfNights { get; set; }
    public int TotalGuests { get; set; }
    public string Status { get; set; } // نام وضعیت به صورت رشته
    public DateTime SubmissionDate { get; set; }
    public DateTime LastStatusUpdateDate { get; set; }
    public string? Notes { get; set; }

    public HotelSlimDto Hotel { get; set; } // اطلاعات خلاصه هتل
    public UserManagementListDto RequestSubmitterUser { get; set; } // اطلاعات خلاصه کاربر ثبت کننده
                                                                  // (یا یک UserSlimDto عمومی‌تر)

    public List<BookingGuestDetailsDto> Guests { get; set; } = new List<BookingGuestDetailsDto>();
    // public List<BookingFileDto> Files { get; set; } = new List<BookingFileDto>(); // در آینده برای فایل‌ها
}