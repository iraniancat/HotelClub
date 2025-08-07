using HotelReservation.Application.DTOs.Booking;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Application.Features.BookingRequests.Queries.GetEmployeeLastBookings;

public class GetEmployeeLastBookingsQueryHandler : IRequestHandler<GetEmployeeLastBookingsQuery, IEnumerable<BookingHistoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetEmployeeLastBookingsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<BookingHistoryDto>> Handle(GetEmployeeLastBookingsQuery request, CancellationToken cancellationToken)
    {
        var bookings = await _unitOfWork.BookingRequestRepository.GetQueryable()
            .Where(b => b.RequestingEmployeeNationalCode == request.EmployeeNationalCode && b.HotelId == request.HotelId)
            .OrderByDescending(b => b.SubmissionDate)
            .Take(10)
            .Select(b => new BookingHistoryDto
            {
                TrackingCode = b.TrackingCode,
                CheckInDate = b.CheckInDate,
                CheckOutDate = b.CheckOutDate,
                Status = b.Status.ToString()
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);
            
        return bookings;
    }
}
