// src/HotelReservation.Application/DTOs/UserManagement/UserManagementDetailsDto.cs
namespace HotelReservation.Application.DTOs.UserManagement;

public class UserManagementDetailsDto
{
    public Guid Id { get; set; }
    public string SystemUserId { get; set; }
    public string FullName { get; set; }
    public string? NationalCode { get; set; }
    public bool IsActive { get; set; }

    public Guid RoleId { get; set; } // شناسه نقش برای Dropdown ویرایش
    public string RoleName { get; set; }

    public string? ProvinceCode { get; set; } // شناسه استان برای Dropdown ویرایش
    public string? ProvinceName { get; set; }

    public string? DepartmentCode { get; set; } // شناسه دپارتمان برای Dropdown ویرایش
    public string? DepartmentName { get; set; }

    public Guid? HotelId { get; set; } // شناسه هتل برای Dropdown ویرایش
    public string? AssignedHotelName { get; set; }

    // در آینده می‌توان لیست وابستگان یا سایر اطلاعات مرتبط را نیز اضافه کرد
    // public List<DependentSlimDto> Dependents { get; set; } = new List<DependentSlimDto>();
}