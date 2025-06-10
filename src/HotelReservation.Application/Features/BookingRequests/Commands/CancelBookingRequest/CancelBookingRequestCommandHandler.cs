// مسیر: src/HotelReservation.Application/Features/BookingRequests/Commands/CancelBookingRequest/CancelBookingRequestCommandHandler.cs
using HotelReservation.Application.Contracts.Infrastructure;
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Application.Contracts.Security;
using HotelReservation.Application.Exceptions;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HotelReservation.Application.Features.BookingRequests.Commands.CancelBookingRequest;

public class CancelBookingRequestCommandHandler : IRequestHandler<CancelBookingRequestCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CancelBookingRequestCommandHandler> _logger;
    private readonly ISmsService _smsService;

    private const string SuperAdminRoleName = "SuperAdmin";

    public CancelBookingRequestCommandHandler(
        IUnitOfWork unitOfWork, 
        ICurrentUserService currentUserService,
        ILogger<CancelBookingRequestCommandHandler> logger,
        ISmsService smsService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
        _smsService = smsService;
    }

    public async Task Handle(CancelBookingRequestCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("--- CancelBookingRequestCommandHandler started for BookingRequestId: {BookingRequestId} ---", request.BookingRequestId);

        var currentUserId = _currentUserService.UserId;
        if (!currentUserId.HasValue)
        {
            _logger.LogWarning("Handler STOPPED: Current user is not authenticated.");
            throw new UnauthorizedAccessException("کاربر برای انجام این عملیات احراز هویت نشده است.");
        }
        _logger.LogInformation("Step 1: CurrentUserId found: {CurrentUserId}", currentUserId.Value);

        var bookingRequest = await _unitOfWork.BookingRequestRepository.GetBookingRequestWithDetailsAsync(request.BookingRequestId, asNoTracking: false);
        if (bookingRequest == null)
        {
            _logger.LogWarning("Handler STOPPED: BookingRequest with ID {BookingRequestId} not found in database.", request.BookingRequestId);
            throw new NotFoundException(nameof(BookingRequest), request.BookingRequestId);
        }
        _logger.LogInformation("Step 2: BookingRequest with ID {BookingRequestId} found and is being tracked. Current Status: {Status}", request.BookingRequestId, bookingRequest.Status);

        bool canCancel = _currentUserService.IsInRole(SuperAdminRoleName) || 
                         bookingRequest.RequestSubmitterUserId == currentUserId.Value;

        if (!canCancel)
        {
            _logger.LogWarning("Handler STOPPED: User {CurrentUserId} is not authorized to cancel BookingRequest {BookingRequestId}.", currentUserId.Value, request.BookingRequestId);
            throw new ForbiddenAccessException("شما مجاز به لغو این درخواست رزرو نیستید.");
        }
        _logger.LogInformation("Step 3: User {CurrentUserId} is authorized to cancel.", currentUserId.Value);

        if (bookingRequest.Status != BookingStatus.SubmittedToHotel && bookingRequest.Status != BookingStatus.HotelApproved)
        {
             _logger.LogWarning("Handler STOPPED: Cannot cancel BookingRequest {BookingRequestId} because its status is {Status}.", request.BookingRequestId, bookingRequest.Status);
             throw new BadRequestException($"امکان لغو این درخواست در وضعیت فعلی ({bookingRequest.Status}) وجود ندارد.");
        }
        _logger.LogInformation("Step 4: BookingRequest status is valid for cancellation ({Status}).", bookingRequest.Status);
        
        string reason = string.IsNullOrWhiteSpace(request.CancellationReason) 
                       ? "لغو شده توسط کاربر." 
                       : $"لغو شده توسط کاربر. دلیل: {request.CancellationReason}";

        _logger.LogInformation("Step 5: Calling bookingRequest.UpdateStatus() to change status to CancelledByUser.");
        bookingRequest.UpdateStatus(BookingStatus.CancelledByUser, currentUserId.Value, reason);
        _logger.LogInformation("Step 6: bookingRequest.UpdateStatus() completed. New status in memory is {NewStatus}.", bookingRequest.Status);
        
        try
        {
            _logger.LogInformation("Step 7: Calling SaveChangesAsync...");
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Step 8: SaveChangesAsync completed successfully for BookingRequest {BookingRequestId}.", request.BookingRequestId);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "A DbUpdateConcurrencyException occurred at SaveChangesAsync. This means the row was modified or deleted by another process, or EF's ChangeTracker is out of sync.");
            throw; // Re-throw the exception to be handled by the middleware
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected exception occurred at SaveChangesAsync.");
            throw;
        }

        var mainEmployeeUser = await _unitOfWork.UserRepository.GetByNationalCodeAsync(bookingRequest.RequestingEmployeeNationalCode, true);
        if (mainEmployeeUser != null && !string.IsNullOrEmpty(mainEmployeeUser.PhoneNumber))
        {
            try
            {
                await _smsService.SendSmsAsync(mainEmployeeUser.PhoneNumber,
                     $"درخواست رزرو شما با کد رهگیری {bookingRequest.TrackingCode} لغو گردید.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send cancellation SMS for booking {TrackingCode}.", bookingRequest.TrackingCode);
            }
        }
    }
}
