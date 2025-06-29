using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Application.DTOs.Quota;

public class SetQuotaDto
{
    [Required]
    public Guid HotelId { get; set; }
    [Required]
    public string ProvinceCode { get; set; }
    [Range(0, 1000, ErrorMessage = "تعداد اتاق باید بین ۰ تا ۱۰۰۰ باشد.")]
    public int RoomLimit { get; set; }
}