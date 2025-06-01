// src/HotelReservation.Application/Features/Roles/Commands/CreateRole/CreateRoleCommandHandler.cs
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Domain.Entities; // برای موجودیت Role
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
// using HotelReservation.Application.Exceptions; // اگر بخواهیم برای نام تکراری اینجا هم خطا دهیم

namespace HotelReservation.Application.Features.Roles.Commands.CreateRole;

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    // private readonly IMapper _mapper; // اگر از AutoMapper استفاده می‌کردیم

    public CreateRoleCommandHandler(IUnitOfWork unitOfWork /*, IMapper mapper*/)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        // _mapper = mapper;
    }

    public async Task<Guid> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        // اعتبارسنجی یکتایی نام نقش توسط FluentValidation Pipeline Behavior انجام شده است.
        // اگر بخواهیم یک لایه دفاعی دیگر داشته باشیم، می‌توانیم اینجا هم چک کنیم:
        // if (await _unitOfWork.RoleRepository.ExistsByNameAsync(request.Name.Trim()))
        // {
        //     throw new BadRequestException($"نقشی با نام '{request.Name}' قبلاً وجود دارد.");
        // }

        var roleEntity = new Role(
            request.Name.Trim(), // بهتر است فضاهای خالی احتمالی ابتدا و انتها حذف شوند
            request.Description?.Trim()
        );

        // اگر از AutoMapper استفاده می‌کردیم:
        // var roleEntity = _mapper.Map<Role>(request);

        await _unitOfWork.RoleRepository.AddAsync(roleEntity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return roleEntity.Id;
    }
}