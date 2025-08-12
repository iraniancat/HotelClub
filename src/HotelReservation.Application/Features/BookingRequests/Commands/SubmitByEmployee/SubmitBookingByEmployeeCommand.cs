using HotelReservation.Application.DTOs.Booking;
using MediatR;

namespace HotelReservation.Application.Features.BookingRequests.Commands.SubmitByEmployee;

// SubmitBookingByEmployeeCommand.cs
public class SubmitBookingByEmployeeCommand : IRequest<CreateBookingRequestResponseDto>
{
    public string RequestingEmployeeNationalCode { get; set; }
    public Guid BookingPeriodId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public Guid HotelId { get; set; }
    public string? Notes { get; set; }
    public List<BookingGuestInputDto> Guests { get; set; } = new();
    //    public SubmitBookingByEmployeeCommand(
    //     string requestingEmployeeNationalCode,
    //     Guid bookingPeriodId,
    //     DateTime checkInDate,
    //     DateTime checkOutDate,
    //     Guid hotelId,
    //     List<BookingGuestInputDto> guests,
    //     string? notes)
    // {
    //     RequestingEmployeeNationalCode = requestingEmployeeNationalCode;
    //     BookingPeriodId = bookingPeriodId;
    //     CheckInDate = checkInDate;
    //     CheckOutDate = checkOutDate;
    //     HotelId = hotelId;
    //     Guests = guests ?? new List<BookingGuestInputDto>();
    //     Notes = notes;
    // }
}
