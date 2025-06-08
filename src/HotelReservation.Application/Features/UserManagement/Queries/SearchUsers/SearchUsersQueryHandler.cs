namespace HotelReservation.Application.Features.UserManagement.Queries.SearchUsers;

using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Application.DTOs.UserManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, IEnumerable<UserWithDependentsDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public SearchUsersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<UserWithDependentsDto>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SearchTerm) || request.SearchTerm.Length < 2)
        {
            return new List<UserWithDependentsDto>();
        }

        var searchTermLower = request.SearchTerm.ToLower().Trim();
        
        // جستجو بر اساس شماره پرسنلی یا نام کامل
        var users = await _unitOfWork.UserRepository.GetQueryable()
            .Where(u => u.SystemUserId.StartsWith(searchTermLower) || u.FullName.ToLower().Contains(searchTermLower))
            .Include(u => u.Dependents) // واکشی وابستگان به همراه کاربر
            .Take(10) // محدود کردن نتایج برای جلوگیری از ارسال داده زیاد
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
