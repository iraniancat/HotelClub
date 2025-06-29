// 2. فایل جدید: src/HotelReservation.Infrastructure/Persistence/Repositories/ProvinceHotelQuotaRepository.cs
//    این کلاس، IProvinceHotelQuotaRepository را با استفاده از EF Core پیاده‌سازی می‌کند.
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Domain.Entities;

namespace HotelReservation.Infrastructure.Persistence.Repositories;

public class ProvinceHotelQuotaRepository : GenericRepository<ProvinceHotelQuota>, IProvinceHotelQuotaRepository
{
    public ProvinceHotelQuotaRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

  
}