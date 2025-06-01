// src/HotelReservation.Application/Features/UserManagement/Commands/SetUserPassword/SetUserPasswordCommandHandler.cs
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Application.Contracts.Infrastructure; // برای IPasswordHasherService
using HotelReservation.Application.Exceptions; // برای NotFoundException
using HotelReservation.Domain.Entities; // برای nameof(User)
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HotelReservation.Application.Features.UserManagement.Commands.SetUserPassword;

public class SetUserPasswordCommandHandler : IRequestHandler<SetUserPasswordCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasherService _passwordHasher;

    public SetUserPasswordCommandHandler(IUnitOfWork unitOfWork, IPasswordHasherService passwordHasher)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    }

    public async Task Handle(SetUserPasswordCommand request, CancellationToken cancellationToken)
    {
        var userToUpdate = await _unitOfWork.UserRepository.GetByIdAsync(request.UserId);
        if (userToUpdate == null)
        {
            throw new NotFoundException(nameof(User), request.UserId);
        }

        // ملاحظه: در اینجا می‌توان بررسی کرد که آیا مدیر ارشد اجازه تغییر رمز این کاربر خاص را دارد یا خیر.
        // به عنوان مثال، شاید رمز عبور کاربرانی که از طریق SQL Job مدیریت می‌شوند (اگر passwordHash آنها هم از آنجا بیاید)
        // نباید توسط این مکانیزم تغییر کند، مگر اینکه این رمز عبور فقط برای این اپلیکیشن باشد.
        // فرض فعلی این است که مدیر ارشد می‌تواند رمز کاربران تحت مدیریتش در این اپلیکیشن را تغییر دهد.

        var hashedPassword = _passwordHasher.HashPassword(request.NewPassword);
        userToUpdate.ChangePassword(hashedPassword); 

        await _unitOfWork.UserRepository.UpdateAsync(userToUpdate); 
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}