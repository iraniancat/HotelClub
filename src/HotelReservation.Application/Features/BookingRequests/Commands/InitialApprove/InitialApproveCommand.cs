using MediatR;

namespace HotelReservation.Application.Features.BookingRequests.Commands.InitialApprove;

// InitialApproveCommand.cs
public class InitialApproveCommand : IRequest { public Guid BookingRequestId { get; set; } }
