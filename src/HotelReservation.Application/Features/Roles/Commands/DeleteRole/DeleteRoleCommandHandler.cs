// src/HotelReservation.Application/Features/Roles/Commands/DeleteRole/DeleteRoleCommandHandler.cs
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Application.Exceptions; // برای NotFoundException و BadRequestException
using HotelReservation.Domain.Entities;     // برای nameof(Role)
using MediatR;
using System;
using System.Linq; // برای Any()
using System.Threading;
using System.Threading.Tasks;

namespace HotelReservation.Application.Features.Roles.Commands.DeleteRole;

public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteRoleCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var roleToDelete = await _unitOfWork.RoleRepository.GetByIdAsync(request.Id);
        if (roleToDelete == null)
        {
            throw new NotFoundException(nameof(Role), request.Id);
        }

        // بررسی اینکه آیا کاربری به این نقش تخصیص داده شده است یا خیر
        var usersWithThisRole = await _unitOfWork.UserRepository.GetUsersByRoleIdAsync(request.Id);
        if (usersWithThisRole.Any())
        {
            throw new BadRequestException("امکان حذف این نقش وجود ندارد زیرا به یک یا چند کاربر تخصیص داده شده است. ابتدا تخصیص نقش به کاربران را تغییر دهید.");
        }

        // اگر نقشی مانند "SuperAdmin" یا نقش‌های سیستمی دیگر نباید حذف شوند، اینجا می‌توانید بررسی کنید:
        // if (roleToDelete.Name == "SuperAdmin" || roleToDelete.Name == "Employee") // نام‌های رزرو شده
        // {
        //     throw new BadRequestException($"نقش '{roleToDelete.Name}' یک نقش سیستمی است و قابل حذف نیست.");
        // }

        await _unitOfWork.RoleRepository.DeleteAsync(roleToDelete);
        // یا می‌توان از DeleteByIdAsync استفاده کرد: await _unitOfWork.RoleRepository.DeleteByIdAsync(request.Id);
        // در این صورت، بررسی NotFoundException باید توسط DeleteByIdAsync انجام شود یا بعد از آن.

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // اگر از IRequest<Unit> استفاده می‌کردیم: return Unit.Value;
    }
}