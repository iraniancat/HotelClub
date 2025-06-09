using HotelReservation.Application.Contracts.Security;
using HotelReservation.Application.DTOs.Booking;
using HotelReservation.Application.DTOs.UserManagement;
using HotelReservation.Application.Exceptions;
using HotelReservation.Application.Features.BookingRequests.Queries.GetBookingRequestDetails;
using HotelReservation.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

public class GetBookingRequestDetailsQueryHandler : IRequestHandler<GetBookingRequestDetailsQuery, BookingRequestDetailsDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetBookingRequestDetailsQueryHandler> _logger;

    public GetBookingRequestDetailsQueryHandler(
        IUnitOfWork unitOfWork, 
        IAuthorizationService authorizationService,
        ICurrentUserService currentUserService,
        ILogger<GetBookingRequestDetailsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<BookingRequestDetailsDto?> Handle(GetBookingRequestDetailsQuery request, CancellationToken cancellationToken)
    {
        var bookingRequest = await _unitOfWork.BookingRequestRepository.GetBookingRequestWithDetailsAsync(request.BookingRequestId);
        if (bookingRequest == null)
        {
            throw new NotFoundException(nameof(BookingRequest), request.BookingRequestId);
        }

        // بررسی مجوز دسترسی (با فرض اینکه ICurrentUserService و Policy به درستی کار می‌کنند)
        var userPrincipal = _currentUserService.GetUserPrincipal();
        if (userPrincipal == null) throw new UnauthorizedAccessException();

        var authorizationResult = await _authorizationService.AuthorizeAsync(userPrincipal, bookingRequest, "CanViewBookingRequest");
        if (!authorizationResult.Succeeded)
        {
            throw new ForbiddenAccessException("شما مجاز به مشاهده این درخواست رزرو نیستید.");
        }

        // واکشی نام کارمند اصلی
        var requestingEmployee = await _unitOfWork.UserRepository.GetByNationalCodeAsync(bookingRequest.RequestingEmployeeNationalCode);

        // نگاشت به DTO
        var detailsDto = new BookingRequestDetailsDto
        {
            Id = bookingRequest.Id,
            TrackingCode = bookingRequest.TrackingCode,
            RequestingEmployeeNationalCode = bookingRequest.RequestingEmployeeNationalCode,
            RequestingEmployeeFullName = requestingEmployee?.FullName, // <<-- پر کردن نام کامل کارمند
            // BookingPeriodId = bookingRequest.BookingPeriodId, // <<-- این خط حذف شد چون در DTO وجود ندارد
            CheckInDate = bookingRequest.CheckInDate,
            CheckOutDate = bookingRequest.CheckOutDate,
            NumberOfNights = bookingRequest.NumberOfNights,
            TotalGuests = bookingRequest.TotalGuests,
            Status = bookingRequest.Status.ToString(),
            SubmissionDate = bookingRequest.SubmissionDate,
            LastStatusUpdateDate = bookingRequest.LastStatusUpdateDate,
            Notes = bookingRequest.Notes,
            Hotel = bookingRequest.Hotel == null ? null : new HotelSlimDto { Id = bookingRequest.Hotel.Id, Name = bookingRequest.Hotel.Name },
            RequestSubmitterUser = bookingRequest.RequestSubmitterUser == null ? null : new UserManagementListDto { Id = bookingRequest.RequestSubmitterUser.Id, FullName = bookingRequest.RequestSubmitterUser.FullName, SystemUserId = bookingRequest.RequestSubmitterUser.SystemUserId },
            Guests = bookingRequest.Guests.Select(g => new BookingGuestDetailsDto
            {
                Id = g.Id,
                FullName = g.FullName,
                NationalCode = g.NationalCode,
                RelationshipToEmployee = g.RelationshipToEmployee,
                DiscountPercentage = g.DiscountPercentage
            }).ToList(),
            Files = bookingRequest.Files.Select(f => new BookingFileDto // <<-- پر کردن لیست فایل‌ها
            {
                Id = f.Id,
                FileName = f.FileName,
                ContentType = f.ContentType
            }).ToList()
        };
        // واکشی و تنظیم نام دوره زمانی
        var period = await _unitOfWork.BookingPeriodRepository.GetByIdAsync(bookingRequest.BookingPeriodId);
        detailsDto.BookingPeriod = period?.Name ?? "نامشخص";


        return detailsDto;
    }
}