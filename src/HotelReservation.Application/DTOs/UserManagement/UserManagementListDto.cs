// src/HotelReservation.Application/DTOs/UserManagement/UserManagementListDto.cs
namespace HotelReservation.Application.DTOs.UserManagement;

public class UserManagementListDto
{
    public Guid Id { get; set; }
    public string SystemUserId { get; set; }
    public string FullName { get; set; }
    public string? NationalCode { get; set; }
    public bool IsActive { get; set; }
    public string RoleName { get; set; } // نام نقش
    public string? ProvinceName { get; set; }
    public string? DepartmentName { get; set; }
    public string? AssignedHotelName { get; set; } // نام هتل تخصیص داده شده (اگر کاربر هتل باشد)
}