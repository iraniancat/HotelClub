namespace HotelReservation.Application.DTOs.Room;

public class RoomDto
{
    public Guid Id { get; set; }
    public string RoomNumber { get; set; }
    public int Capacity { get; set; }
    public decimal PricePerNight { get; set; }
    public Guid HotelId { get; set; }
    public string? HotelName { get; set; }
    public bool IsActive { get; set; }
}