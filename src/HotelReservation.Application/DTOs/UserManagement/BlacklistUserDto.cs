
// 1. فایل جدید: src/HotelReservation.Application/DTOs/UserManagement/BlacklistUserDto.cs
// این DTO اطلاعات لازم برای به‌روزرسانی وضعیت لیست سیاه را از UI دریافت می‌کند.
using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Application.DTOs.UserManagement;

public class BlacklistUserDto
{
    [Required]
    public bool IsBlacklisted { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; } // اگر IsBlacklisted=true باشد، این فیلد الزامی می‌شود.

    public DateTime? EndDate { get; set; } // تاریخ پایان محدودیت (می‌تواند null باشد)
}