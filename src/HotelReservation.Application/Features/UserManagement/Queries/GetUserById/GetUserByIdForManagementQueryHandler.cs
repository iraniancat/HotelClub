// src/HotelReservation.Application/Features/UserManagement/Queries/GetUserById/GetUserByIdForManagementQueryHandler.cs
using HotelReservation.Application.Contracts.Persistence; // برای IUnitOfWork
using HotelReservation.Application.DTOs.UserManagement; // برای UserManagementDetailsDto
// using HotelReservation.Application.Exceptions; // اگر بخواهیم NotFoundException پرتاب کنیم
using MediatR; // برای IRequestHandler
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HotelReservation.Application.Features.UserManagement.Queries.GetUserById;

public class GetUserByIdForManagementQueryHandler : IRequestHandler<GetUserByIdForManagementQuery, UserManagementDetailsDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    // private readonly IMapper _mapper; // اگر از AutoMapper استفاده می‌کردیم

    public GetUserByIdForManagementQueryHandler(IUnitOfWork unitOfWork /*, IMapper mapper*/)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        // _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<UserManagementDetailsDto?> Handle(GetUserByIdForManagementQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.UserRepository.GetUserWithFullDetailsAsync(request.UserId);

        if (user == null)
        {
            // می‌توانیم NotFoundException پرتاب کنیم که توسط Middleware مدیریت خطا گرفته شود:
            // throw new NotFoundException(nameof(User), request.UserId);
            return null; // یا فعلاً null برگردانیم تا Controller آن را مدیریت کند.
        }

        // نگاشت دستی از موجودیت User به UserManagementDetailsDto
        // اگر از AutoMapper استفاده می‌کردیم: return _mapper.Map<UserManagementDetailsDto>(user);
        return new UserManagementDetailsDto
        {
            Id = user.Id,
            SystemUserId = user.SystemUserId,
            FullName = user.FullName,
            NationalCode = user.NationalCode,
            IsActive = user.IsActive,
            RoleId = user.RoleId,
            RoleName = user.Role?.Name ?? string.Empty, // بررسی null برای Role
            ProvinceCode = user.ProvinceCode,
            ProvinceName = user.ProvinceName, // استفاده از فیلد دنرمالایز شده User
            // ProvinceName = user.Province?.Name, // یا استفاده از Navigation Property
            DepartmentCode = user.DepartmentCode,
            DepartmentName = user.DepartmentName, // استفاده از فیلد دنرمالایز شده User
            // DepartmentName = user.Department?.Name, // یا استفاده از Navigation Property
            HotelId = user.HotelId,
            AssignedHotelName = user.AssignedHotel?.Name
        };
    }
}