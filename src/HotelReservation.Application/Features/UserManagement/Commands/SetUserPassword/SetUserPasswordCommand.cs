// src/HotelReservation.Application/Features/UserManagement/Commands/SetUserPassword/SetUserPasswordCommand.cs
using MediatR;
using System;

namespace HotelReservation.Application.Features.UserManagement.Commands.SetUserPassword;

public class SetUserPasswordCommand : IRequest // یا IRequest<Unit>
{
    public Guid UserId { get; } 
    public string NewPassword { get; } 
    public string ConfirmPassword { get; } 

    public SetUserPasswordCommand(Guid userId, string newPassword, string confirmPassword)
    {
        UserId = userId;
        NewPassword = newPassword;
        ConfirmPassword = confirmPassword;
    }
}