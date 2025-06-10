// فایل: src/HotelReservation.Application/Features/BookingRequests/Commands/ApproveBookingRequest/ApproveBookingRequestCommandHandler.cs

using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Application.Contracts.Security;
using HotelReservation.Application.Exceptions;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore; // برای ToHashSetAsync در آینده
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HotelReservation.Application.Features.BookingRequests.Commands.ApproveBookingRequest;

public class ApproveBookingRequestCommandHandler : IRequestHandler<ApproveBookingRequestCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ApproveBookingRequestCommandHandler> _logger;

    public ApproveBookingRequestCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<ApproveBookingRequestCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task Handle(ApproveBookingRequestCommand request, CancellationToken cancellationToken)
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
        if (bookingRequest.Status != BookingStatus.SubmittedToHotel) throw new BadRequestException("این درخواست در وضعیت قابل تأیید نیست.");

        // منطق بررسی ظرفیت اتاق
        var overlappingBookings = await _unitOfWork.BookingRequestRepository.GetAsync(
            br => br.HotelId == bookingRequest.HotelId &&
                  br.Id != bookingRequest.Id &&
                  br.Status == BookingStatus.HotelApproved &&
                  (br.CheckInDate < bookingRequest.CheckOutDate && br.CheckOutDate > bookingRequest.CheckInDate)
        );
        var takenRoomIds = overlappingBookings
                            .Where(br => br.AssignedRoomId.HasValue)
                            .Select(br => br.AssignedRoomId)
                            .ToHashSet();
                            
        var suitableRoom = (await _unitOfWork.RoomRepository.GetAsync(
            r => r.HotelId == bookingRequest.HotelId &&
                 !takenRoomIds.Contains(r.Id) && // <<-- اصلاح شد: r.Id به جای r.AssignedRoomId
                 r.Capacity >= bookingRequest.TotalGuests
        )).OrderBy(r => r.Capacity).FirstOrDefault();

        if (suitableRoom == null)
        {
            throw new BadRequestException("متاسفانه، هیچ اتاق خالی با ظرفیت کافی برای تاریخ‌های درخواستی موجود نیست.");
        }

        bookingRequest.AssignRoom(suitableRoom.Id, suitableRoom);
        
        var comments = string.IsNullOrWhiteSpace(request.Comments) 
            ? $"تأیید شده توسط هتل. اتاق {suitableRoom.RoomNumber} تخصیص داده شد." 
            : $"{request.Comments} (اتاق {suitableRoom.RoomNumber} تخصیص داده شد.)";
            
        bookingRequest.UpdateStatus(BookingStatus.HotelApproved, currentUserId.Value, comments);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("BookingRequest {Id} approved by User {UserId}", request.BookingRequestId, currentUserId.Value);
    }
}

// ... سایر فایل‌ها در این Canvas بدون تغییر باقی می‌مانند ...

