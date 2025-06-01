// src/HotelReservation.Application/Features/Roles/Commands/DeleteRole/DeleteRoleCommandValidator.cs
using FluentValidation;
using System;

namespace HotelReservation.Application.Features.Roles.Commands.DeleteRole;

public class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
{
    public DeleteRoleCommandValidator()
    {
        RuleFor(p => p.Id)
            .NotEmpty().WithMessage("شناسه نقش برای حذف الزامی است.")
            .NotEqual(Guid.Empty).WithMessage("شناسه نقش معتبر نیست.");
    }
}