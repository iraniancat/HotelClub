// src/HotelReservation.Application/Features/Roles/Commands/UpdateRole/UpdateRoleCommand.cs
using MediatR;
using System;

namespace HotelReservation.Application.Features.Roles.Commands.UpdateRole;

public class UpdateRoleCommand : IRequest // یا IRequest<Unit>
{
    public Guid Id { get; set; } // شناسه نقشی که باید به‌روز شود (از مسیر URL)
    public string Name { get; set; } // از بدنه درخواست (DTO)
    public string? Description { get; set; } // از بدنه درخواست (DTO)

    public UpdateRoleCommand(Guid id, string name, string? description)
    {
        Id = id;
        Name = name;
        Description = description;
    }
}