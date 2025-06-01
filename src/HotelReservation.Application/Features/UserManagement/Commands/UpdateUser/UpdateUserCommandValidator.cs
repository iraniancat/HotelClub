// src/HotelReservation.Application/Features/UserManagement/Commands/UpdateUser/UpdateUserCommandValidator.cs
using FluentValidation;
using System;

namespace HotelReservation.Application.Features.UserManagement.Commands.UpdateUser;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(p => p.UserId)
            .NotEmpty().WithMessage("شناسه کاربر الزامی است.")
            .NotEqual(Guid.Empty).WithMessage("شناسه کاربر معتبر نیست.");

        RuleFor(p => p.FullName)
            .NotEmpty().WithMessage("نام کامل الزامی است.")
            .MaximumLength(200);

        RuleFor(p => p.RoleId)
            .NotEmpty().WithMessage("شناسه نقش الزامی است.")
            .NotEqual(Guid.Empty).WithMessage("شناسه نقش معتبر نیست.");

        RuleFor(p => p.NationalCode)
            .MaximumLength(10)
            .Matches(@"^[0-9]*$").WithMessage("کد ملی فقط می‌تواند شامل ارقام باشد.")
            .When(p => !string.IsNullOrEmpty(p.NationalCode));
            
        RuleFor(p => p.PhoneNumber) // <<-- اضافه شد
           .MaximumLength(20).WithMessage("شماره تلفن نمی‌تواند بیشتر از ۲۰ کاراکتر باشد.")
           .Matches(@"^[0-9\+\-\(\)\s]*$").WithMessage("فرمت شماره تلفن معتبر نیست.") // یک نمونه ساده برای اعتبارسنجی فرمت
           .When(p => !string.IsNullOrEmpty(p.PhoneNumber));

        RuleFor(p => p.ProvinceCode)
            .MaximumLength(10)
            .When(p => !string.IsNullOrEmpty(p.ProvinceCode));

        RuleFor(p => p.DepartmentCode)
            .MaximumLength(20)
            .When(p => !string.IsNullOrEmpty(p.DepartmentCode));

        RuleFor(p => p.HotelId)
            .NotEqual(Guid.Empty).WithMessage("شناسه هتل معتبر نیست.")
            .When(p => p.HotelId.HasValue);
    }
}