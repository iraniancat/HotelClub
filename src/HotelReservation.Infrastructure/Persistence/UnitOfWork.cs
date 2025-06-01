// src/HotelReservation.Infrastructure/Persistence/UnitOfWork.cs
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Domain.Entities;
using HotelReservation.Infrastructure.Persistence.Repositories; // برای دسترسی به پیاده‌سازی‌ها

namespace HotelReservation.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    // از Lazy<T> استفاده می‌کنیم تا Repositoryها فقط در صورت نیاز نمونه‌سازی شوند
    private Lazy<IUserRepository> _userRepository;
    private Lazy<IRoleRepository> _roleRepository;
    private Lazy<IProvinceRepository> _provinceRepository;
    private Lazy<IDepartmentRepository> _departmentRepository;
    private Lazy<IHotelRepository> _hotelRepository;
    private Lazy<IRoomRepository> _roomRepository;
    private Lazy<IBookingRequestRepository> _bookingRequestRepository;
    private Lazy<IDependentDataRepository> _dependentDataRepository; // <<-- اضافه/تایید شود
    private Lazy<IGenericRepository<BookingFile>> _bookingFileRepository;

    public UnitOfWork(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));

        // نمونه‌سازی Lazy برای هر Repository
        _userRepository = new Lazy<IUserRepository>(() => new UserRepository(_context));
        _roleRepository = new Lazy<IRoleRepository>(() => new RoleRepository(_context));
        _provinceRepository = new Lazy<IProvinceRepository>(() => new ProvinceRepository(_context));
        _departmentRepository = new Lazy<IDepartmentRepository>(() => new DepartmentRepository(_context));
        _hotelRepository = new Lazy<IHotelRepository>(() => new HotelRepository(_context));
        _roomRepository = new Lazy<IRoomRepository>(() => new RoomRepository(_context));
        _bookingRequestRepository = new Lazy<IBookingRequestRepository>(() => new BookingRequestRepository(_context));
        _dependentDataRepository = new Lazy<IDependentDataRepository>(() => new DependentDataRepository(_context)); // <<-- اضافه/تایید شود
         _bookingFileRepository = new Lazy<IGenericRepository<BookingFile>>(() => new GenericRepository<BookingFile>(_context));
        // یا _bookingFileRepository = new Lazy<IBookingFileRepository>(() => new BookingFileRepository(_context));
    }

    public IUserRepository UserRepository => _userRepository.Value;
    public IRoleRepository RoleRepository => _roleRepository.Value;
    public IProvinceRepository ProvinceRepository => _provinceRepository.Value;
    public IDepartmentRepository DepartmentRepository => _departmentRepository.Value;
    public IHotelRepository HotelRepository => _hotelRepository.Value;
    public IRoomRepository RoomRepository => _roomRepository.Value;
    public IBookingRequestRepository BookingRequestRepository => _bookingRequestRepository.Value;


    public IDependentDataRepository DependentDataRepository => _dependentDataRepository.Value; 
     public IGenericRepository<BookingFile> BookingFileRepository => _bookingFileRepository.Value;
    // یا public IBookingFileRepository BookingFileRepository => _bookingFileRepository.Value;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose(); // DbContext را Dispose می‌کنیم
        GC.SuppressFinalize(this);
    }
}