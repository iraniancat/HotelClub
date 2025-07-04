// src/HotelReservation.Application/Features/UserManagement/Commands/CreateNonEmployeeUser/CreateNonEmployeeUserCommandValidator.cs
using FluentValidation;
using System;

namespace HotelReservation.Application.Features.UserManagement.Commands.CreateNonEmployeeUser;

public class CreateNonEmployeeUserCommandValidator : AbstractValidator<CreateNonEmployeeUserCommand>
{
    public CreateNonEmployeeUserCommandValidator()
    {
        RuleFor(p => p.SystemUserId)
            .NotEmpty().WithMessage("شناسه کاربری سیستم الزامی است.")
            .MaximumLength(100);

        RuleFor(p => p.FullName)
            .NotEmpty().WithMessage("نام کامل الزامی است.")
            .MaximumLength(200);

        RuleFor(p => p.Password)
            .NotEmpty().WithMessage("رمز عبور الزامی است.")
            .MinimumLength(6).WithMessage("رمز عبور باید حداقل ۶ کاراکتر باشد.");

        RuleFor(p => p.RoleId)
            .NotEmpty().WithMessage("شناسه نقش الزامی است.")
            .NotEqual(Guid.Empty).WithMessage("شناسه نقش معتبر نیست.");
        
        RuleFor(p => p.NationalCode)
            .MaximumLength(10).WithMessage("کد ملی نمی‌تواند بیشتر از ۱۰ کاراکتر باشد.")
            .Matches(@"^[0-9]*$").WithMessage("کد ملی فقط می‌تواند شامل ارقام باشد.") // نمونه اعتبارسنجی فرمت
            .When(p => !string.IsNullOrEmpty(p.NationalCode));
    RuleFor(p => p.PhoneNumber) // <<-- اضافه شد
            .MaximumLength(20).WithMessage("شماره تلفن نمی‌تواند بیشتر از {MaxLength} کاراکتر باشد.")
            .Matches(@"^[0-9\+\-\(\)\s]*$").WithMessage("فرمت شماره تلفن معتبر نیست.")
            .When(p => !string.IsNullOrEmpty(p.PhoneNumber));
        // اعتبارسنجی برای ProvinceCode, DepartmentCode, HotelId می‌تواند پیچیده‌تر باشد
        // و نیاز به بررسی وجود آن‌ها در پایگاه داده داشته باشد (معمولاً در Handler یا با Custom Validator)
        // فعلاً فقط طول را بررسی می‌کنیم اگر مقدار داشته باشند.
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