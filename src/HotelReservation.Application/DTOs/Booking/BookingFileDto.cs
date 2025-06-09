// src/HotelReservation.Application/DTOs/Booking/BookingFileDto.cs
using System;

namespace HotelReservation.Application.DTOs.Booking;

public class BookingFileDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public DateTime UploadedDate { get; set; } // <<-- اضافه شد
    public string? DownloadUrl { get; set; }    // <<-- اضافه شد
}