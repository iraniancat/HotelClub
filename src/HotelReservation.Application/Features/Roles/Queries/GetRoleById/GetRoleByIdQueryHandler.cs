// src/HotelReservation.Application/Features/Roles/Queries/GetRoleById/GetRoleByIdQueryHandler.cs
using HotelReservation.Application.Contracts.Persistence; // برای IUnitOfWork
using HotelReservation.Application.DTOs.Role; // برای RoleDto
// using HotelReservation.Application.Exceptions; // اگر بخواهیم NotFoundException پرتاب کنیم
using MediatR; // برای IRequestHandler
using System;
using System.Threading;
using System.Threading.Tasks;
// using AutoMapper; // اگر از AutoMapper استفاده می‌کردیم

namespace HotelReservation.Application.Features.Roles.Queries.GetRoleById;

public class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, RoleDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    // private readonly IMapper _mapper;

    public GetRoleByIdQueryHandler(IUnitOfWork unitOfWork /*, IMapper mapper*/)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        // _mapper = mapper;
    }

    public async Task<RoleDto?> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        var role = await _unitOfWork.RoleRepository.GetByIdAsync(request.Id);

        if (role == null)
        {
            // throw new NotFoundException(nameof(Role), request.Id);
            return null; // Controller این حالت را به 404 Not Found تبدیل می‌کند
        }

        // اگر از AutoMapper استفاده می‌کردیم:
        // return _mapper.Map<RoleDto>(role);

        // نگاشت دستی
        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description
        };
    }
}