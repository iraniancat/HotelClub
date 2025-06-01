// src/HotelReservation.Infrastructure/Persistence/Repositories/ProvinceRepository.cs
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Domain.Entities;
using Microsoft.EntityFrameworkCore; // برای ToListAsync و SingleOrDefaultAsync

namespace HotelReservation.Infrastructure.Persistence.Repositories;

public class ProvinceRepository : GenericRepository<Province>, IProvinceRepository
{
    public ProvinceRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    // GetByCodeAsync با استفاده از متد عمومی از GenericRepository یا پیاده‌سازی اختصاصی
    // public async Task<Province?> GetByCodeAsync(string code)
    // {
    //     return await _dbContext.Provinces.FirstOrDefaultAsync(p => p.Code == code);
    // }
    // یا اگر از متد GetByStringIdAsync در GenericRepository استفاده می‌کنیم، این متد اینجا لازم نیست.
    // فرض می‌کنیم GetByStringIdAsync کافی است.

    public async Task<bool> ExistsByCodeAsync(string code)
    {
        return await _dbContext.Provinces.AnyAsync(p => p.Code == code);
    }

    // DeleteByCodeAsync هم می‌تواند از DeleteByStringIdAsync در GenericRepository استفاده کند.
}