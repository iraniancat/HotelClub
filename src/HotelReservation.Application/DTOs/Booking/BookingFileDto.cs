// src/HotelReservation.Application/DTOs/Booking/BookingFileDto.cs
using System;

namespace HotelReservation.Application.DTOs.Booking;

public class BookingFileDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public DateTime UploadedDate { get; set; }
    public string DownloadUrl { get; set; } // URL برای دانلود فایل (بعداً ایجاد می‌شود)
    // public long FileSize { get; set; } // اندازه فایل (اختیاری)
}