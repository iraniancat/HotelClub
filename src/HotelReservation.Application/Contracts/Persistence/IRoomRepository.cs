using HotelReservation.Domain.Entities;

namespace HotelReservation.Application.Contracts.Persistence;
public interface IRoomRepository : IGenericRepository<Room>
{
    Task<bool> IsRoomNumberUniqueAsync(Guid hotelId, string roomNumber, Guid? roomIdToExclude = null);
}