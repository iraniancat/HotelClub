using HotelReservation.Domain.Entities;

namespace HotelReservation.Application.Contracts.Persistence;

public interface IHotelRepository : IGenericRepository<Hotel>
{
    Task<Hotel?> GetHotelWithDetailsAsync(Guid id); // مثلاً با اتاق‌ها
    // << حذف شد: Task<IReadOnlyList<Hotel>> GetHotelsByProvinceCodeAsync(string provinceCode); >>
    // می‌توان متدهای جستجوی دیگری اضافه کرد، مثلاً بر اساس نام یا بخشی از آدرس
    Task<IReadOnlyList<Hotel>> FindByNameAsync(string name);
    Task<bool> ExistsAsync(Guid hotelId);
}