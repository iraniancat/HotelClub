// src/HotelReservation.Application/Features/UserManagement/Commands/SetUserPassword/SetUserPasswordCommandValidator.cs
using FluentValidation;
using System;

namespace HotelReservation.Application.Features.UserManagement.Commands.SetUserPassword;

public class SetUserPasswordCommandValidator : AbstractValidator<SetUserPasswordCommand>
{
    public SetUserPasswordCommandValidator()
    {
        RuleFor(p => p.UserId)
            .NotEmpty().WithMessage("شناسه کاربر الزامی است.")
            .NotEqual(Guid.Empty).WithMessage("شناسه کاربر معتبر نیست.");

        RuleFor(p => p.NewPassword)
            .NotEmpty().WithMessage("رمز عبور جدید الزامی است.")
            .MinimumLength(6).WithMessage("رمز عبور جدید باید حداقل ۶ کاراکتر باشد.");
            // می‌توان قوانین پیچیدگی رمز عبور را نیز در اینجا اضافه کرد

        RuleFor(p => p.ConfirmPassword)
            .NotEmpty().WithMessage("تکرار رمز عبور جدید الزامی است.")
            .Equal(p => p.NewPassword).WithMessage("رمز عبور جدید و تکرار آن باید یکسان باشند.");
    }
}