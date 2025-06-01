// src/HotelReservation.Application/Features/Roles/Queries/GetAllRoles/GetAllRolesQueryHandler.cs
using HotelReservation.Application.Contracts.Persistence; // برای IUnitOfWork
using HotelReservation.Application.DTOs.Role; // برای RoleDto
using MediatR; // برای IRequestHandler
using System.Collections.Generic;
using System.Linq; // برای Select
using System.Threading;
using System.Threading.Tasks;
// using AutoMapper; // اگر از AutoMapper استفاده می‌کردیم

namespace HotelReservation.Application.Features.Roles.Queries.GetAllRoles;

public class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, IEnumerable<RoleDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    // private readonly IMapper _mapper;

    public GetAllRolesQueryHandler(IUnitOfWork unitOfWork /*, IMapper mapper*/)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        // _mapper = mapper;
    }

    public async Task<IEnumerable<RoleDto>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _unitOfWork.RoleRepository.GetAllAsync(); // متد GetAllAsync از IGenericRepository

        // اگر از AutoMapper استفاده می‌کردیم:
        // return _mapper.Map<IEnumerable<RoleDto>>(roles);

        // نگاشت دستی
        if (roles == null || !roles.Any())
        {
            return new List<RoleDto>();
        }

        return roles.Select(role => new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description
        }).ToList();
    }
}