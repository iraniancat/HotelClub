// مسیر: src/HotelReservation.Application/Features/BookingRequests/Queries/GetBookingRequestDetails/GetBookingRequestDetailsQueryHandler.cs
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Application.Contracts.Security; // برای ICurrentUserService
using HotelReservation.Application.DTOs.Booking;
using HotelReservation.Application.DTOs.Hotel;
using HotelReservation.Application.DTOs.UserManagement;
using HotelReservation.Application.Exceptions;
using HotelReservation.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization; // برای IAuthorizationService
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HotelReservation.Application.Features.BookingRequests.Queries.GetBookingRequestDetails;

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

        // --- استفاده مستقیم از ICurrentUserService برای بررسی مجوز ---
        var userPrincipal = _currentUserService.GetUserPrincipal();
        if (userPrincipal == null || !_currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("کاربر برای این عملیات احراز هویت نشده است.");
        }

        var authorizationResult = await _authorizationService.AuthorizeAsync(
            userPrincipal,
            bookingRequest, // منبع
            "CanViewBookingRequest" // نام Policy
        );

        if (!authorizationResult.Succeeded)
        {
            _logger.LogWarning("User {UserId} (Role: {UserRole}) failed authorization for policy 'CanViewBookingRequest' on BookingRequest {BookingId}.", 
                _currentUserService.UserId, _currentUserService.UserRole, request.BookingRequestId);
            throw new ForbiddenAccessException("شما مجاز به مشاهده این درخواست رزرو نیستید.");
        }
        _logger.LogInformation("User {UserId} authorized to view BookingRequest {BookingId}.", _currentUserService.UserId, request.BookingRequestId);

        // واکشی نام کارمند اصلی
        var requestingEmployee = await _unitOfWork.UserRepository.GetByNationalCodeAsync(bookingRequest.RequestingEmployeeNationalCode);
        
        // واکشی نام دوره زمانی
        var period = await _unitOfWork.BookingPeriodRepository.GetByIdAsync(bookingRequest.BookingPeriodId);

        // نگاشت به DTO
        var detailsDto = new BookingRequestDetailsDto
        {
            Id = bookingRequest.Id,
            TrackingCode = bookingRequest.TrackingCode,
            RequestingEmployeeNationalCode = bookingRequest.RequestingEmployeeNationalCode,
            RequestingEmployeeFullName = requestingEmployee?.FullName,
            BookingPeriod = period?.Name ?? "نامشخص",
            CheckInDate = bookingRequest.CheckInDate,
            CheckOutDate = bookingRequest.CheckOutDate,
            NumberOfNights = bookingRequest.NumberOfNights,
            TotalGuests = bookingRequest.TotalGuests,
            Status = bookingRequest.Status.ToString(),
            SubmissionDate = bookingRequest.SubmissionDate,
            LastStatusUpdateDate = bookingRequest.LastStatusUpdateDate,
            Notes = bookingRequest.Notes,
            Hotel = bookingRequest.Hotel == null ? null : new HotelSlimDto { Id = bookingRequest.Hotel.Id, Name = bookingRequest.Hotel.Name },
            RequestSubmitterUser = bookingRequest.RequestSubmitterUser == null ? null : new UserManagementListDto { 
                Id = bookingRequest.RequestSubmitterUser.Id, 
                FullName = bookingRequest.RequestSubmitterUser.FullName, 
                SystemUserId = bookingRequest.RequestSubmitterUser.SystemUserId 
            },
            Guests = bookingRequest.Guests.Select(g => new BookingGuestDetailsDto
            {
                Id = g.Id,
                FullName = g.FullName,
                NationalCode = g.NationalCode,
                RelationshipToEmployee = g.RelationshipToEmployee,
                DiscountPercentage = g.DiscountPercentage
            }).ToList(),
            Files = bookingRequest.Files.Select(f => new BookingFileDto
            {
                Id = f.Id,
                FileName = f.FileName,
                ContentType = f.ContentType,
                UploadedDate = f.UploadedDate
            }).ToList()
        };

        return detailsDto;
    }
}
