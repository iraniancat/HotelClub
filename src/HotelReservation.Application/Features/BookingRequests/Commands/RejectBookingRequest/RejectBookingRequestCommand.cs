// src/HotelReservation.Application/Features/BookingRequests/Commands/RejectBookingRequest/RejectBookingRequestCommand.cs
using MediatR;
using System;

namespace HotelReservation.Application.Features.BookingRequests.Commands.RejectBookingRequest;

public class RejectBookingRequestCommand : IRequest
{
    public Guid BookingRequestId { get; }
    public string RejectionReason { get; }

    public RejectBookingRequestCommand(Guid bookingRequestId, string rejectionReason)
    {
        BookingRequestId = bookingRequestId;
        RejectionReason = rejectionReason;
    }
}