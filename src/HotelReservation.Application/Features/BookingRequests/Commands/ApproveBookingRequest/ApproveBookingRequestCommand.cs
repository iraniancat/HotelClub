// src/HotelReservation.Application/Features/BookingRequests/Commands/ApproveBookingRequest/ApproveBookingRequestCommand.cs
using MediatR;
using System;

namespace HotelReservation.Application.Features.BookingRequests.Commands.ApproveBookingRequest;

public class ApproveBookingRequestCommand : IRequest // یا IRequest<Unit>
{
    public Guid BookingRequestId { get; } // از مسیر URL
    public Guid HotelUserId { get; }      // از کاربر احراز هویت شده هتل
    public string? Comments { get; }      // از DTO (اختیاری)

    public ApproveBookingRequestCommand(Guid bookingRequestId, Guid hotelUserId, string? comments)
    {
        BookingRequestId = bookingRequestId;
        HotelUserId = hotelUserId;
        Comments = comments;
    }
}