// src/HotelReservation.Application/Features/UserManagement/Commands/SetUserActivation/SetUserActivationCommandValidator.cs
using FluentValidation;
using System;

namespace HotelReservation.Application.Features.UserManagement.Commands.SetUserActivation;

public class SetUserActivationCommandValidator : AbstractValidator<SetUserActivationCommand>
{
    public SetUserActivationCommandValidator()
    {
        RuleFor(p => p.UserId)
            .NotEmpty().WithMessage("شناسه کاربر الزامی است.")
            .NotEqual(Guid.Empty).WithMessage("شناسه کاربر معتبر نیست.");

        // برای فیلد IsActive (بولین) معمولاً اعتبارسنجی خاصی لازم نیست مگر اینکه منطق خاصی داشته باشید.
    }
}