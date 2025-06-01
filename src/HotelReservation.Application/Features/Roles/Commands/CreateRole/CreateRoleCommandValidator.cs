// src/HotelReservation.Application/Features/Roles/Commands/CreateRole/CreateRoleCommandValidator.cs
using FluentValidation;
using HotelReservation.Application.Contracts.Persistence; // برای IRoleRepository (از طریق IUnitOfWork)
using System.Threading;
using System.Threading.Tasks;

namespace HotelReservation.Application.Features.Roles.Commands.CreateRole;

public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    private readonly IUnitOfWork _unitOfWork; // یا IRoleRepository به طور مستقیم

    public CreateRoleCommandValidator(IUnitOfWork unitOfWork) // تزریق وابستگی
    {
        _unitOfWork = unitOfWork;

        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("نام نقش الزامی است.")
            .MaximumLength(50).WithMessage("نام نقش نمی‌تواند بیشتر از {MaxLength} کاراکتر باشد.")
            .MustAsync(BeUniqueName).WithMessage("نقشی با این نام قبلاً ثبت شده است.");

        RuleFor(p => p.Description)
            .MaximumLength(250).WithMessage("توضیحات نقش نمی‌تواند بیشتر از {MaxLength} کاراکتر باشد.")
            .When(p => !string.IsNullOrEmpty(p.Description));
    }

    private async Task<bool> BeUniqueName(string name, CancellationToken cancellationToken)
    {
        // بررسی اینکه آیا نقشی با این نام قبلاً وجود دارد یا خیر
        return !(await _unitOfWork.RoleRepository.ExistsByNameAsync(name.Trim()));
        // یا اگر ExistsByNameAsync نداشتید:
        // var existingRole = await _unitOfWork.RoleRepository.GetByNameAsync(name.Trim());
        // return existingRole == null;
    }
}