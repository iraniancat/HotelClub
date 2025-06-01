// src/HotelReservation.Infrastructure/Services/PasswordHasherService.cs
using HotelReservation.Application.Contracts.Infrastructure;
using System.Security.Cryptography; // برای SHA256
using System.Text; // برای Encoding

namespace HotelReservation.Infrastructure.Services;

public class PasswordHasherService : IPasswordHasherService
{
    // هشدار: این یک پیاده‌سازی بسیار ساده و ناامن برای هش کردن است.
    // در محیط واقعی باید از کتابخانه‌هایی مانند BCrypt.Net یا ASP.NET Core Identity Password Hasher استفاده شود.
    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password)) return string.Empty;

        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            // تبدیل بایت‌ها به رشته هگزادسیمال
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLowerInvariant();
        }
    }

    public bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        if (string.IsNullOrEmpty(hashedPassword) || string.IsNullOrEmpty(providedPassword)) return false;
        
        var hashOfProvidedPassword = HashPassword(providedPassword);
        return StringComparer.OrdinalIgnoreCase.Compare(hashedPassword, hashOfProvidedPassword) == 0;
    }
}