// src/HotelReservation.Application/Features/UserManagement/Queries/GetAllUsers/GetAllUsersForManagementQueryHandler.cs
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Application.DTOs.Common;
using HotelReservation.Application.DTOs.UserManagement;
using HotelReservation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace HotelReservation.Application.Features.UserManagement.Queries.GetAllUsers;

public class GetAllUsersForManagementQueryHandler : IRequestHandler<GetAllUsersForManagementQuery, PagedResult<UserManagementListDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAllUsersForManagementQueryHandler> _logger;

    public GetAllUsersForManagementQueryHandler(IUnitOfWork unitOfWork, ILogger<GetAllUsersForManagementQueryHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PagedResult<UserManagementListDto>> Handle(GetAllUsersForManagementQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Fetching users for management. Page: {PageNumber}, Size: {PageSize}, Search: '{SearchTerm}', RoleIdFilter: '{RoleIdFilter}', IsActiveFilter: {IsActiveFilter}",
            request.PageNumber, request.PageSize, request.SearchTerm, request.RoleIdFilter, request.IsActiveFilter);

        IQueryable<User> query = _unitOfWork.UserRepository.GetQueryable()
                               .Include(u => u.Role)
                               .Include(u => u.Province)
                               .Include(u => u.Department)
                               .Include(u => u.AssignedHotel);

        // ۱. اعمال فیلتر جستجو (SearchTerm)
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTermLower = request.SearchTerm.ToLower().Trim();
            query = query.Where(u =>
                (u.SystemUserId.ToLower().Contains(searchTermLower)) ||
                (u.FullName.ToLower().Contains(searchTermLower)) ||
                (u.NationalCode != null && u.NationalCode.Contains(searchTermLower)) ||
                (u.Role != null && u.Role.Name.ToLower().Contains(searchTermLower)) ||
                (u.ProvinceName != null && u.ProvinceName.ToLower().Contains(searchTermLower)) || // جستجو در نام استان دنرمالایز شده
                (u.DepartmentName != null && u.DepartmentName.ToLower().Contains(searchTermLower)) || // جستجو در نام دپارتمان دنرمالایز شده
                (u.AssignedHotel != null && u.AssignedHotel.Name.ToLower().Contains(searchTermLower)) // جستجو در نام هتل
            );
        }

        // ۲. اعمال فیلتر نقش (RoleIdFilter)
        if (request.RoleIdFilter.HasValue && request.RoleIdFilter.Value != Guid.Empty)
        {
            query = query.Where(u => u.RoleId == request.RoleIdFilter.Value);
        }

        // ۳. اعمال فیلتر وضعیت فعالیت (IsActiveFilter)
        if (request.IsActiveFilter.HasValue)
        {
            query = query.Where(u => u.IsActive == request.IsActiveFilter.Value);
        }

        // ۴. گرفتن تعداد کل رکوردها (پس از فیلترها، قبل از صفحه‌بندی)
        var totalCount = await query.CountAsync(cancellationToken);

        // ۵. اعمال ترتیب و صفحه‌بندی
        var users = await query
            .OrderBy(u => u.FullName) // یا هر ترتیب دیگر مورد نظر
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // ۶. نگاشت به DTO
        var userDtos = users.Select(user => new UserManagementListDto
        {
            Id = user.Id,
            SystemUserId = user.SystemUserId,
            FullName = user.FullName,
            NationalCode = user.NationalCode,
            IsActive = user.IsActive,
            RoleName = user.Role?.Name ?? "تعیین نشده",
            ProvinceName = user.ProvinceName ?? user.Province?.Name, // اولویت با فیلد دنرمالایز شده
            DepartmentName = user.DepartmentName ?? user.Department?.Name, // اولویت با فیلد دنرمالایز شده
            AssignedHotelName = user.AssignedHotel?.Name
        }).ToList();

        return new PagedResult<UserManagementListDto>(userDtos, totalCount, request.PageNumber, request.PageSize);
    }
}