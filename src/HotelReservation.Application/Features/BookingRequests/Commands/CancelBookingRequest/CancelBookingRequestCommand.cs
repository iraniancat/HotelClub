using MediatR;

namespace HotelReservation.Application.Features.BookingRequests.Commands.CancelBookingRequest;

public class CancelBookingRequestCommand : IRequest
{
    public Guid BookingRequestId { get; }
    public string? CancellationReason { get; } // اختیاری

    public CancelBookingRequestCommand(Guid bookingRequestId, string? cancellationReason)
    {
        BookingRequestId = bookingRequestId;
        CancellationReason = cancellationReason;
    }

}