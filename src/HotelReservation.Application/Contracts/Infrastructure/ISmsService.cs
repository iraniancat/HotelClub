// src/HotelReservation.Application/Contracts/Infrastructure/ISmsService.cs
using System.Threading.Tasks;

namespace HotelReservation.Application.Contracts.Infrastructure;

public interface ISmsService
{
    Task SendSmsAsync(string phoneNumber, string message);
    // می‌توان متدهای خاص‌تری هم اضافه کرد، مانند:
    // Task SendBookingConfirmationSmsAsync(string employeePhoneNumber, string trackingCode, DateTime checkInDate, string hotelName);
}