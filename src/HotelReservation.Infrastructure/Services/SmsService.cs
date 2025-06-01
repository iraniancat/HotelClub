// src/HotelReservation.Infrastructure/Services/SmsService.cs
using HotelReservation.Application.Contracts.Infrastructure;
using Microsoft.Extensions.Logging; // برای لاگ کردن
using System.Threading.Tasks;

namespace HotelReservation.Infrastructure.Services;

public class SmsService : ISmsService
{
    private readonly ILogger<SmsService> _logger;

    public SmsService(ILogger<SmsService> logger)
    {
        _logger = logger;
    }

    public Task SendSmsAsync(string phoneNumber, string message)
    {
        // در یک پروژه واقعی، اینجا کد اتصال به پنل SMS و ارسال پیامک قرار می‌گیرد.
        // فعلاً فقط در لاگ می‌نویسیم.
        _logger.LogInformation($"SMS Sent to {phoneNumber}: \"{message}\"");
        return Task.CompletedTask;
    }

    // پیاده‌سازی سایر متدهای ISmsService در صورت وجود
}