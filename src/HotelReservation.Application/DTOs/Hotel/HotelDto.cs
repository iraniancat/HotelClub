// src/HotelReservation.Application/DTOs/Hotel/HotelDto.cs
namespace HotelReservation.Application.DTOs.Hotel;

public class HotelDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string? ContactPerson { get; set; }
    public string? PhoneNumber { get; set; }
    // در آینده می‌توانیم لیست اتاق‌ها یا اطلاعات دیگری را هم به این DTO اضافه کنیم
    // public List<RoomDto> Rooms { get; set; } = new List<RoomDto>();
}