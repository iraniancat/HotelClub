// src/HotelReservation.Infrastructure/Persistence/Repositories/BookingRequestRepository.cs
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Infrastructure.Persistence.Repositories;

public class BookingRequestRepository : GenericRepository<BookingRequest>, IBookingRequestRepository
{
    public BookingRequestRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<BookingRequest?> GetBookingRequestWithDetailsAsync(Guid id)
    {
        return await _dbContext.BookingRequests
            .AsNoTracking()
            .Include(br => br.Hotel)
            .Include(br => br.RequestSubmitterUser).ThenInclude(u => u.Role) // برای نمایش نقش کاربر ثبت کننده
            .Include(br => br.Guests)
            .Include(br => br.Files)
            .Include(br => br.StatusHistory).ThenInclude(sh => sh.ChangedByUser) // کاربر تغییر دهنده تاریخچه
            .FirstOrDefaultAsync(br => br.Id == id);
    }

    public async Task<IReadOnlyList<BookingRequest>> GetBookingRequestsByHotelIdAsync(Guid hotelId, BookingStatus? status = null)
    {
        var query = _dbContext.BookingRequests
            .AsNoTracking()
            .Where(br => br.HotelId == hotelId);

        if (status.HasValue)
        {
            query = query.Where(br => br.Status == status.Value);
        }

        return await query.Include(br => br.RequestSubmitterUser).ToListAsync(); // شامل کردن اطلاعات کاربر ثبت کننده
    }
    
    public async Task<IReadOnlyList<BookingRequest>> GetBookingRequestsBySubmitterSystemUserIdAsync(string systemUserId)
    {
        return await _dbContext.BookingRequests
            .AsNoTracking()
            .Where(br => br.RequestSubmitterUser.SystemUserId == systemUserId)
            .Include(br => br.Hotel) // برای نمایش نام هتل
            .ToListAsync();
    }

    public async Task<BookingRequest?> GetByTrackingCodeAsync(string trackingCode)
    {
        return await _dbContext.BookingRequests
            .AsNoTracking()
            .Include(br => br.Hotel)
            .Include(br => br.RequestSubmitterUser)
            .FirstOrDefaultAsync(br => br.TrackingCode == trackingCode);
    }
}