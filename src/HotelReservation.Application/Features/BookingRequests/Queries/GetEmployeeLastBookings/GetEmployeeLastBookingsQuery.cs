using HotelReservation.Application.DTOs.Booking;
using MediatR;

namespace HotelReservation.Application.Features.BookingRequests.Queries.GetEmployeeLastBookings;

public class GetEmployeeLastBookingsQuery : IRequest<IEnumerable<BookingHistoryDto>>
{
    public string EmployeeNationalCode { get; }
    public Guid HotelId { get; }

    public GetEmployeeLastBookingsQuery(string employeeNationalCode, Guid hotelId)
    {
        EmployeeNationalCode = employeeNationalCode;
        HotelId = hotelId;
    }
}
