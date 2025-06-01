// src/HotelReservation.Application/Features/UserManagement/Commands/UpdateUser/UpdateUserCommandHandler.cs
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Application.Exceptions;
using HotelReservation.Domain.Entities;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HotelReservation.Application.Features.UserManagement.Commands.UpdateUser;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private const string HotelUserRoleName = "HotelUser"; // باید با نام واقعی نقش در پایگاه داده مطابقت داشته باشد

    public UpdateUserCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var userToUpdate = await _unitOfWork.UserRepository.GetByIdAsync(request.UserId);
        if (userToUpdate == null)
        {
            throw new NotFoundException(nameof(User), request.UserId);
        }

        var newRole = await _unitOfWork.RoleRepository.GetByIdAsync(request.RoleId);
        if (newRole == null)
        {
            throw new BadRequestException($"نقش با شناسه '{request.RoleId}' یافت نشد.");
        }

        Province? newProvince = null;
        if (!string.IsNullOrEmpty(request.ProvinceCode))
        {
            newProvince = await _unitOfWork.ProvinceRepository.GetByStringIdAsync(request.ProvinceCode);
            if (newProvince == null) throw new BadRequestException($"استان با کد '{request.ProvinceCode}' یافت نشد.");
        }

        Department? newDepartment = null;
        if (!string.IsNullOrEmpty(request.DepartmentCode))
        {
            newDepartment = await _unitOfWork.DepartmentRepository.GetByStringIdAsync(request.DepartmentCode);
            if (newDepartment == null) throw new BadRequestException($"اداره/شعبه با کد '{request.DepartmentCode}' یافت نشد.");
        }

        Hotel? newHotel = null;
        if (newRole.Name == "HotelUser")
        {
            if (!request.HotelId.HasValue || request.HotelId.Value == Guid.Empty)
            {
                throw new BadRequestException($"برای نقش 'HotelUser'، شناسه هتل الزامی است.");
            }
            newHotel = await _unitOfWork.HotelRepository.GetByIdAsync(request.HotelId.Value);
            if (newHotel == null) throw new BadRequestException($"هتل با شناسه '{request.HotelId.Value}' یافت نشد.");
        }
        // استفاده از متد موجودیت برای به‌روزرسانی
        userToUpdate.UpdateDetailsByAdmin(
            request.FullName,
            request.IsActive,
            request.RoleId, newRole,
            request.NationalCode,
            request.PhoneNumber, // <<-- آرگومان جدید اضافه شد
            request.ProvinceCode, newProvince, request.ProvinceCode != null ? newProvince?.Name : null,
            request.DepartmentCode, newDepartment, request.DepartmentCode != null ? newDepartment?.Name : null,
            (newRole.Name == "HotelUser") ? request.HotelId : null,
            (newRole.Name == "HotelUser") ? newHotel : null
        );

        await _unitOfWork.UserRepository.UpdateAsync(userToUpdate);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}