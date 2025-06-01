// src/HotelReservation.Infrastructure/Persistence/Repositories/DepartmentRepository.cs
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Infrastructure.Persistence.Repositories;

public class DepartmentRepository : GenericRepository<Department>, IDepartmentRepository
{
    public DepartmentRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<bool> ExistsByCodeAsync(string code)
    {
        return await _dbContext.Departments.AnyAsync(d => d.Code == code);
    }
    // سایر پیاده‌سازی‌های خاص در صورت نیاز
}