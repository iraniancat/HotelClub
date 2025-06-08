using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Infrastructure.Persistence.Repositories;

public class BookingPeriodRepository : GenericRepository<BookingPeriod>, IBookingPeriodRepository
{
    public BookingPeriodRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _dbContext.BookingPeriods
            .AsNoTracking()
            .AnyAsync(p => p.Name.ToLower() == name.ToLower());
    }
}
