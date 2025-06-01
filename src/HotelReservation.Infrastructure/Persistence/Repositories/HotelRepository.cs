// src/HotelReservation.Infrastructure/Persistence/Repositories/HotelRepository.cs
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Infrastructure.Persistence.Repositories;

public class HotelRepository : GenericRepository<Hotel>, IHotelRepository
{
    public HotelRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Hotel?> GetHotelWithDetailsAsync(Guid id)
    {
        return await _dbContext.Hotels
            .AsNoTracking()
            .Include(h => h.Rooms) // به عنوان مثال، اتاق‌ها را هم بارگذاری می‌کنیم
            // .Include(h => h.Province) // این دیگر وجود ندارد
            .FirstOrDefaultAsync(h => h.Id == id);
    }

    public async Task<IReadOnlyList<Hotel>> FindByNameAsync(string name)
    {
        return await _dbContext.Hotels
            .AsNoTracking()
            .Where(h => h.Name.Contains(name)) // جستجوی ساده بر اساس نام
            .ToListAsync();
    }
}