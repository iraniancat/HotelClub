using HotelReservation.Application.Contracts.Security;
using HotelReservation.Application.DTOs.Booking;
using HotelReservation.Application.DTOs.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelReservation.Application.Features.BookingRequests.Queries.GetAllBookingRequests;
public class GetAllBookingRequestsQueryHandler : IRequestHandler<GetAllBookingRequestsQuery, PagedResult<BookingRequestSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetAllBookingRequestsQueryHandler> _logger;

    public GetAllBookingRequestsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, ILogger<GetAllBookingRequestsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<PagedResult<BookingRequestSummaryDto>> Handle(GetAllBookingRequestsQuery request, CancellationToken cancellationToken)
    {
        // ابتدا کوئری پایه را ایجاد می‌کنیم
        var query = _unitOfWork.BookingRequestRepository.GetQueryable();

        // مرحله ۱: اعمال تمام فیلترها (WHERE clauses)
       if (_currentUserService.IsInRole("ProvinceUser"))
        {
            var provinceCode = _currentUserService.ProvinceCode;
            if (!string.IsNullOrWhiteSpace(provinceCode))
            {
                // کاربر استان، رزروهای مربوط به کارمندان استان خودش را می‌بیند
                query = query.Where(br => br.RequestingEmployee.ProvinceCode == provinceCode);
                _logger.LogInformation("Filtering bookings for ProvinceUser of province {ProvinceCode}", provinceCode);
            }
        }
        else if (_currentUserService.IsInRole("HotelUser"))
        {
            var hotelId = _currentUserService.HotelId;
            if (hotelId.HasValue)
            {
                // کاربر هتل، رزروهای مربوط به هتل خودش را می‌بیند
                query = query.Where(br => br.HotelId == hotelId.Value);
                _logger.LogInformation("Filtering bookings for HotelUser of hotel {HotelId}", hotelId.Value);
            }
        }
        
      if (!string.IsNullOrWhiteSpace(request.StatusFilter) && Enum.TryParse<Domain.Enums.BookingStatus>(request.StatusFilter, true, out var statusEnum))
        {
            query = query.Where(br => br.Status == statusEnum);
        }
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(br => br.TrackingCode.ToLower().Contains(term) 
                                   || (br.Hotel != null && br.Hotel.Name.ToLower().Contains(term))
                                   || (br.RequestingEmployee != null && br.RequestingEmployee.FullName.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query.OrderByDescending(br => br.SubmissionDate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(br => new BookingRequestSummaryDto {
                Id = br.Id,
                TrackingCode = br.TrackingCode,
                RequestingEmployeeFullName = br.RequestingEmployee != null ? br.RequestingEmployee.FullName : "نامشخص",
                HotelName = br.Hotel != null ? br.Hotel.Name : "نامشخص",
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