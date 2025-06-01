using HotelReservation.Domain.Entities;

namespace HotelReservation.Application.Contracts.Persistence;

public interface IRoleRepository : IGenericRepository<Role>
{
    Task<Role?> GetByNameAsync(string roleName);
    Task<bool> ExistsByNameAsync(string roleName);
    // سایر متدهای خاص نقش
}