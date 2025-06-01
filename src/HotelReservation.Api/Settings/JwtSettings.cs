// src/HotelReservation.Api/Settings/JwtSettings.cs
namespace HotelReservation.Api.Settings;

public class JwtSettings
{
    public const string SectionName = "JwtSettings"; // نام بخش در appsettings.json

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public int DurationInMinutes { get; set; }
}