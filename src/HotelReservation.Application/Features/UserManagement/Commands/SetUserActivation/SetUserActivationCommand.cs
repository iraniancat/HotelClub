// src/HotelReservation.Application/Features/UserManagement/Commands/SetUserActivation/SetUserActivationCommand.cs
using MediatR; // برای IRequest
using System;   // برای Guid

namespace HotelReservation.Application.Features.UserManagement.Commands.SetUserActivation;

public class SetUserActivationCommand : IRequest // یا IRequest<Unit>
{
    public Guid UserId { get; }
    public bool IsActive { get; }

    public SetUserActivationCommand(Guid userId, bool isActive)
    {
        UserId = userId;
        IsActive = isActive;
    }
}