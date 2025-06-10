// src/HotelReservation.Application/Features/BookingRequests/Commands/ApproveBookingRequest/ApproveBookingRequestCommand.cs
using MediatR;
using System;

namespace HotelReservation.Application.Features.BookingRequests.Commands.ApproveBookingRequest;

public class ApproveBookingRequestCommand : IRequest // یا IRequest<Unit>
{
     public Guid BookingRequestId { get; }
    public string? Comments { get; }

    public ApproveBookingRequestCommand(Guid bookingRequestId, string? comments)
    {
        BookingRequestId = bookingRequestId;
        Comments = comments;
    }
}