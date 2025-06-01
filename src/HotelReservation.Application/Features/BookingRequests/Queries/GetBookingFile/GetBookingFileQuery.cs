// src/HotelReservation.Application/Features/BookingRequests/Queries/GetBookingFile/GetBookingFileQuery.cs
using HotelReservation.Application.DTOs.Booking; // برای BookingFileDownloadDto
using MediatR;
using System;

namespace HotelReservation.Application.Features.BookingRequests.Queries.GetBookingFile;

public class GetBookingFileQuery : IRequest<BookingFileDownloadDto?> // می‌تواند null باشد اگر فایل یافت نشود یا مجوز نباشد
{
    public Guid BookingRequestId { get; }
    public Guid FileId { get; }
    
    // اطلاعات کاربر احراز هویت شده برای بررسی مجوز در Handler
    // این اطلاعات از ICurrentUserService در Handler خوانده می‌شود، پس نیازی به پاس دادن صریح آن‌ها نیست.
    // اما اگر ICurrentUserService را تزریق نمی‌کردیم، باید اینجا می‌آمدند.
    // public Guid AuthenticatedUserId { get; } 
    // public string AuthenticatedUserRole { get; }
    // public string? AuthenticatedUserProvinceCode { get; }
    // public Guid? AuthenticatedUserHotelId { get; }


    public GetBookingFileQuery(Guid bookingRequestId, Guid fileId)
    {
        BookingRequestId = bookingRequestId;
        FileId = fileId;
    }
}