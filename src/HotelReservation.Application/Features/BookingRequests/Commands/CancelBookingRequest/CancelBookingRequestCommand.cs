// src/HotelReservation.Application/Features/BookingRequests/Commands/CancelBookingRequest/CancelBookingRequestCommand.cs
using MediatR;
using System;

namespace HotelReservation.Application.Features.BookingRequests.Commands.CancelBookingRequest;

public class CancelBookingRequestCommand : IRequest // یا IRequest<Unit>
{
    public Guid BookingRequestId { get; }
    public string? CancellationReason { get; } // اختیاری

    // شناسه کاربری که این درخواست را لغو می‌کند از ICurrentUserService خوانده خواهد شد.

    public CancelBookingRequestCommand(Guid bookingRequestId, string? cancellationReason)
    {
        BookingRequestId = bookingRequestId;
        CancellationReason = cancellationReason;
    }
}