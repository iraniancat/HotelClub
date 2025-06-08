// GetAllBookingPeriodsQuery.cs
using HotelReservation.Application.DTOs.Booking;
using MediatR;

namespace HotelReservation.Application.Features.BookingPeriods.Queries.GetAllBookingPeriods;

public class GetAllBookingPeriodsQuery : IRequest<IEnumerable<BookingPeriodDto>> { }
