using HotelReservation.Domain.Entities;

namespace HotelReservation.Application.Contracts.Persistence;

public interface IProvinceRepository : IGenericRepository<Province>
{
    // GetByStringIdAsync از IGenericRepository می‌تواند برای GetByCodeAsync استفاده شود.
    // Task<Province?> GetByCodeAsync(string code); // دیگر لازم نیست، از GetByStringIdAsync استفاده می‌شود

    Task<bool> ExistsByCodeAsync(string code);
    // DeleteByStringIdAsync از IGenericRepository می‌تواند برای DeleteByCodeAsync استفاده شود.
}