// src/HotelReservation.Application/Features/BookingRequests/Queries/GetAllBookingRequests/GetAllBookingRequestsQuery.cs
// ...
using HotelReservation.Application.DTOs.Booking;
using HotelReservation.Application.DTOs.Common;
using MediatR;

public class GetAllBookingRequestsQuery : IRequest<PagedResult<BookingRequestSummaryDto>>
{
    // <<-- حذف پارامترهای AuthenticatedUserId, AuthenticatedUserRole, AuthenticatedUserProvinceCode, AuthenticatedUserHotelId -->>
    // این اطلاعات از ICurrentUserService در Handler خوانده خواهند شد.

    public string? StatusFilter { get; set; }
    public string? SearchTerm { get; set; }
    // ... (پارامترهای صفحه‌بندی همانند قبل) ...
     private const int MaxPageSize = 50;
     private int _pageNumber = 1;
     public int PageNumber //...
     {
        get => _pageNumber;
        set => _pageNumber = (value <= 0) ? 1 : value;
     }
     private int _pageSize = 10;
     public int PageSize //...
     {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : (value <= 0 ? 10 : value);
     }

    // سازنده می‌تواند خالی باشد یا فقط پارامترهای فیلتر را بگیرد
    public GetAllBookingRequestsQuery() { }
}