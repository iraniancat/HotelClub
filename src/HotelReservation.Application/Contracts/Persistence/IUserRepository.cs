using HotelReservation.Domain.Entities;

namespace HotelReservation.Application.Contracts.Persistence;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetBySystemUserIdAsync(string systemUserId); // قبلاً EmployeeId بود
    Task<bool> ExistsBySystemUserIdAsync(string systemUserId);
    Task<User?> GetByNationalCodeAsync(string nationalCode);
    Task<bool> ExistsByNationalCodeAsync(string nationalCode);
    Task<IReadOnlyList<User>> GetUsersByRoleIdAsync(Guid roleId);
    Task<IReadOnlyList<User>> GetUsersByProvinceCodeAsync(string provinceCode);
    Task<IReadOnlyList<User>> GetUsersByDepartmentCodeAsync(string departmentCode);
    Task<IReadOnlyList<User>> GetUsersByHotelIdAsync(Guid hotelId); // برای کاربران هتل
    Task<User?> GetUserWithDependentsAsync(Guid userId);
    Task<User?> GetUserWithFullDetailsAsync(Guid userId); // با نقش، استان، دپارتمان، هتل (در صورت وجود)
    Task<User?> GetBySystemUserIdWithDetailsAsync(string systemUserId); // <<-- متد جدید
}