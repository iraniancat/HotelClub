// src/HotelReservation.Application/Features/BookingRequests/Commands/UploadBookingFile/UploadBookingFileCommand.cs
using HotelReservation.Application.DTOs.Booking; // برای BookingFileDto
using MediatR;
using Microsoft.AspNetCore.Http; // برای IFormFile
using System;
using System.IO; // برای Stream

namespace HotelReservation.Application.Features.BookingRequests.Commands.UploadBookingFile;

// این Command می‌تواند BookingFileDto یا فقط شناسه BookingFile را برگرداند
public class UploadBookingFileCommand : IRequest<BookingFileDto> 
{
    public Guid BookingRequestId { get; set; } // شناسه درخواست رزروی که فایل به آن پیوست می‌شود
    
    // به جای پاس دادن مستقیم IFormFile به Handler، اطلاعات لازم را از آن استخراج می‌کنیم.
    // این کار وابستگی لایه Application به Microsoft.AspNetCore.Http را کمتر می‌کند (اگرچه IFormFile خود یک abstraction است).
    // یا می‌توانیم IFormFile را مستقیماً پاس دهیم اگر سادگی مد نظر باشد.
    // گزینه دیگر: پاس دادن Stream و نام فایل و ContentType به صورت جداگانه.
    public Stream FileStream { get; set; }
    public string FileName { get; set; }
    public string ContentType { get; set; }
    
    public Guid UploadedByUserId { get; set; } // شناسه کاربری که فایل را آپلود می‌کند (برای لاگ و مجوزدهی احتمالی)

    public UploadBookingFileCommand(
        Guid bookingRequestId, 
        Stream fileStream, 
        string fileName, 
        string contentType, 
        Guid uploadedByUserId)
    {
        BookingRequestId = bookingRequestId;
        FileStream = fileStream;
        FileName = fileName;
        ContentType = contentType;
        UploadedByUserId = uploadedByUserId;
    }
}