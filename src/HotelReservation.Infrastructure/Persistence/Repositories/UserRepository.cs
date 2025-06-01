// src/HotelReservation.Infrastructure/Persistence/Repositories/UserRepository.cs
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Infrastructure.Persistence.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<User?> GetBySystemUserIdAsync(string systemUserId)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.SystemUserId == systemUserId);
    }

    public async Task<bool> ExistsBySystemUserIdAsync(string systemUserId)
    {
        return await _dbContext.Users.AnyAsync(u => u.SystemUserId == systemUserId);
    }

    public async Task<User?> GetByNationalCodeAsync(string nationalCode)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.NationalCode == nationalCode);
    }

    public async Task<bool> ExistsByNationalCodeAsync(string nationalCode)
    {
        return await _dbContext.Users.AnyAsync(u => u.NationalCode == nationalCode);
    }

    public async Task<IReadOnlyList<User>> GetUsersByRoleIdAsync(Guid roleId)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.RoleId == roleId)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<User>> GetUsersByProvinceCodeAsync(string provinceCode)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.ProvinceCode == provinceCode)
            .Include(u => u.Province) // برای نمایش نام استان
            .Include(u => u.Department) // و دپارتمان
            .ToListAsync();
    }

    public async Task<IReadOnlyList<User>> GetUsersByDepartmentCodeAsync(string departmentCode)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.DepartmentCode == departmentCode)
            .Include(u => u.Province)
            .Include(u => u.Department)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<User>> GetUsersByHotelIdAsync(Guid hotelId)
    {
        return await _dbContext.Users
           .AsNoTracking()
           .Where(u => u.HotelId == hotelId)
           .Include(u => u.AssignedHotel)
           .ToListAsync();
    }


    public async Task<User?> GetUserWithDependentsAsync(Guid userId)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Include(u => u.Dependents)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User?> GetUserWithFullDetailsAsync(Guid userId)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .Include(u => u.Province)
            .Include(u => u.Department)
            .Include(u => u.AssignedHotel) // هتل ممکن است null باشد
            .Include(u => u.Dependents)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }
      public async Task<User?> GetBySystemUserIdWithDetailsAsync(string systemUserId)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .Include(u => u.Province)
            .Include(u => u.Department)
            .Include(u => u.AssignedHotel)
            // .Include(u => u.Dependents) // وابستگان معمولاً برای Claim لازم نیستند
            .FirstOrDefaultAsync(u => u.SystemUserId == systemUserId);
    }

    // متد GetUserWithFullDetailsAsync(Guid userId) که قبلاً داشتیم:
    
}