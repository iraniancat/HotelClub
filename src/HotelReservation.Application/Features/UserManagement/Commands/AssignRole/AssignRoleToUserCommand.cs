// src/HotelReservation.Application/Features/UserManagement/Commands/AssignRole/AssignRoleToUserCommand.cs
using MediatR;
using System;

namespace HotelReservation.Application.Features.UserManagement.Commands.AssignRole;

public class AssignRoleToUserCommand : IRequest // یا IRequest<Unit>
{
    public Guid UserId { get; } // از مسیر URL می‌آید
    public Guid RoleId { get; } // از بدنه درخواست (DTO)
    public Guid? HotelId { get; } // از بدنه درخواست (DTO)، اختیاری

    public AssignRoleToUserCommand(Guid userId, Guid roleId, Guid? hotelId)
    {
        UserId = userId;
        RoleId = roleId;
        HotelId = hotelId;
    }
}