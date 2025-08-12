namespace HotelReservation.Infrastructure.Services.Sms;

using HotelReservation.Application.Contracts.Infrastructure;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

public class FakeSmsService : ISmsService
{
    private readonly ILogger<FakeSmsService> _logger;

    public FakeSmsService(ILogger<FakeSmsService> logger)
    {
        _logger = logger;
    }

    public Task SendSmsAsync(string mobileNumber, string message)
    {
        // به جای ارسال واقعی، پیام را در کنسول لاگ می‌کنیم
        _logger.LogInformation("--- FAKE SMS SERVICE ---");
        _logger.LogInformation("To: {MobileNumber}", mobileNumber);
        _logger.LogInformation("Message: {Message}", message);
        _logger.LogInformation("------------------------");
        
        return Task.CompletedTask;
    }
}