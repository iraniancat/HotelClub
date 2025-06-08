using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Application.DTOs.Booking;

public class UpdateBookingPeriodDto
{
    [Required(ErrorMessage = "نام دوره زمانی الزامی است.")]
    [MaxLength(150)]
    public string Name { get; set; }

    [Required(ErrorMessage = "تاریخ شروع الزامی است.")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "تاریخ پایان الزامی است.")]
    public DateTime EndDate { get; set; }

    [Required(ErrorMessage = "وضعیت فعالیت الزامی است.")]
    public bool IsActive { get; set; }
}