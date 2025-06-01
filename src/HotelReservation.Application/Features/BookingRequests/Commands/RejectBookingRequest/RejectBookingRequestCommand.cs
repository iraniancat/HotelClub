// src/HotelReservation.Application/Features/BookingRequests/Commands/RejectBookingRequest/RejectBookingRequestCommand.cs
using MediatR;
using System;

namespace HotelReservation.Application.Features.BookingRequests.Commands.RejectBookingRequest;

public class RejectBookingRequestCommand : IRequest // یا IRequest<Unit>
{
    public Guid BookingRequestId { get; } // از مسیر URL
    public Guid HotelUserId { get; }      // از کاربر احراز هویت شده هتل
    public string RejectionReason { get; } // از DTO

    public RejectBookingRequestCommand(Guid bookingRequestId, Guid hotelUserId, string rejectionReason)
    {
        BookingRequestId = bookingRequestId;
        HotelUserId = hotelUserId;
        RejectionReason = rejectionReason;
    }
}