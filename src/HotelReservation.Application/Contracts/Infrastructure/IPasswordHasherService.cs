// src/HotelReservation.Application/Contracts/Infrastructure/IPasswordHasherService.cs
namespace HotelReservation.Application.Contracts.Infrastructure;

public interface IPasswordHasherService
{
    string HashPassword(string password);
    bool VerifyPassword(string hashedPassword, string providedPassword);
}