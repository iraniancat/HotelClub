// GetActiveBookingPeriodsQuery.cs
using HotelReservation.Application.DTOs.Booking;
using MediatR;

namespace HotelReservation.Application.Features.BookingPeriods.Queries.GetActiveBookingPeriods;

public class GetActiveBookingPeriodsQuery : IRequest<IEnumerable<BookingPeriodDto>> { }
