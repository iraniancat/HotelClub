// مسیر: src/HotelReservation.Application/Features/UserManagement/Queries/SearchUsers/SearchUsersQueryHandler.cs
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Application.Contracts.Security; // <<-- اضافه شد
using HotelReservation.Application.DTOs.UserManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // <<-- اضافه شد
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HotelReservation.Application.Features.UserManagement.Queries.SearchUsers;

public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, IEnumerable<UserWithDependentsDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService; // <<-- اضافه شد
    private readonly ILogger<SearchUsersQueryHandler> _logger; // <<-- اضافه شد

    public SearchUsersQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<SearchUsersQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<IEnumerable<UserWithDependentsDto>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SearchTerm) || request.SearchTerm.Length < 2)
        {
            return new List<UserWithDependentsDto>();
        }

        var searchTermLower = request.SearchTerm.ToLower().Trim();

        var query = _unitOfWork.UserRepository.GetQueryable()
            .Where(u => u.SystemUserId.StartsWith(searchTermLower) || u.FullName.ToLower().Contains(searchTermLower));
        _logger.LogWarning("ProvinceUser {UserId} has no ProvinceCode claim.");
        // <<-- شروع منطق فیلترینگ بر اساس نقش کاربر -->>
        if (_currentUserService.IsInRole("ProvinceUser"))
        {
            var currentUserProvinceCode = _currentUserService.ProvinceCode;
            if (string.IsNullOrWhiteSpace(currentUserProvinceCode))
            {
                _logger.LogWarning("ProvinceUser {UserId} has no ProvinceCode claim. Returning empty search results.", _currentUserService.UserId);
                return new List<UserWithDependentsDto>();
            }

            // فیلتر کردن نتایج برای کاربران همان استان
            query = query.Where(u => u.ProvinceCode == currentUserProvinceCode);
            _logger.LogInformation("SearchUsersQueryHandler: Filtering search results for ProvinceUser from Province {ProvinceCode}", currentUserProvinceCode);
        }
        // برای SuperAdmin، هیچ فیلتر اضافه‌ای اعمال نمی‌شود.
        // <<-- پایان منطق فیلترینگ -->>

        var users = await query
            .Include(u => u.Dependents)
            .Take(10)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return users.Select(user => new UserWithDependentsDto
        {
            Id = user.Id,
            SystemUserId = user.SystemUserId,
            FullName = user.FullName,
            NationalCode = user.NationalCode ?? string.Empty,
            Dependents = user.Dependents.Select(d => new DependentSlimDto
            {
                FullName = d.FullName,
                NationalCode = d.NationalCode,
                Relationship = d.Relationship
            }).ToList()
        }).ToList();
    }
}
