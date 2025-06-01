// src/HotelReservation.Application/Features/UserManagement/Commands/AssignRole/AssignRoleToUserCommandValidator.cs
using FluentValidation;
using System;

namespace HotelReservation.Application.Features.UserManagement.Commands.AssignRole;

public class AssignRoleToUserCommandValidator : AbstractValidator<AssignRoleToUserCommand>
{
    public AssignRoleToUserCommandValidator()
    {
        RuleFor(p => p.UserId)
            .NotEmpty().WithMessage("شناسه کاربر الزامی است.")
            .NotEqual(Guid.Empty).WithMessage("شناسه کاربر معتبر نیست.");

        RuleFor(p => p.RoleId)
            .NotEmpty().WithMessage("شناسه نقش الزامی است.")
            .NotEqual(Guid.Empty).WithMessage("شناسه نقش معتبر نیست.");
        
        // اعتبارسنجی شرطی برای HotelId (که HotelId باید وجود داشته باشد اگر نقش "HotelUser" است)
        // معمولاً در Handler انجام می‌شود چون نیاز به دسترسی به داده (نام نقش) دارد.
        // اینجا فقط می‌توانیم بررسی کنیم که اگر HotelId مقدار دارد، Guid.Empty نباشد.
        RuleFor(p => p.HotelId)
            .NotEqual(Guid.Empty).WithMessage("شناسه هتل معتبر نیست.")
            .When(p => p.HotelId.HasValue);
    }
}