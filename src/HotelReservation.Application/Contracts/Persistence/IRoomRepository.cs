// src/HotelReservation.Application/Contracts/Persistence/IRoomRepository.cs
using HotelReservation.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelReservation.Application.Contracts.Persistence;

public interface IRoomRepository : IGenericRepository<Room>
{
    // Task<IReadOnlyList<Room>> GetAvailableRoomsAsync(Guid hotelId, DateTime checkIn, DateTime checkOut, int requiredCapacity);
    // متدهای خاص اتاق می‌توانند اینجا اضافه شوند، اما فعلاً GenericRepository کافی به نظر می‌رسد.
}