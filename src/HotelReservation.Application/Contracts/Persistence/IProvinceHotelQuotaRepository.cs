// 1. فایل جدید: src/HotelReservation.Application/Contracts/Persistence/IProvinceHotelQuotaRepository.cs
//    این اینترفیس، قرارداد مربوط به ریپازیتوری محدودیت‌های استان-هتل را تعریف می‌کند.
using HotelReservation.Domain.Entities;

namespace HotelReservation.Application.Contracts.Persistence;

public interface IProvinceHotelQuotaRepository : IGenericRepository<ProvinceHotelQuota>
{
    // در آینده می‌توانید متدهای خاص این ریپازیتوری را اینجا اضافه کنید.
   // Task<bool> ExistsAsync(Guid hotelId);
}