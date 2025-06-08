// DeleteBookingPeriodCommand.cs
using MediatR;

namespace HotelReservation.Application.Features.BookingPeriods.Commands.DeleteBookingPeriod;

public class DeleteBookingPeriodCommand : IRequest
{
    public Guid Id { get; set; }
}
