// GetActiveBookingPeriodsQueryHandler.cs
using HotelReservation.Application.DTOs.Booking;
using MediatR;

namespace HotelReservation.Application.Features.BookingPeriods.Queries.GetActiveBookingPeriods;

// GetActiveBookingPeriodsQueryHandler.cs
public class GetActiveBookingPeriodsQueryHandler : IRequestHandler<GetActiveBookingPeriodsQuery, IEnumerable<BookingPeriodDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetActiveBookingPeriodsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<IEnumerable<BookingPeriodDto>> Handle(GetActiveBookingPeriodsQuery request, CancellationToken cancellationToken)
    {
        var periods = await _unitOfWork.BookingPeriodRepository.GetAsync(p => p.IsActive && p.EndDate >= DateTime.UtcNow);
        return periods.Select(p => new BookingPeriodDto
        {
            Id = p.Id,
            Name = p.Name,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            IsActive = p.IsActive
        }).OrderBy(p => p.StartDate).ToList();
    }
}