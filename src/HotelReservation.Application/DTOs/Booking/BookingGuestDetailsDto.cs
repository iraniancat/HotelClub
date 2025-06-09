// src/HotelReservation.Application/DTOs/Booking/BookingGuestDetailsDto.cs
namespace HotelReservation.Application.DTOs.Booking;

public class BookingGuestDetailsDto // DTO برای نمایش مهمان در پاسخ
{
    public Guid Id { get; set; }
    public string FullName { get; set; }
    public string NationalCode { get; set; }
    public string RelationshipToEmployee { get; set; }
    public decimal DiscountPercentage { get; set; }
}
