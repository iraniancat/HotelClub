// src/HotelReservation.Application/Features/Roles/Commands/UpdateRole/UpdateRoleCommandValidator.cs
using FluentValidation;
using HotelReservation.Application.Contracts.Persistence; // برای IUnitOfWork یا IRoleRepository
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HotelReservation.Application.Features.Roles.Commands.UpdateRole;

public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateRoleCommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(p => p.Id)
            .NotEmpty().WithMessage("شناسه نقش الزامی است.")
            .NotEqual(Guid.Empty).WithMessage("شناسه نقش معتبر نیست.");

        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("نام نقش الزامی است.")
            .MaximumLength(50).WithMessage("نام نقش نمی‌تواند بیشتر از {MaxLength} کاراکتر باشد.")
            .MustAsync(BeUniqueNameWhenChanged).WithMessage("نقشی با این نام جدید قبلاً برای رکورد دیگری ثبت شده است.");

        RuleFor(p => p.Description)
            .MaximumLength(250).WithMessage("توضیحات نقش نمی‌تواند بیشتر از {MaxLength} کاراکتر باشد.")
            .When(p => !string.IsNullOrEmpty(p.Description));
    }

    private async Task<bool> BeUniqueNameWhenChanged(UpdateRoleCommand command, string newName, CancellationToken cancellationToken)
    {
        // بررسی اینکه آیا نقشی با نام جدید وجود دارد که ID آن با ID نقش فعلی متفاوت باشد
        var existingRoleWithNewName = await _unitOfWork.RoleRepository.GetByNameAsync(newName.Trim());
        if (existingRoleWithNewName == null)
        {
            return true; // نام جدید یکتا است
        }
        // اگر نقشی با نام جدید پیدا شد، باید ID آن با نقش در حال ویرایش متفاوت باشد
        return existingRoleWithNewName.Id == command.Id;
    }
}