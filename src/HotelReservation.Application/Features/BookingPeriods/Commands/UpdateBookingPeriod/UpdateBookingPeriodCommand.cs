// UpdateBookingPeriodCommand.cs
using MediatR;

namespace HotelReservation.Application.Features.BookingPeriods.Commands.UpdateBookingPeriod;

public class UpdateBookingPeriodCommand : IRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
}
