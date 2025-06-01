// src/HotelReservation.Application/Features/UserManagement/Commands/AssignRole/AssignRoleToUserCommandHandler.cs
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Application.Exceptions; // برای NotFoundException و BadRequestException
using HotelReservation.Domain.Entities; // برای nameof(User), nameof(Role), nameof(Hotel)
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HotelReservation.Application.Features.UserManagement.Commands.AssignRole;

public class AssignRoleToUserCommandHandler : IRequestHandler<AssignRoleToUserCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    // نام نقش کاربر هتل - بهتر است این از یک منبع تنظیمات یا ثابت‌ها خوانده شود.
    private const string HotelUserRoleName = "HotelUser";


    public AssignRoleToUserCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task Handle(AssignRoleToUserCommand request, CancellationToken cancellationToken)
    {
        var userToUpdate = await _unitOfWork.UserRepository.GetByIdAsync(request.UserId);
        if (userToUpdate == null)
        {
            throw new NotFoundException(nameof(User), request.UserId);
        }

        var roleToAssign = await _unitOfWork.RoleRepository.GetByIdAsync(request.RoleId);
        if (roleToAssign == null)
        {
            throw new NotFoundException(nameof(Role), request.RoleId); // یا BadRequestException
        }

        // منطق برای نقش کاربر هتل
        if (roleToAssign.Name == HotelUserRoleName)
        {
            if (!request.HotelId.HasValue || request.HotelId.Value == Guid.Empty)
            {
                throw new BadRequestException($"برای تخصیص نقش '{HotelUserRoleName}'، شناسه هتل الزامی است.");
            }

            var hotelToAssign = await _unitOfWork.HotelRepository.GetByIdAsync(request.HotelId.Value);
            if (hotelToAssign == null)
            {
                throw new NotFoundException(nameof(Hotel), request.HotelId.Value); // یا BadRequestException
            }
            
            userToUpdate.ChangeRole(roleToAssign.Id, roleToAssign); // متد موجودیت User
            userToUpdate.AssignToHotel(hotelToAssign.Id, hotelToAssign); // متد موجودیت User
        }
        else
        {
            userToUpdate.ChangeRole(roleToAssign.Id, roleToAssign); // این متد باید HotelId را null کند
        }

        await _unitOfWork.UserRepository.UpdateAsync(userToUpdate); // برای اطمینان از علامت‌گذاری برای به‌روزرسانی
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}