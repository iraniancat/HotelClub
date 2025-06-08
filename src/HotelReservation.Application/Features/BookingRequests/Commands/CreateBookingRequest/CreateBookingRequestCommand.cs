// src/HotelReservation.Application/Features/BookingRequests/Commands/CreateBookingRequest/CreateBookingRequestCommand.cs


using HotelReservation.Application.DTOs.Booking;
using MediatR;
using System;
using System.Collections.Generic;

namespace HotelReservation.Application.Features.BookingRequests.Commands.CreateBookingRequest;

public class CreateBookingRequestCommand : IRequest<CreateBookingRequestResponseDto>
{
    public string RequestingEmployeeNationalCode { get; set; }
    public Guid BookingPeriodId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public Guid HotelId { get; set; }
    public string? Notes { get; set; }
    public List<BookingGuestInputDto> Guests { get; set; } = new List<BookingGuestInputDto>();

    public CreateBookingRequestCommand(
        string requestingEmployeeNationalCode,
        Guid bookingPeriodId,
        DateTime checkInDate,
        DateTime checkOutDate,
        Guid hotelId,
        List<BookingGuestInputDto> guests,
        string? notes)
    {
        RequestingEmployeeNationalCode = requestingEmployeeNationalCode;
        BookingPeriodId = bookingPeriodId;
        CheckInDate = checkInDate;
        CheckOutDate = checkOutDate;
        HotelId = hotelId;
        Guests = guests ?? new List<BookingGuestInputDto>();
        Notes = notes;
    }
}