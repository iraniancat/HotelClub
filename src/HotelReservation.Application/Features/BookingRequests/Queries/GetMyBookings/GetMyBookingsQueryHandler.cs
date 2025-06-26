using HotelReservation.Application.Contracts.Security;
using HotelReservation.Application.DTOs.Booking;
using HotelReservation.Application.DTOs.Common;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Enums;
using LinqKit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelReservation.Application.Features.BookingRequests.Queries.GetMyBookings;

public class GetMyBookingsQueryHandler : IRequestHandler<GetMyBookingsQuery, PagedResult<BookingRequestSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetMyBookingsQueryHandler> _logger;

    public GetMyBookingsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, ILogger<GetMyBookingsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<PagedResult<BookingRequestSummaryDto>> Handle(GetMyBookingsQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        if (!currentUserId.HasValue)
        {
            throw new UnauthorizedAccessException("کاربر احراز هویت نشده است.");
        }

        var currentUser = await _unitOfWork.UserRepository.GetByIdAsync(currentUserId.Value, asNoTracking: true);
        var currentUserNationalCode = currentUser?.NationalCode;
        
        // <<-- لاگ‌های تشخیصی -->>
        _logger.LogInformation("GetMyBookings: CurrentUserId = {UserId}", currentUserId.Value);
        _logger.LogInformation("GetMyBookings: CurrentUserNationalCode = {NationalCode}", currentUserNationalCode ?? "NULL");

        IQueryable<BookingRequest> query = _unitOfWork.BookingRequestRepository.GetQueryable()
            .Include(br => br.Hotel)
            .Include(br => br.RequestSubmitterUser); // برای نمایش نام ثبت کننده

        // فیلتر اصلی بر اساس شناسه ثبت‌کننده یا کد ملی کارمند اصلی
        var filterExpression = PredicateBuilder.New<BookingRequest>(false);
        filterExpression = filterExpression.Or(br => br.RequestSubmitterUserId == currentUserId.Value);
        if (!string.IsNullOrWhiteSpace(currentUserNationalCode))
        {
            filterExpression = filterExpression.Or(br => br.RequestingEmployeeNationalCode == currentUserNationalCode);
        }
        query = query.Where(filterExpression);
        
        // اعمال فیلتر و جستجوی بیشتر
        if (!string.IsNullOrWhiteSpace(request.StatusFilter) && Enum.TryParse<BookingStatus>(request.StatusFilter, true, out var status))
        {
            query = query.Where(br => br.Status == status);
        }
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(br => br.TrackingCode.ToLower().Contains(term) || br.Hotel.Name.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        _logger.LogInformation("GetMyBookings: Found {Count} total matching bookings for user after all filters.", totalCount);

        var items = await query.OrderByDescending(br => br.SubmissionDate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(br => new BookingRequestSummaryDto {
                Id = br.Id,
                TrackingCode = br.TrackingCode,
                RequestingEmployeeNationalCode = br.RequestingEmployeeNationalCode,
                RequestingEmployeeFullName = null, // این باید با Join یا واکشی جداگانه پر شود
                HotelName = br.Hotel.Name,
                CheckInDate = br.CheckInDate,
                CheckOutDate = br.CheckOutDate,
                Status = br.Status.ToString(),
                TotalGuests = br.TotalGuests,
                SubmissionDate = br.SubmissionDate,
                SubmitterUserFullName = br.RequestSubmitterUser.FullName
            })
            .ToListAsync(cancellationToken);

        // پر کردن نام کارمند اصلی برای نمایش در UI
        var nationalCodes = items.Select(i => i.RequestingEmployeeNationalCode).Distinct().ToList();
        if (nationalCodes.Any())
        {
            var employees = await _unitOfWork.UserRepository.GetAsync(u => nationalCodes.Contains(u.NationalCode));
            foreach(var item in items)
            {
                item.RequestingEmployeeFullName = employees.FirstOrDefault(e => e.NationalCode == item.RequestingEmployeeNationalCode)?.FullName;
            }
        }

        return new PagedResult<BookingRequestSummaryDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}