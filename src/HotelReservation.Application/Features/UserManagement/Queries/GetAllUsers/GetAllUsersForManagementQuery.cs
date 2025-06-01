// src/HotelReservation.Application/Features/UserManagement/Queries/GetAllUsers/GetAllUsersForManagementQuery.cs
using HotelReservation.Application.DTOs.Common;
using HotelReservation.Application.DTOs.UserManagement;
using MediatR;
using System;

namespace HotelReservation.Application.Features.UserManagement.Queries.GetAllUsers;

public class GetAllUsersForManagementQuery : IRequest<PagedResult<UserManagementListDto>>
{
    public string? SearchTerm { get; set; }
    public Guid? RoleIdFilter { get; set; } // فیلتر بر اساس شناسه نقش
    public bool? IsActiveFilter { get; set; }

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

    // سازنده می‌تواند برای تست یا مقداردهی اولیه پارامترها استفاده شود
    public GetAllUsersForManagementQuery() { }
}