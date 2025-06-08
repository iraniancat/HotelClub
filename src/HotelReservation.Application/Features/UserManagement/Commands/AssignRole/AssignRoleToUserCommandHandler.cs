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
    private const string HotelUserRoleName = "HotelUser";
    private const string ProvinceUserRoleName = "ProvinceUser"; // نام نقش کاربر استان

    public AssignRoleToUserCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AssignRoleToUserCommand request, CancellationToken cancellationToken)
    {
        var userToUpdate = await _unitOfWork.UserRepository.GetByIdAsync(request.UserId);
        if (userToUpdate == null) throw new NotFoundException(nameof(User), request.UserId);

        var roleToAssign = await _unitOfWork.RoleRepository.GetByIdAsync(request.RoleId);
        if (roleToAssign == null) throw new BadRequestException($"نقش با شناسه '{request.RoleId}' یافت نشد.");

        // ۱. ابتدا نقش کاربر را تغییر می‌دهیم.
        // متد ChangeRole در موجودیت User باید تخصیص‌های قبلی (هتل و استان) را پاک کند.
        userToUpdate.ChangeRole(roleToAssign.Id, roleToAssign);

        // ۲. بر اساس نقش جدید، تخصیص‌های لازم را انجام می‌دهیم.
        if (roleToAssign.Name == ProvinceUserRoleName)
        {
            if (string.IsNullOrWhiteSpace(request.ProvinceCode))
            {
                throw new BadRequestException("برای تخصیص نقش کاربر استان، انتخاب استان الزامی است.");
            }
            var provinceToAssign = await _unitOfWork.ProvinceRepository.GetByStringIdAsync(request.ProvinceCode);
            if (provinceToAssign == null)
            {
                throw new BadRequestException($"استان با کد '{request.ProvinceCode}' یافت نشد.");
            }
            userToUpdate.AssignToProvince(provinceToAssign.Code, provinceToAssign, provinceToAssign.Name); // فراخوانی متد جدید در موجودیت User
        }
        else if (roleToAssign.Name == HotelUserRoleName)
        {
            if (!request.HotelId.HasValue || request.HotelId.Value == Guid.Empty)
            {
                throw new BadRequestException($"برای تخصیص نقش '{HotelUserRoleName}'، شناسه هتل الزامی است.");
            }
            var hotelToAssign = await _unitOfWork.HotelRepository.GetByIdAsync(request.HotelId.Value);
            if (hotelToAssign == null)
            {
                throw new BadRequestException($"هتل با شناسه '{request.HotelId.Value}' یافت نشد.");
            }
            userToUpdate.AssignToHotel(hotelToAssign.Id, hotelToAssign);
        }

        await _unitOfWork.UserRepository.UpdateAsync(userToUpdate);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}