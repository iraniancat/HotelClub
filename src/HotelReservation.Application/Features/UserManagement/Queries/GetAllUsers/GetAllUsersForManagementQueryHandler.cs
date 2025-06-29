// src/HotelReservation.Application/Features/UserManagement/Queries/GetAllUsers/GetAllUsersForManagementQueryHandler.cs
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Application.Contracts.Security;
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
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetAllUsersForManagementQueryHandler> _logger;

    public GetAllUsersForManagementQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, ILogger<GetAllUsersForManagementQueryHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

 public async Task<PagedResult<UserManagementListDto>> Handle(GetAllUsersForManagementQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching users for management by User: {UserId}, Role: {UserRole}",
            _currentUserService.UserId, _currentUserService.UserRole);

        IQueryable<User> query = _unitOfWork.UserRepository.GetQueryable()
                                  .Include(u => u.Role)
                                  .Include(u => u.Province)
                                  .Include(u => u.Department)
                                  .Include(u => u.AssignedHotel);

        // <<-- شروع منطق فیلترینگ بر اساس نقش کاربر -->>
        if (_currentUserService.IsInRole("ProvinceUser"))
        {
            var currentUserProvinceCode = _currentUserService.ProvinceCode;
            if (string.IsNullOrWhiteSpace(currentUserProvinceCode))
            {
                _logger.LogWarning("ProvinceUser {UserId} has no ProvinceCode claim. Returning empty user list.", _currentUserService.UserId);
                return new PagedResult<UserManagementListDto>(new List<UserManagementListDto>(), 0, request.PageNumber, request.PageSize);
            }
            
            query = query.Where(u => u.ProvinceCode == currentUserProvinceCode);
            _logger.LogInformation("Filtering users for ProvinceUser. ProvinceCode: {ProvinceCode}", currentUserProvinceCode);
        }
        // برای نقش SuperAdmin، هیچ فیلتر مبتنی بر نقشی اعمال نمی‌شود و تمام کاربران نمایش داده می‌شوند.
        // <<-- پایان منطق فیلترینگ -->>


        // اعمال فیلترهای دیگر (جستجو، نقش، وضعیت)
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTermLower = request.SearchTerm.ToLower().Trim();
            query = query.Where(u =>
                (u.SystemUserId.ToLower().Contains(searchTermLower)) ||
                (u.FullName.ToLower().Contains(searchTermLower)) ||
                (u.NationalCode != null && u.NationalCode.Contains(searchTermLower))
            );
        }

        if (request.RoleIdFilter.HasValue && request.RoleIdFilter.Value != Guid.Empty)
        {
            query = query.Where(u => u.RoleId == request.RoleIdFilter.Value);
        }

        if (request.IsActiveFilter.HasValue)
        {
            query = query.Where(u => u.IsActive == request.IsActiveFilter.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .OrderBy(u => u.FullName)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var userDtos = users.Select(user => new UserManagementListDto
        {
            Id = user.Id,
            SystemUserId = user.SystemUserId,
            FullName = user.FullName,
            NationalCode = user.NationalCode,
            IsActive = user.IsActive,
            RoleName = user.Role?.Name ?? "تعیین نشده",
            ProvinceName = user.ProvinceName ?? user.Province?.Name,
            DepartmentName = user.DepartmentName ?? user.Department?.Name,
            AssignedHotelName = user.AssignedHotel?.Name
        }).ToList();

        return new PagedResult<UserManagementListDto>(userDtos, totalCount, request.PageNumber, request.PageSize);
    }
}