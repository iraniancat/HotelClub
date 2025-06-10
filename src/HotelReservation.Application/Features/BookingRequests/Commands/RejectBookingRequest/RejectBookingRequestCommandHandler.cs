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
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<RejectBookingRequestCommandHandler> _logger;

    public RejectBookingRequestCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, ILogger<RejectBookingRequestCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task Handle(RejectBookingRequestCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        var currentUserHotelId = _currentUserService.HotelId;

        if (!currentUserId.HasValue || !currentUserHotelId.HasValue || !_currentUserService.IsInRole("HotelUser"))
        {
            throw new ForbiddenAccessException("کاربر برای انجام این عملیات مجوز ندارد.");
        }

        var bookingRequest = await _unitOfWork.BookingRequestRepository.GetByIdAsync(request.BookingRequestId, asNoTracking: false);
        if (bookingRequest == null) throw new NotFoundException(nameof(BookingRequest), request.BookingRequestId);
        if (bookingRequest.HotelId != currentUserHotelId.Value) throw new ForbiddenAccessException("شما فقط می‌توانید درخواست‌های هتل خود را مدیریت کنید.");
        if (bookingRequest.Status != BookingStatus.SubmittedToHotel) throw new BadRequestException("این درخواست در وضعیت قابل رد کردن نیست.");
        
        var reason = $"رد شده توسط هتل. دلیل: {request.RejectionReason}";
        bookingRequest.UpdateStatus(BookingStatus.HotelRejected, currentUserId.Value, reason);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("BookingRequest {Id} rejected by User {UserId}", request.BookingRequestId, currentUserId.Value);
    }
}
