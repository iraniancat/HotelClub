// src/HotelReservation.Application/Features/BookingRequests/Queries/GetBookingRequestDetails/GetBookingRequestDetailsQueryHandler.cs
using HotelReservation.Application.Contracts.Persistence;
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
using System.Security.Claims; // برای ساخت ClaimsPrincipal موقت
using System.Threading;
using System.Threading.Tasks;
using HotelReservation.Application.Contracts.Security; // برای CustomClaimTypes

namespace HotelReservation.Application.Features.BookingRequests.Queries.GetBookingRequestDetails;

public class GetBookingRequestDetailsQueryHandler : IRequestHandler<GetBookingRequestDetailsQuery, BookingRequestDetailsDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<GetBookingRequestDetailsQueryHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public GetBookingRequestDetailsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuthorizationService authorizationService,
        ILogger<GetBookingRequestDetailsQueryHandler> logger)
    {
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    }

    public async Task<BookingRequestDetailsDto?> Handle(GetBookingRequestDetailsQuery request, CancellationToken cancellationToken)
    {
        var bookingRequest = await _unitOfWork.BookingRequestRepository.GetBookingRequestWithDetailsAsync(request.BookingRequestId);
       
        if (bookingRequest == null)
        {
            _logger.LogWarning("BookingRequest with ID {BookingRequestId} not found.", request.BookingRequestId);
            throw new NotFoundException(nameof(BookingRequest), request.BookingRequestId);
        }
        var userPrincipal = _currentUserService.GetUserPrincipal(); // <<-- استفاده از سرویس
        if (userPrincipal == null || !_currentUserService.IsAuthenticated) {
            throw new UnauthorizedAccessException("کاربر برای این عملیات احراز هویت نشده است.");
        }

        var authorizationResult = await _authorizationService.AuthorizeAsync(
            userPrincipal,
            bookingRequest,
            "CanViewBookingRequest"
        );
        if (!authorizationResult.Succeeded) {throw new ForbiddenAccessException("شما مجاز به مشاهده این درخواست رزرو نیستید.");}
        


        // --- ساخت ClaimsPrincipal از اطلاعات کاربر در Query ---
        // این روش برای انتقال اطلاعات کاربر به AuthorizationHandler است.
        // در یک سیستم واقعی، بهتر است ICurrentUserService داشته باشیم.
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, request.AuthenticatedUserId.ToString()),
            new Claim(ClaimTypes.Role, request.AuthenticatedUserRole)
        };
        if (!string.IsNullOrEmpty(request.AuthenticatedUserProvinceCode))
            claims.Add(new Claim(CustomClaimTypes.ProvinceCode, request.AuthenticatedUserProvinceCode));
        if (request.AuthenticatedUserHotelId.HasValue)
            claims.Add(new Claim(CustomClaimTypes.HotelId, request.AuthenticatedUserHotelId.Value.ToString()));
        
        

        

        if (!authorizationResult.Succeeded)
        {
            _logger.LogWarning("User {UserId} (Role: {UserRole}) failed authorization policy 'CanViewBookingRequest' for BookingRequest {BookingId}.",
                request.AuthenticatedUserId, request.AuthenticatedUserRole, request.BookingRequestId);
            // پرتاب ForbiddenAccessException به جای بازگرداندن null، تا Middleware آن را به 403 تبدیل کند.
            throw new ForbiddenAccessException("شما مجاز به مشاهده این درخواست رزرو نیستید.");
        }
        _logger.LogInformation("User {UserId} authorized via policy 'CanViewBookingRequest' to view BookingRequest {BookingId}.", 
            request.AuthenticatedUserId, request.BookingRequestId);
        
        // نگاشت به DTO (همانند قبل)
        var submitterUserDto = new UserManagementListDto(); 
        if (bookingRequest.RequestSubmitterUser != null)
        {
            submitterUserDto.Id = bookingRequest.RequestSubmitterUser.Id;
            submitterUserDto.SystemUserId = bookingRequest.RequestSubmitterUser.SystemUserId;
            submitterUserDto.FullName = bookingRequest.RequestSubmitterUser.FullName;
            submitterUserDto.RoleName = bookingRequest.RequestSubmitterUser.Role?.Name ?? "تعیین نشده";
            submitterUserDto.IsActive = bookingRequest.RequestSubmitterUser.IsActive;
        }

        var hotelDto = new HotelSlimDto(); 
        if (bookingRequest.Hotel != null)
        {
            hotelDto.Id = bookingRequest.Hotel.Id;
            hotelDto.Name = bookingRequest.Hotel.Name;
        }

        return new BookingRequestDetailsDto
        {
            Id = bookingRequest.Id,
            TrackingCode = bookingRequest.TrackingCode,
            RequestingEmployeeNationalCode = bookingRequest.RequestingEmployeeNationalCode,
            BookingPeriod = bookingRequest.BookingPeriod,
            CheckInDate = bookingRequest.CheckInDate,
            CheckOutDate = bookingRequest.CheckOutDate,
            NumberOfNights = bookingRequest.NumberOfNights,
            TotalGuests = bookingRequest.TotalGuests,
            Status = bookingRequest.Status.ToString(),
            SubmissionDate = bookingRequest.SubmissionDate,
            LastStatusUpdateDate = bookingRequest.LastStatusUpdateDate,
            Notes = bookingRequest.Notes,
            Hotel = hotelDto,
            RequestSubmitterUser = submitterUserDto,
            Guests = bookingRequest.Guests.Select(g => new BookingGuestDetailsDto
            {
                Id = g.Id,
                FullName = g.FullName,
                NationalCode = g.NationalCode,
                RelationshipToEmployee = g.RelationshipToEmployee,
                DiscountPercentage = g.DiscountPercentage
            }).ToList()
        };
    }
}