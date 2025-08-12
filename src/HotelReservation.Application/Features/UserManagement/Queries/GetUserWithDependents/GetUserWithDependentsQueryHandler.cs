//    این Handler منطق واکشی داده‌ها را پیاده‌سازی می‌کند.
namespace HotelReservation.Application.Features.UserManagement.Queries.GetUserWithDependents;

using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Application.DTOs.UserManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class GetUserWithDependentsQueryHandler : IRequestHandler<GetUserWithDependentsQuery, UserWithDependentsDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUserWithDependentsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<UserWithDependentsDto?> Handle(GetUserWithDependentsQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.UserRepository.GetQueryable()
            .Where(u => u.Id == request.UserId)
            .Include(u => u.Dependents) // واکشی وابستگان به همراه کاربر
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            return null;
        }

        return new UserWithDependentsDto
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
        };
    }
}
