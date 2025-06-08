using MediatR;

namespace HotelReservation.Application.Features.BookingPeriods.Commands.CreateBookingPeriod;

public class CreateBookingPeriodCommand : IRequest<Guid>
{
    public string Name { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
}
