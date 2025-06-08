// src/HotelReservation.Application/Features/UserManagement/Commands/AssignRole/AssignRoleToUserCommand.cs
using MediatR;
using System;

namespace HotelReservation.Application.Features.UserManagement.Commands.AssignRole;

public class AssignRoleToUserCommand : IRequest
{
    public Guid UserId { get; }
    public Guid RoleId { get; }
    public Guid? HotelId { get; }
    public string? ProvinceCode { get; } // <<-- اضافه شد

    public AssignRoleToUserCommand(Guid userId, Guid roleId, Guid? hotelId, string? provinceCode)
    {
        UserId = userId;
        RoleId = roleId;
        HotelId = hotelId;
        ProvinceCode = provinceCode; // <<-- اضافه شد
    }
}