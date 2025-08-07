namespace HotelReservation.Application.DTOs.Booking;

public class BookingHistoryDto
{
    public string TrackingCode { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public string Status { get; set; }
}