using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Domain.Entities;

public interface IUnitOfWork : IDisposable
{
    IUserRepository UserRepository { get; }
    IRoleRepository RoleRepository { get; }
    IProvinceRepository ProvinceRepository { get; }
    IDepartmentRepository DepartmentRepository { get; }
    IHotelRepository HotelRepository { get; }
    IRoomRepository RoomRepository { get; }
    IBookingRequestRepository BookingRequestRepository { get; }
    IDependentDataRepository DependentDataRepository { get; }
    IGenericRepository<BookingFile> BookingFileRepository { get; } // <<-- اضافه شد (استفاده از Generic)
    // یا اگر IBookingFileRepository اختصاصی ساختید:

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    
}