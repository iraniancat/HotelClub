// src/HotelReservation.Application/Features/UserManagement/Commands/CreateNonEmployeeUser/CreateNonEmployeeUserCommandHandler.cs
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Application.Contracts.Infrastructure; // برای IPasswordHasherService
using HotelReservation.Application.Exceptions;
using HotelReservation.Domain.Entities;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HotelReservation.Application.Features.UserManagement.Commands.CreateNonEmployeeUser;

public class CreateNonEmployeeUserCommandHandler : IRequestHandler<CreateNonEmployeeUserCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasherService _passwordHasher;
    private const string HotelUserRoleName = "HotelUser"; // باید با نام واقعی نقش در پایگاه داده مطابقت داشته باشد

    public CreateNonEmployeeUserCommandHandler(IUnitOfWork unitOfWork, IPasswordHasherService passwordHasher)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    }

    public async Task<Guid> Handle(CreateNonEmployeeUserCommand request, CancellationToken cancellationToken)
    {
        // ۱. بررسی یکتا بودن SystemUserId
        if (await _unitOfWork.UserRepository.ExistsBySystemUserIdAsync(request.SystemUserId))
        {
            throw new BadRequestException($"کاربری با شناسه کاربری سیستم '{request.SystemUserId}' قبلاً وجود دارد.");
        }

        // ۲. بررسی یکتا بودن NationalCode (اگر ارائه شده و باید یکتا باشد)
        if (!string.IsNullOrEmpty(request.NationalCode) && await _unitOfWork.UserRepository.ExistsByNationalCodeAsync(request.NationalCode))
        {
            throw new BadRequestException($"کاربری با کد ملی '{request.NationalCode}' قبلاً وجود دارد.");
        }

        // ۳. واکشی و اعتبارسنجی Role, Province, Department, Hotel
        var role = await _unitOfWork.RoleRepository.GetByIdAsync(request.RoleId);
        if (role == null) throw new BadRequestException($"نقش با شناسه '{request.RoleId}' یافت نشد.");

        Province? province = null;
        if (!string.IsNullOrEmpty(request.ProvinceCode))
        {
            province = await _unitOfWork.ProvinceRepository.GetByStringIdAsync(request.ProvinceCode);
            if (province == null) throw new BadRequestException($"استان با کد '{request.ProvinceCode}' یافت نشد.");
        }

        Department? department = null;
        if (!string.IsNullOrEmpty(request.DepartmentCode))
        {
            department = await _unitOfWork.DepartmentRepository.GetByStringIdAsync(request.DepartmentCode);
            if (department == null) throw new BadRequestException($"اداره/شعبه با کد '{request.DepartmentCode}' یافت نشد.");
        }
        
        Hotel? hotel = null;
        if (role.Name == HotelUserRoleName)
        {
            if (!request.HotelId.HasValue || request.HotelId.Value == Guid.Empty)
            {
                throw new BadRequestException($"برای نقش '{HotelUserRoleName}'، شناسه هتل الزامی است.");
            }
            hotel = await _unitOfWork.HotelRepository.GetByIdAsync(request.HotelId.Value);
            if (hotel == null) throw new BadRequestException($"هتل با شناسه '{request.HotelId.Value}' یافت نشد.");
        } else if (request.HotelId.HasValue) {
             // اگر نقش کاربر هتل نیست اما شناسه هتل ارسال شده، می‌توان خطا داد یا آن را نادیده گرفت
             // فعلا نادیده می‌گیریم و در سازنده User مدیریت می‌شود که HotelId فقط برای نقش HotelUser ست شود.
        }


        // ۴. هش کردن رمز عبور
        var hashedPassword = _passwordHasher.HashPassword(request.Password);

        // ۵. ایجاد موجودیت User
        var newUser = new User(
            request.SystemUserId,
            request.FullName,
            hashedPassword,
            request.RoleId,
            role, // شیء Role واکشی شده
            request.IsActive,
            request.NationalCode,
            request.PhoneNumber,
            request.ProvinceCode,
            province, // شیء Province واکشی شده
            request.ProvinceName ?? province?.Name, // استفاده از نام ارسالی یا نام واکشی شده
            request.DepartmentCode,
            department, // شیء Department واکشی شده
            request.DepartmentName ?? department?.Name, // مشابه استان
            (role.Name == HotelUserRoleName) ? request.HotelId : null,
            (role.Name == HotelUserRoleName) ? hotel : null
        );

        // ۶. افزودن به Repository و ذخیره تغییرات
        await _unitOfWork.UserRepository.AddAsync(newUser);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return newUser.Id;
    }
}