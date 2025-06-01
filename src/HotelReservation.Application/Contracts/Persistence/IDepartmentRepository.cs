using HotelReservation.Domain.Entities;

namespace HotelReservation.Application.Contracts.Persistence;

public interface IDepartmentRepository : IGenericRepository<Department>
{
    // GetByStringIdAsync از IGenericRepository می‌تواند برای GetByCodeAsync استفاده شود.
    Task<bool> ExistsByCodeAsync(string code);
    // سایر متدهای خاص دپارتمان
}