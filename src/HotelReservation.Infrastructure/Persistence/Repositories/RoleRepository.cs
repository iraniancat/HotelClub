// src/HotelReservation.Infrastructure/Persistence/Repositories/RoleRepository.cs
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Infrastructure.Persistence.Repositories;

public class RoleRepository : GenericRepository<Role>, IRoleRepository
{
    public RoleRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Role?> GetByNameAsync(string roleName)
    {
        return await _dbContext.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == roleName);
    }

    public async Task<bool> ExistsByNameAsync(string roleName)
    {
        return await _dbContext.Roles.AnyAsync(r => r.Name == roleName);
    }
}