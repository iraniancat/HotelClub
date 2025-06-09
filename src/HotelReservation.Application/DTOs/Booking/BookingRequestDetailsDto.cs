// src/HotelReservation.Application/DTOs/Booking/BookingRequestDetailsDto.cs
using HotelReservation.Application.DTOs.UserManagement; // برای UserSlimDto یا مشابه
using HotelReservation.Application.DTOs.Hotel; // برای HotelSlimDto یا مشابه
using System;
using System.Collections.Generic;

namespace HotelReservation.Application.DTOs.Booking;

public class BookingRequestDetailsDto
{
    public Guid Id { get; set; }
    public string TrackingCode { get; set; }
    public string RequestingEmployeeNationalCode { get; set; }
    public string? RequestingEmployeeFullName { get; set; } // <<-- اضافه شد
    public string BookingPeriod { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfNights { get; set; }
    public int TotalGuests { get; set; }
    public string Status { get; set; }
    public DateTime SubmissionDate { get; set; }
    public DateTime LastStatusUpdateDate { get; set; }
    public string? Notes { get; set; }

    public HotelSlimDto? Hotel { get; set; }
    public UserManagementListDto? RequestSubmitterUser { get; set; }

    public List<BookingGuestDetailsDto> Guests { get; set; } = new();
    public List<BookingFileDto> Files { get; set; } = new(); // <<-- اضافه شد
}
