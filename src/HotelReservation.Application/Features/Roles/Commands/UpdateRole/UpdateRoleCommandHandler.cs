// src/HotelReservation.Application/Features/Roles/Commands/UpdateRole/UpdateRoleCommandHandler.cs
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Application.Exceptions; // برای NotFoundException
using HotelReservation.Domain.Entities;     // برای nameof(Role)
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HotelReservation.Application.Features.Roles.Commands.UpdateRole;

public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    // private readonly IMapper _mapper; // اگر از AutoMapper استفاده می‌کردیم

    public UpdateRoleCommandHandler(IUnitOfWork unitOfWork /*, IMapper mapper*/)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        // _mapper = mapper;
    }

    public async Task Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var roleToUpdate = await _unitOfWork.RoleRepository.GetByIdAsync(request.Id);
        if (roleToUpdate == null)
        {
            throw new NotFoundException(nameof(Role), request.Id);
        }

        // اعتبارسنجی یکتایی نام (اگر تغییر کرده باشد) توسط FluentValidation Pipeline Behavior انجام شده است.
        // اگر می‌خواهید یک لایه دفاعی دیگر داشته باشید یا Validator پیچیده بود، می‌توانستید اینجا هم چک کنید:
        // if (roleToUpdate.Name.Trim().ToLower() != request.Name.Trim().ToLower())
        // {
        //     var existingRoleWithNewName = await _unitOfWork.RoleRepository.GetByNameAsync(request.Name.Trim());
        //     if (existingRoleWithNewName != null && existingRoleWithNewName.Id != request.Id)
        //     {
        //         throw new BadRequestException($"نقشی با نام '{request.Name}' قبلاً برای رکورد دیگری ثبت شده است.");
        //     }
        // }

        // استفاده از متد موجودیت برای به‌روزرسانی
        roleToUpdate.UpdateDetails(request.Name, request.Description);

        // UpdateAsync از GenericRepository وضعیت موجودیت را Modified می‌کند.
        await _unitOfWork.RoleRepository.UpdateAsync(roleToUpdate);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // اگر از IRequest<Unit> استفاده می‌کردیم: return Unit.Value;
    }
}