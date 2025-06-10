using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Application.DTOs.Room;

public class CreateOrUpdateRoomDto
{
    [Required(ErrorMessage = "شماره اتاق الزامی است.")]
    [MaxLength(20)]
    public string RoomNumber { get; set; }

    [Required(ErrorMessage = "ظرفیت الزامی است.")]
    [Range(1, 10, ErrorMessage = "ظرفیت باید بین ۱ تا ۱۰ نفر باشد.")]
    public int Capacity { get; set; }

    [Required(ErrorMessage = "قیمت هر شب الزامی است.")]
    [Range(1, double.MaxValue, ErrorMessage = "قیمت باید یک مقدار مثبت باشد.")]
    public decimal PricePerNight { get; set; }

    // این فیلد فقط برای ایجاد جدید استفاده می‌شود و در ویرایش نادیده گرفته می‌شود
    public Guid? HotelId { get; set; }
    
    public bool IsActive { get; set; } = true;
}