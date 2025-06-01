// src/HotelReservation.Application/Features/BookingRequests/Commands/RejectBookingRequest/RejectBookingRequestCommandHandler.cs
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Application.Contracts.Infrastructure; // برای ISmsService
using HotelReservation.Application.Exceptions;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization; // برای IAuthorizationService
using System.Security.Claims;          // برای ساخت ClaimsPrincipal موقت
using HotelReservation.Application.Contracts.Security; // برای CustomClaimTypes

namespace HotelReservation.Application.Features.BookingRequests.Commands.RejectBookingRequest;

public class RejectBookingRequestCommandHandler : IRequestHandler<RejectBookingRequestCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISmsService _smsService;
    private readonly ILogger<RejectBookingRequestCommandHandler> _logger;
    private readonly IAuthorizationService _authorizationService; // <<-- اضافه شد

    public RejectBookingRequestCommandHandler(
        IUnitOfWork unitOfWork, 
        ISmsService smsService, 
        ILogger<RejectBookingRequestCommandHandler> logger,
        IAuthorizationService authorizationService) // <<-- اضافه شد
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService)); // <<-- اضافه شد
    }

    public async Task Handle(RejectBookingRequestCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing RejectBookingRequest for BookingRequestId: {BookingRequestId} by HotelUser: {HotelUserId}",
            request.BookingRequestId, request.HotelUserId);

        var bookingRequestToReject = await _unitOfWork.BookingRequestRepository.GetBookingRequestWithDetailsAsync(request.BookingRequestId);
        if (bookingRequestToReject == null)
        {
             _logger.LogWarning("RejectBookingRequest: BookingRequest with ID {BookingRequestId} not found.", request.BookingRequestId);
            throw new NotFoundException(nameof(BookingRequest), request.BookingRequestId);
        }

        var hotelUserEntity = await _unitOfWork.UserRepository.GetByIdAsync(request.HotelUserId);
        if (hotelUserEntity == null)
        {
            _logger.LogWarning("RejectBookingRequest: HotelUser entity for ID {HotelUserId} not found for building ClaimsPrincipal.", request.HotelUserId);
            throw new BadRequestException($"اطلاعات کاربر هتل برای بررسی مجوز یافت نشد.");
        }
        var hotelUserRole = hotelUserEntity.RoleId != Guid.Empty ? (await _unitOfWork.RoleRepository.GetByIdAsync(hotelUserEntity.RoleId))?.Name : null;


        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, request.HotelUserId.ToString()),
            new Claim(ClaimTypes.Role, hotelUserRole ?? string.Empty)
        };
        if (hotelUserEntity.HotelId.HasValue)
        {
            claims.Add(new Claim(CustomClaimTypes.HotelId, hotelUserEntity.HotelId.Value.ToString()));
        }
        var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "CustomAuthForHandler"));

        var authorizationResult = await _authorizationService.AuthorizeAsync(
            userPrincipal,
            bookingRequestToReject,
            "CanManageBookingForOwnHotel"
        );

        if (!authorizationResult.Succeeded)
        {
            _logger.LogWarning(
                "RejectBookingRequest: Authorization failed for HotelUser {HotelUserId} on BookingRequest {BookingRequestId}. Policy: CanManageBookingForOwnHotel.",
                request.HotelUserId, request.BookingRequestId);
            throw new ForbiddenAccessException("شما مجاز به رد این درخواست رزرو برای این هتل نیستید.");
        }
         _logger.LogInformation("RejectBookingRequest: HotelUser {HotelUserId} authorized for BookingRequest {BookingRequestId}.", request.HotelUserId, request.BookingRequestId);


        if (bookingRequestToReject.Status != BookingStatus.SubmittedToHotel)
        {
            // ... (همانند قبل) ...
             _logger.LogWarning(
             "RejectBookingRequest: BookingRequest {BookingRequestId} is in status {Status} and cannot be rejected. Expected status: {ExpectedStatus}.",
             request.BookingRequestId, bookingRequestToReject.Status, BookingStatus.SubmittedToHotel);
         throw new BadRequestException($"این درخواست رزرو در وضعیت '{bookingRequestToReject.Status}' بوده و قابل رد کردن نیست. فقط درخواست‌های در وضعیت '{BookingStatus.SubmittedToHotel}' قابل رد شدن هستند.");
        }

        // userWhoRejected باید از پایگاه داده خوانده شود تا شیء کامل باشد
        var userWhoRejected = await _unitOfWork.UserRepository.GetByIdAsync(request.HotelUserId);
         if(userWhoRejected == null) { 
              _logger.LogError("User who rejected the request (HotelUserId: {HotelUserId}) not found right before updating status.", request.HotelUserId);
              throw new ApplicationException("خطا در واکشی اطلاعات کاربر رد کننده.");
         }

        bookingRequestToReject.UpdateStatus(BookingStatus.HotelRejected, request.HotelUserId, userWhoRejected, request.RejectionReason);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
         _logger.LogInformation(
             "RejectBookingRequest: BookingRequestId: {BookingRequestId} rejected successfully. Status updated to {NewStatus}.", 
             bookingRequestToReject.Id, BookingStatus.HotelRejected);


        // ارسال SMS به کارمند اصلی (همانند قبل)
        var mainEmployeeUser = await _unitOfWork.UserRepository.GetByNationalCodeAsync(bookingRequestToReject.RequestingEmployeeNationalCode);
        if (mainEmployeeUser != null && !string.IsNullOrEmpty(mainEmployeeUser.PhoneNumber))
        {
            try
            {
                await _smsService.SendSmsAsync(mainEmployeeUser.PhoneNumber,
                    $"درخواست رزرو شما با کد رهگیری {bookingRequestToReject.TrackingCode} برای هتل {bookingRequestToReject.Hotel.Name} متاسفانه رد شد. دلیل: {request.RejectionReason}");
                _logger.LogInformation(
                    "RejectBookingRequest: Rejection SMS sent to {PhoneNumber} for booking {TrackingCode}.", 
                    mainEmployeeUser.PhoneNumber, bookingRequestToReject.TrackingCode);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, 
                    "RejectBookingRequest: Failed to send rejection SMS for booking {TrackingCode} to {PhoneNumber}.", 
                    bookingRequestToReject.TrackingCode, mainEmployeeUser.PhoneNumber);
            }
        }
         else
        {
             _logger.LogWarning(
                 "RejectBookingRequest: Phone number for main employee (NationalCode: {NationalCode}) not found or empty for booking {TrackingCode}. Rejection SMS not sent.", 
                 bookingRequestToReject.RequestingEmployeeNationalCode, bookingRequestToReject.TrackingCode);
        }
    }
}