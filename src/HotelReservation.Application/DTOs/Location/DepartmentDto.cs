// src/HotelReservation.Application/DTOs/Location/DepartmentDto.cs
namespace HotelReservation.Application.DTOs.Location;

public class DepartmentDto
{
    public string Code { get; set; } // کلید اصلی
    public string Name { get; set; }
    // می‌توان فیلدهای دیگری مانند ProvinceCode یا ProvinceName را هم اضافه کرد اگر لازم باشد
    // که این دپارتمان متعلق به کدام استان است (اگر چنین رابطه‌ای وجود دارد و توسط Job پر می‌شود).
    // فعلاً ساده نگه می‌داریم.
}