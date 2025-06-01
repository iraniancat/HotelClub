// src/HotelReservation.Application/Features/UserManagement/Commands/CreateNonEmployeeUser/CreateNonEmployeeUserCommand.cs
using MediatR;
using System;

namespace HotelReservation.Application.Features.UserManagement.Commands.CreateNonEmployeeUser;

public class CreateNonEmployeeUserCommand : IRequest<Guid> // شناسه کاربر جدید را باز می‌گرداند
{
    public string SystemUserId { get; set; }
    public string FullName { get; set; }
    public string Password { get; set; } // رمز عبور خام برای هش شدن در Handler
    public Guid RoleId { get; set; }
    public bool IsActive { get; set; }
    public string? NationalCode { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ProvinceCode { get; set; }
    public string? ProvinceName { get; set; }
    public string? DepartmentCode { get; set; }
    public string? DepartmentName { get; set; }
    public Guid? HotelId { get; set; }

    // سازنده برای پر کردن راحت‌تر Command از DTO
    public CreateNonEmployeeUserCommand(
        string systemUserId, string fullName, string password, Guid roleId, bool isActive,
        string? nationalCode,string? phoneNumber,  string? provinceCode, string? provinceName,
        string? departmentCode, string? departmentName, Guid? hotelId)
    {
        SystemUserId = systemUserId;
        FullName = fullName;
        Password = password;
        RoleId = roleId;
        IsActive = isActive;
        NationalCode = nationalCode;
        PhoneNumber = phoneNumber;
        ProvinceCode = provinceCode;
        ProvinceName = provinceName;
        DepartmentCode = departmentCode;
        DepartmentName = departmentName;
        HotelId = hotelId;
    }
}