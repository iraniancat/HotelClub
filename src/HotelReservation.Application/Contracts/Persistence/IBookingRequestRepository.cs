using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Enums; // برای BookingStatus

namespace HotelReservation.Application.Contracts.Persistence;

public interface IBookingRequestRepository : IGenericRepository<BookingRequest>
{
    Task<BookingRequest?> GetBookingRequestWithDetailsAsync(Guid id, bool asNoTracking = true); 
    Task<IReadOnlyList<BookingRequest>> GetBookingRequestsByHotelIdAsync(Guid hotelId, BookingStatus? status = null);
    Task<IReadOnlyList<BookingRequest>> GetBookingRequestsBySubmitterSystemUserIdAsync(string systemUserId); // تغییر از Guid به systemUserId
    Task<BookingRequest?> GetByTrackingCodeAsync(string trackingCode);
}