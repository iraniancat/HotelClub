// src/HotelReservation.Application/Features/BookingRequests/Queries/GetBookingRequestDetails/GetBookingRequestDetailsQuery.cs
using HotelReservation.Application.DTOs.Booking;
using MediatR;
using System;

namespace HotelReservation.Application.Features.BookingRequests.Queries.GetBookingRequestDetails;

public class GetBookingRequestDetailsQuery : IRequest<BookingRequestDetailsDto?>
{
    public Guid BookingRequestId { get; }

    // اطلاعات کاربر احراز هویت شده برای بررسی مجوز دسترسی
    public Guid AuthenticatedUserId { get; }
    public string AuthenticatedUserRole { get; } // نام نقش، مثلا "SuperAdmin", "ProvinceUser", "HotelUser", "Employee"
    public string? AuthenticatedUserProvinceCode { get; } // برای ProvinceUser
    public Guid? AuthenticatedUserHotelId { get; } // برای HotelUser

    public GetBookingRequestDetailsQuery(
        Guid bookingRequestId, 
        Guid authenticatedUserId, 
        string authenticatedUserRole, 
        string? authenticatedUserProvinceCode, 
        Guid? authenticatedUserHotelId)
    {
        BookingRequestId = bookingRequestId;
        AuthenticatedUserId = authenticatedUserId;
        AuthenticatedUserRole = authenticatedUserRole;
        AuthenticatedUserProvinceCode = authenticatedUserProvinceCode;
        AuthenticatedUserHotelId = authenticatedUserHotelId;
    }
}