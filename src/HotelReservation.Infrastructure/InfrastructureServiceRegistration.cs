
using HotelReservation.Application.Contracts.Infrastructure;
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Infrastructure.Persistence;
using HotelReservation.Infrastructure.Persistence.Repositories;
using HotelReservation.Infrastructure.Services;
using Microsoft.EntityFrameworkCore; // برای AddDbContext
using Microsoft.Extensions.Configuration; // برای IConfiguration
using Microsoft.Extensions.DependencyInjection; // برای IServiceCollection
using HotelReservation.Application.Contracts.Infrastructure; // برای ISmsService
using HotelReservation.Infrastructure.Services;
using HotelReservation.Infrastructure.Authentication; // برای SmsService

namespace HotelReservation.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // ثبت DbContext (این بخش را از Program.cs به اینجا منتقل می‌کنیم)
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        // ثبت UnitOfWork و Repositoryها
        // IUnitOfWork به عنوان Scoped ثبت می‌شود چون DbContext هم معمولاً Scoped است.
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddTransient<IPasswordHasherService, PasswordHasherService>(); // یا AddTransient
                                                                             // ...
        services.AddTransient<ISmsService, SmsService>(); // <<-- ثبت سرویس SMS

        // ثبت سرویس تولید توکن JWT
        services.AddScoped<IJwtTokenGenerator, JwtTokenGeneratorService>(); // <<-- اضافه شد

        services.AddScoped<IFileStorageService, FileSystemStorageService>();
        
         services.AddScoped<IBookingRequestRepository, BookingRequestRepository>();
         services.AddScoped<IProvinceHotelQuotaRepository, ProvinceHotelQuotaRepository>();
        // Repositoryهای اختصاصی هم به عنوان Scoped ثبت می‌شوند
        // (اگر UnitOfWork آن‌ها را نمونه‌سازی می‌کند، شاید نیازی به ثبت تک تک آن‌ها نباشد،
        // اما برای صراحت یا اگر مستقیماً تزریق می‌شوند، ثبتشان می‌کنیم)
        // با پیاده‌سازی فعلی UnitOfWork که Repositoryها را new می‌کند،
        // فقط IUnitOfWork را ثبت می‌کنیم کافی است.
        // اگر بخواهیم هر Repository را جداگانه تزریق کنیم:
        // services.AddScoped<IUserRepository, UserRepository>();
        // services.AddScoped<IRoleRepository, RoleRepository>();
        // services.AddScoped<IProvinceRepository, ProvinceRepository>();
        // services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        // services.AddScoped<IHotelRepository, HotelRepository>();
        // services.AddScoped<IBookingRequestRepository, BookingRequestRepository>();

        // اگر از Generic Repository به صورت مستقیم هم استفاده می‌کنید، می‌توانید آن را هم ثبت کنید:
        // services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        // اما معمولاً از طریق UnitOfWork یا Repositoryهای اختصاصی به آن‌ها دسترسی پیدا می‌کنیم.

        // سایر سرویس‌های Infrastructure (مانند EmailService, SmsService و ...) در اینجا ثبت می‌شوند.

        return services;
    }
}
