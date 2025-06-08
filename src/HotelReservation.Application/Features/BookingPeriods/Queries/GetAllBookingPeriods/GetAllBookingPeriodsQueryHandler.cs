// GetAllBookingPeriodsQueryHandler.cs
using HotelReservation.Application.DTOs.Booking;
using MediatR;

namespace HotelReservation.Application.Features.BookingPeriods.Queries.GetAllBookingPeriods;

// GetAllBookingPeriodsQueryHandler.cs
public class GetAllBookingPeriodsQueryHandler : IRequestHandler<GetAllBookingPeriodsQuery, IEnumerable<BookingPeriodDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetAllBookingPeriodsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<IEnumerable<BookingPeriodDto>> Handle(GetAllBookingPeriodsQuery request, CancellationToken cancellationToken)
    {
        var periods = await _unitOfWork.BookingPeriodRepository.GetAllAsync();
        return periods.Select(p => new BookingPeriodDto
        {
            Id = p.Id,
            Name = p.Name,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            IsActive = p.IsActive
        }).OrderByDescending(p => p.StartDate).ToList();
    }
}