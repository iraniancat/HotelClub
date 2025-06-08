// tests/HotelReservation.Api.IntegrationTests/CustomWebApplicationFactory.cs
using HotelReservation.Infrastructure.Persistence; // برای AppDbContext
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions; // برای TryRemove
using System;
using System.Linq;
// using HotelReservation.Application.Contracts.Infrastructure; // اگر می‌خواهید سرویس‌ها را mock کنید
// using Moq; // اگر از Moq استفاده می‌کنید

namespace HotelReservation.Api.IntegrationTests;

// TEntryPoint معمولاً کلاس Program از پروژه Api شماست
public class CustomWebApplicationFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint> where TEntryPoint : class
{
   protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // ۱. حذف رجیستری AppDbContext قبلی
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }
            // (اختیاری) حذف DbContext Pool اگر از AddDbContextPool استفاده شده بود
            var dbContextPoolDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextPool<AppDbContext>));
            if (dbContextPoolDescriptor != null)
            {
                services.Remove(dbContextPoolDescriptor);
            }


            // ۲. افزودن AppDbContext با استفاده از پایگاه داده InMemory
            var dbName = $"HotelReservationTestDb_{Guid.NewGuid()}";
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(dbName);
            });

            // ۳. (اختیاری) جایگزینی سایر سرویس‌ها با Mockها (همانند قبل)

            // ۴. تنظیم Authentication برای تست‌ها با اولویت دادن به TestAuthHandler
            // ابتدا سعی می‌کنیم رجیستری‌های قبلی Authentication را پاک کنیم (اگرچه همیشه لازم نیست)
            // services.RemoveAll<IAuthenticationService>(); 
            // services.RemoveAll<AuthenticationSchemeOptions>(); // این ممکن است بیش از حد باشد

            services.AddAuthentication(options => // <<-- تنظیم صریح schémaهای پیش‌فرض
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.AuthenticationScheme;
                options.DefaultChallengeScheme = TestAuthHandler.AuthenticationScheme;
                options.DefaultScheme = TestAuthHandler.AuthenticationScheme; // این مهم است
            })
            .AddScheme<TestAuthHandlerOptions, TestAuthHandler>(
                TestAuthHandler.AuthenticationScheme, options => { });

            // اطمینان از اینکه سرویس‌های Authorization وجود دارند
            // (معمولاً Program.cs اصلی این کار را انجام می‌دهد، اما برای اطمینان)
            services.AddAuthorization(options =>
            {
                // اگر Policyهای خاصی در Program.cs تعریف کرده‌اید که در تست‌ها هم لازمند،
                // باید مطمئن شوید که در اینجا هم قابل دسترس هستند یا دوباره تعریف شوند.
                // برای [Authorize(Roles="...")]، تعریف پایه کافی است.
                // مثال: options.AddPolicy("CanViewBookingRequest", policy => policy.Requirements.Add(new ViewBookingRequestRequirement()));
                // این Policy ها باید توسط AddApplicationServices یا AddInfrastructureServices از برنامه اصلی اضافه شده باشند.
            });
        });
        builder.UseEnvironment("Test"); // یا "Development" اگر تنظیمات خاصی برای محیط تست ندارید
    }
}