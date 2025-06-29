namespace HotelReservation.Application.DTOs.Quota;

public class ProvinceHotelQuotaDto
{
    public Guid Id { get; set; }
    public string ProvinceCode { get; set; }
    public string ProvinceName { get; set; }
    public Guid HotelId { get; set; }
    public string HotelName { get; set; }
    public int RoomLimit { get; set; }
}