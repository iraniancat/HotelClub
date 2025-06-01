// src/HotelReservation.Infrastructure/Persistence/Repositories/RoomRepository.cs
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Domain.Entities;
using Microsoft.EntityFrameworkCore; // برای استفاده از متدهای EF خاص اگر لازم باشد
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelReservation.Infrastructure.Persistence.Repositories;

public class RoomRepository : GenericRepository<Room>, IRoomRepository
{
    public RoomRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    // پیاده‌سازی متدهای خاص IRoomRepository در صورت وجود
    // public async Task<IReadOnlyList<Room>> GetAvailableRoomsAsync(Guid hotelId, DateTime checkIn, DateTime checkOut, int requiredCapacity)
    // {
    //     // منطق پیچیده‌تر برای یافتن اتاق‌های کاملاً آزاد در بازه زمانی با ظرفیت مشخص
    // }
}