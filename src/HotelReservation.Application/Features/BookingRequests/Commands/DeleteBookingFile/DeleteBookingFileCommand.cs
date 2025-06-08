// src/HotelReservation.Application/Features/BookingRequests/Commands/DeleteBookingFile/DeleteBookingFileCommand.cs
using MediatR;
using System;

namespace HotelReservation.Application.Features.BookingRequests.Commands.DeleteBookingFile;

public class DeleteBookingFileCommand : IRequest // یا IRequest<Unit>
{
    public Guid BookingRequestId { get; } // شناسه درخواست رزروی که فایل به آن تعلق دارد
    public Guid FileId { get; }           // شناسه فایلی که باید حذف شود
    
    // اطلاعات کاربر احراز هویت شده از ICurrentUserService در Handler خوانده می‌شود
    // public Guid DeletingUserId { get; } 

    public DeleteBookingFileCommand(Guid bookingRequestId, Guid fileId)
    {
        BookingRequestId = bookingRequestId;
        FileId = fileId;
    }
}