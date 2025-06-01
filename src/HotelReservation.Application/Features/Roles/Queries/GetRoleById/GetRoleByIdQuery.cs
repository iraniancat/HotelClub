// src/HotelReservation.Application/Features/Roles/Queries/GetRoleById/GetRoleByIdQuery.cs
using HotelReservation.Application.DTOs.Role; // برای RoleDto
using MediatR; // برای IRequest
using System; // برای Guid

namespace HotelReservation.Application.Features.Roles.Queries.GetRoleById;

public class GetRoleByIdQuery : IRequest<RoleDto?> // نتیجه می‌تواند RoleDto یا null باشد
{
    public Guid Id { get; }

    public GetRoleByIdQuery(Guid id)
    {
        Id = id;
    }
}