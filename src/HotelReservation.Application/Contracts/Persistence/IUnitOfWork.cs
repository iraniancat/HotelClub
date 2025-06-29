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
    IGenericRepository<BookingFile> BookingFileRepository { get; }
    IBookingPeriodRepository BookingPeriodRepository { get; } // <<-- اضافه شد
    IProvinceHotelQuotaRepository ProvinceHotelQuotaRepository { get; }
    IGenericRepository<BookingStatusHistory> BookingStatusHistoryRepository { get; }


    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);


}