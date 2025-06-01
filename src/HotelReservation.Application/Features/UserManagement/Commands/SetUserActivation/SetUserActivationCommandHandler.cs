// src/HotelReservation.Application/Features/UserManagement/Commands/SetUserActivation/SetUserActivationCommandHandler.cs
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Application.Exceptions; // برای NotFoundException
using HotelReservation.Domain.Entities; // برای nameof(User)
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HotelReservation.Application.Features.UserManagement.Commands.SetUserActivation;

public class SetUserActivationCommandHandler : IRequestHandler<SetUserActivationCommand> // یا IRequestHandler<SetUserActivationCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public SetUserActivationCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task Handle(SetUserActivationCommand request, CancellationToken cancellationToken)
    {
        var userToUpdate = await _unitOfWork.UserRepository.GetByIdAsync(request.UserId);

        if (userToUpdate == null)
        {
            throw new NotFoundException(nameof(User), request.UserId);
        }

        if (request.IsActive)
        {
            userToUpdate.Activate(); // متدی که در موجودیت User تعریف کردیم
        }
        else
        {
            userToUpdate.Deactivate(); // متدی که در موجودیت User تعریف کردیم
        }

        // UpdateAsync از GenericRepository وضعیت موجودیت را Modified می‌کند.
        // اگرچه EF Core برای موجودیت‌های Track شده تغییرات را خودکار تشخیص می‌دهد،
        // فراخوانی صریح آن برای اطمینان ضرری ندارد.
        await _unitOfWork.UserRepository.UpdateAsync(userToUpdate); 

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // اگر از IRequest<Unit> استفاده می‌کردیم: return Unit.Value;
    }
}