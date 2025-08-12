//    این DTO برای دریافت دلیل رد از UI استفاده می‌شود.
using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Application.DTOs.Booking;

public class InitialRejectDto
{
    [Required(ErrorMessage = "دلیل رد درخواست الزامی است.")]
    [MaxLength(500, ErrorMessage = "دلیل رد نمی‌تواند بیشتر از ۵۰۰ کاراکتر باشد.")]
    public string RejectionReason { get; set; } = string.Empty;
}