// مسیر: src/HotelReservation.Application/Features/BookingRequests/Commands/InitialReject/
using MediatR;

namespace HotelReservation.Application.Features.BookingRequests.Commands.InitialReject;

public class InitialRejectCommand : IRequest
{
    public Guid BookingRequestId { get; set; }
    public string RejectionReason { get; set; }

    public InitialRejectCommand(Guid bookingRequestId, string rejectionReason)
    {
        BookingRequestId = bookingRequestId;
        RejectionReason = rejectionReason;
    }
}
