// src/HotelReservation.Infrastructure/Persistence/Repositories/BookingRequestRepository.cs
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelReservation.Infrastructure.Persistence.Repositories;

public class BookingRequestRepository : GenericRepository<BookingRequest>, IBookingRequestRepository
{
    private readonly ILogger<BookingRequestRepository> _logger; // <<-- اضافه شد

    // سازنده اصلاح شده برای تزریق لاگر
    public BookingRequestRepository(AppDbContext dbContext, ILogger<BookingRequestRepository> logger) : base(dbContext)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<BookingRequest?> GetBookingRequestWithDetailsAsync(Guid id, bool asNoTracking = true)
    {
        _logger.LogInformation("Repository: Attempting to find BookingRequest with ID: {BookingRequestId} (AsNoTracking: {AsNoTracking})", id, asNoTracking);

        IQueryable<BookingRequest> query = _dbContext.BookingRequests;

        if (asNoTracking)
        {
            query = query.AsNoTracking(); // <<-- فقط در صورت نیاز، بدون ردیابی می‌خوانیم
        }

        var bookingRequest = await query
            .Include(br => br.Hotel)
            .Include(br => br.RequestSubmitterUser).ThenInclude(u => u.Role)
            .Include(br => br.Guests)
            .Include(br => br.Files)
            .Include(br => br.StatusHistory).ThenInclude(sh => sh.ChangedByUser)
            .FirstOrDefaultAsync(br => br.Id == id);

        if (bookingRequest == null)
        {
            _logger.LogWarning("Repository: BookingRequest with ID: {BookingRequestId} was NOT FOUND.", id);
        }
        else
        {
            _logger.LogInformation("Repository: BookingRequest with ID: {BookingRequestId} FOUND successfully.", id);
        }

        return bookingRequest;
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