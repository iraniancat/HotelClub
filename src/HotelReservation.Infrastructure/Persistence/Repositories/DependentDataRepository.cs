// src/HotelReservation.Infrastructure/Persistence/Repositories/DependentDataRepository.cs
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelReservation.Infrastructure.Persistence.Repositories;

public class DependentDataRepository : GenericRepository<DependentData>, IDependentDataRepository
{
    public DependentDataRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<DependentData>> GetByEmployeeDataIdAsync(Guid userId) // نام پارامتر به userId تغییر یافت
    {
        return await _dbContext.DependentData
            .AsNoTracking()
            .Where(d => d.UserId == userId)
            .ToListAsync();
    }

    public async Task<DependentData?> GetByEmployeeDataIdAndNationalCodeAsync(Guid userId, string dependentNationalCode) // نام پارامتر به userId تغییر یافت
    {
        return await _dbContext.DependentData
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.UserId == userId && d.NationalCode == dependentNationalCode);
    }
}