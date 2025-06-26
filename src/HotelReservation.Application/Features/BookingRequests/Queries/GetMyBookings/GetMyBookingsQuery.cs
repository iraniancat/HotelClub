using HotelReservation.Application.DTOs.Booking;
using HotelReservation.Application.DTOs.Common;
using MediatR;

namespace HotelReservation.Application.Features.BookingRequests.Queries.GetMyBookings;

public class GetMyBookingsQuery : IRequest<PagedResult<BookingRequestSummaryDto>>
{
    public string? StatusFilter { get; set; }
    public string? SearchTerm { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
