// src/HotelReservation.Application/Features/Roles/Commands/CreateRole/CreateRoleCommand.cs
using MediatR;
using System;

namespace HotelReservation.Application.Features.Roles.Commands.CreateRole;

public class CreateRoleCommand : IRequest<Guid> // شناسه نقش جدید را باز می‌گرداند
{
    public string Name { get; set; }
    public string? Description { get; set; }

    public CreateRoleCommand(string name, string? description)
    {
        Name = name;
        Description = description;
    }
}