// src/HotelReservation.Application/Features/UserManagement/Commands/UpdateUser/UpdateUserCommand.cs
using MediatR;
using System;

namespace HotelReservation.Application.Features.UserManagement.Commands.UpdateUser;

public class UpdateUserCommand : IRequest // یا IRequest<Unit>
{
    public Guid UserId { get; set; } // شناسه کاربری که باید به‌روز شود (از مسیر URL)
    
    // فیلدهایی که از UpdateUserDto می‌آیند
    public string FullName { get; set; }
    public bool IsActive { get; set; }
    public Guid RoleId { get; set; }
    public string? NationalCode { get; set; }
     public string? PhoneNumber { get; set; } // <<-- اضافه شد
    public string? ProvinceCode { get; set; }
    // ProvinceName در Command لازم نیست، از ProvinceCode خوانده می‌شود
    public string? DepartmentCode { get; set; }
    // DepartmentName در Command لازم نیست
    public Guid? HotelId { get; set; }
}