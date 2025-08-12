using HotelReservation.Application.Contracts.Security;
using HotelReservation.Application.DTOs.Booking;
using HotelReservation.Application.DTOs.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Application.Features.BookingRequests.Queries.GetAllBookingRequests;
public class GetAllBookingRequestsQueryHandler : IRequestHandler<GetAllBookingRequestsQuery, PagedResult<BookingRequestSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetAllBookingRequestsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResult<BookingRequestSummaryDto>> Handle(GetAllBookingRequestsQuery request, CancellationToken cancellationToken)
    {
        // ابتدا کوئری پایه را ایجاد می‌کنیم
        var query = _unitOfWork.BookingRequestRepository.GetQueryable();

        // مرحله ۱: اعمال تمام فیلترها (WHERE clauses)
        if (_currentUserService.IsInRole("ProvinceUser"))
        {
            var provinceCode = _currentUserService.ProvinceCode;
            query = query.Where(br => br.RequestingEmployee.ProvinceCode == provinceCode);
        }
        else if (_currentUserService.IsInRole("HotelUser"))
        {
            var hotelId = _currentUserService.HotelId;
            query = query.Where(br => br.HotelId == hotelId);
        }
        
        if (!string.IsNullOrWhiteSpace(request.StatusFilter) && Enum.TryParse<Domain.Enums.BookingStatus>(request.StatusFilter, true, out var statusEnum))
        {
            query = query.Where(br => br.Status == statusEnum);
        }
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(br => br.TrackingCode.ToLower().Contains(term) 
                                   || br.Hotel.Name.ToLower().Contains(term)
                                   || br.RequestingEmployee.FullName.ToLower().Contains(term));
        }

        // شمارش نتایج فیلتر شده برای صفحه‌بندی
        var totalCount = await query.CountAsync(cancellationToken);

        // مرحله ۲: اعمال واکشی داده‌های مرتبط (Includes)، مرتب‌سازی و صفحه‌بندی
        var items = await query
            .Include(br => br.Hotel)
            .Include(br => br.RequestingEmployee)
            .Include(br => br.Guests)
            .OrderByDescending(br => br.SubmissionDate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(br => new BookingRequestSummaryDto {
                Id = br.Id,
                TrackingCode = br.TrackingCode,
                RequestingEmployeeFullName = br.RequestingEmployee.FullName,
                HotelName = br.Hotel.Name,
                CheckInDate = br.CheckInDate,
                CheckOutDate = br.CheckOutDate,
                Status = br.Status.ToString(),
                TotalGuests = br.TotalGuests,
                GuestNames = string.Join("، ", br.Guests.Select(g => g.FullName).Take(3))
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        
        return new PagedResult<BookingRequestSummaryDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}