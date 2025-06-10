using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Infrastructure.Persistence.Repositories;
public class RoomRepository : GenericRepository<Room>, IRoomRepository
{
    public RoomRepository(AppDbContext dbContext) : base(dbContext) { }

    public async Task<bool> IsRoomNumberUniqueAsync(Guid hotelId, string roomNumber, Guid? roomIdToExclude = null)
    {
        var query = _dbContext.Rooms
            .Where(r => r.HotelId == hotelId && r.RoomNumber.ToLower() == roomNumber.ToLower());
        
        if (roomIdToExclude.HasValue)
        {
            query = query.Where(r => r.Id != roomIdToExclude.Value);
        }

        return !await query.AnyAsync();
    }
}