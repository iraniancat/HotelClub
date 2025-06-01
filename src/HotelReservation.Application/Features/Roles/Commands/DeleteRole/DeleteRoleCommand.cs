// src/HotelReservation.Application/Features/Roles/Commands/DeleteRole/DeleteRoleCommand.cs
using MediatR;
using System;

namespace HotelReservation.Application.Features.Roles.Commands.DeleteRole;

public class DeleteRoleCommand : IRequest // یا IRequest<Unit>
{
    public Guid Id { get; }

    public DeleteRoleCommand(Guid id)
    {
        Id = id;
    }
}