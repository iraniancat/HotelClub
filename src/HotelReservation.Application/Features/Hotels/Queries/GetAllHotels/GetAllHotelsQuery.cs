// src/HotelReservation.Application/Features/Hotels/Queries/GetAllHotels/GetAllHotelsQuery.cs
using HotelReservation.Application.DTOs.Common; // <<-- برای PagedResult
using HotelReservation.Application.DTOs.Hotel;  // برای HotelDto
using MediatR;

namespace HotelReservation.Application.Features.Hotels.Queries.GetAllHotels;

public class GetAllHotelsQuery : IRequest<PagedResult<HotelDto>> // <<-- نوع بازگشتی تغییر کرد
{
    public string? SearchTerm { get; set; } // برای جستجو در نام، آدرس و ...

    // پارامترهای صفحه‌بندی
    private const int MaxPageSize = 50;
    private int _pageNumber = 1;
    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = (value <= 0) ? 1 : value;
    }

    private int _pageSize = 10;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : (value <= 0 ? 10 : value);
    }

    public GetAllHotelsQuery() { }
}