using HotelReservation.Domain.Entities;

namespace HotelReservation.Application.Contracts.Persistence;

public interface IBookingPeriodRepository : IGenericRepository<BookingPeriod>
{
    Task<bool> ExistsByNameAsync(string name);
    // سایر متدهای خاص دوره‌های زمانی در صورت نیاز، اینجا اضافه می‌شوند.
}