// src/HotelReservation.Application/Features/BookingRequests/Commands/CancelBookingRequest/CancelBookingRequestCommandHandler.cs
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Application.Contracts.Infrastructure; // برای ISmsService
using HotelReservation.Application.Contracts.Security;    // برای ICurrentUserService
using HotelReservation.Application.Exceptions;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization; // برای IAuthorizationService
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic; // برای List<Claim>

namespace HotelReservation.Application.Features.BookingRequests.Commands.CancelBookingRequest;

public class CancelBookingRequestCommandHandler : IRequestHandler<CancelBookingRequestCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService; // برای بررسی مجوزهای پیچیده‌تر
    private readonly ISmsService _smsService;
    private readonly ILogger<CancelBookingRequestCommandHandler> _logger;
    
    private const string SuperAdminRoleName = "SuperAdmin";

    public CancelBookingRequestCommandHandler(
        IUnitOfWork unitOfWork, 
        ICurrentUserService currentUserService,
        IAuthorizationService authorizationService,
        ISmsService smsService, 
        ILogger<CancelBookingRequestCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(CancelBookingRequestCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        if (!currentUserId.HasValue)
        {
            throw new UnauthorizedAccessException("کاربر برای انجام این عملیات احراز هویت نشده است.");
        }

        _logger.LogInformation("User {UserId} attempting to cancel BookingRequestId: {BookingRequestId}", currentUserId.Value, request.BookingRequestId);

        var bookingRequest = await _unitOfWork.BookingRequestRepository.GetBookingRequestWithDetailsAsync(request.BookingRequestId);
        if (bookingRequest == null)
        {
            throw new NotFoundException(nameof(BookingRequest), request.BookingRequestId);
        }

        // بررسی مجوز:
        // ۱. مدیر ارشد همیشه می‌تواند لغو کند.
        // ۲. کاربری که درخواست را ثبت کرده، می‌تواند لغو کند (اگر وضعیت اجازه دهد).
        bool canCancel = false;
        if (_currentUserService.IsInRole(SuperAdminRoleName))
        {
            canCancel = true;
        }
        else if (bookingRequest.RequestSubmitterUserId == currentUserId.Value)
        {
            // بررسی وضعیت‌های مجاز برای لغو توسط ثبت کننده
            if (bookingRequest.Status == BookingStatus.SubmittedToHotel || bookingRequest.Status == BookingStatus.Draft)
            {
                canCancel = true;
            }
            else
            {
                _logger.LogWarning("User {UserId} attempted to cancel booking {BookingId} in non-cancellable state {Status} by submitter.",
                    currentUserId.Value, request.BookingRequestId, bookingRequest.Status);
                throw new BadRequestException($"امکان لغو این درخواست در وضعیت فعلی ({bookingRequest.Status}) توسط شما وجود ندارد.");
            }
        }

        if (!canCancel)
        {
            // اینجا می‌توان از Resource-based Authorization Policy هم استفاده کرد اگر پیچیده‌تر بود.
            // فعلاً با بررسی مستقیم پیش می‌رویم.
            _logger.LogWarning("User {UserId} is not authorized to cancel BookingRequest {BookingRequestId}.", currentUserId.Value, request.BookingRequestId);
            throw new ForbiddenAccessException("شما مجاز به لغو این درخواست رزرو نیستید.");
        }

        // بررسی اینکه آیا درخواست قبلاً لغو یا تکمیل نشده باشد
        if (bookingRequest.Status == BookingStatus.CancelledByUser || bookingRequest.Status == BookingStatus.Completed || bookingRequest.Status == BookingStatus.HotelRejected)
        {
            _logger.LogInformation("BookingRequest {BookingRequestId} is already in a final state ({Status}) and cannot be cancelled again.", request.BookingRequestId, bookingRequest.Status);
            // می‌توان خطا برنگرداند و فقط عملیات را انجام ندهد، یا یک پیام موفقیت‌آمیز (اما بدون تغییر) برگرداند.
            // برای سادگی، اگر قبلاً در وضعیت نهایی است، خطایی برنمی‌گردانیم.
            return; 
        }

        var cancellingUserEntity = await _unitOfWork.UserRepository.GetByIdAsync(currentUserId.Value);
        if(cancellingUserEntity == null) { /* این نباید اتفاق بیفتد اگر کاربر لاگین کرده */
             throw new ApplicationException("خطا در واکشی اطلاعات کاربر لغو کننده.");
        }

        string reason = string.IsNullOrWhiteSpace(request.CancellationReason) 
                        ? "لغو شده توسط کاربر." 
                        : $"لغو شده توسط کاربر. دلیل: {request.CancellationReason}";

        bookingRequest.UpdateStatus(BookingStatus.CancelledByUser, currentUserId.Value, cancellingUserEntity, reason);
        // متد UpdateStatus باید AssignedRoomId را null کند اگر وضعیت به CancelledByUser تغییر می‌کند (که این کار را انجام می‌دهد).

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("BookingRequest {BookingRequestId} cancelled successfully by User {UserId}.", request.BookingRequestId, currentUserId.Value);

        // ارسال SMS به کارمند اصلی درخواست دهنده (و شاید هتل اگر قبلاً به هتل ارسال شده بود)
        var mainEmployeeUser = await _unitOfWork.UserRepository.GetByNationalCodeAsync(bookingRequest.RequestingEmployeeNationalCode);
        if (mainEmployeeUser != null && !string.IsNullOrEmpty(mainEmployeeUser.PhoneNumber))
        {
            try
            {
               await _smsService.SendSmsAsync(mainEmployeeUser.PhoneNumber,
                    $"درخواست رزرو شما با کد رهگیری {bookingRequest.TrackingCode} برای هتل {bookingRequest.Hotel.Name} لغو گردید.");
               _logger.LogInformation("Cancellation SMS sent to {PhoneNumber} for booking {TrackingCode}.", mainEmployeeUser.PhoneNumber, bookingRequest.TrackingCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send cancellation SMS for booking {TrackingCode}.", bookingRequest.TrackingCode);
            }
        }
    }
}