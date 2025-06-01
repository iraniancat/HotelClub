// src/HotelReservation.Application/Features/Roles/Queries/GetAllRoles/GetAllRolesQuery.cs
using HotelReservation.Application.DTOs.Role; // برای RoleDto
using MediatR; // برای IRequest
using System.Collections.Generic; // برای IEnumerable

namespace HotelReservation.Application.Features.Roles.Queries.GetAllRoles;

public class GetAllRolesQuery : IRequest<IEnumerable<RoleDto>>
{
    // این Query در حال حاضر پارامتری برای فیلتر یا صفحه‌بندی ندارد.
}