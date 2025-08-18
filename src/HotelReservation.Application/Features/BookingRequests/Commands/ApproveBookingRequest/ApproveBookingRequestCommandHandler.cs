// فایل: src/HotelReservation.Application/Features/BookingRequests/Commands/ApproveBookingRequest/ApproveBookingRequestCommandHandler.cs

using HotelReservation.Application.Contracts.Infrastructure;
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
     private readonly ISmsService _smsService;


    public ApproveBookingRequestCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<ApproveBookingRequestCommandHandler> logger,
        ISmsService smsService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
        _smsService = smsService;
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
        // var overlappingBookings = await _unitOfWork.BookingRequestRepository.GetAsync(
        //     br => br.HotelId == bookingRequest.HotelId &&
        //           br.Id != bookingRequest.Id &&
        //           br.Status == BookingStatus.HotelApproved &&
        //           (br.CheckInDate < bookingRequest.CheckOutDate && br.CheckOutDate > bookingRequest.CheckInDate)
        // );
        // var takenRoomIds = overlappingBookings
        //                     .Where(br => br.AssignedRoomId.HasValue)
        //                     .Select(br => br.AssignedRoomId)
        //                     .ToHashSet();

        // var suitableRoom = (await _unitOfWork.RoomRepository.GetAsync(
        //     r => r.HotelId == bookingRequest.HotelId &&
        //          !takenRoomIds.Contains(r.Id) && // <<-- اصلاح شد: r.Id به جای r.AssignedRoomId
        //          r.Capacity >= bookingRequest.TotalGuests
        // )).OrderBy(r => r.Capacity).FirstOrDefault();

        // if (suitableRoom == null)
        // {
        //     throw new BadRequestException("متاسفانه، هیچ اتاق خالی با ظرفیت کافی برای تاریخ‌های درخواستی موجود نیست.");
        // }

        // bookingRequest.AssignRoom(suitableRoom.Id, suitableRoom);

        // var comments = string.IsNullOrWhiteSpace(request.Comments) 
        //     ? $"تأیید شده توسط هتل. اتاق {suitableRoom.RoomNumber} تخصیص داده شد." 
        //     : $"{request.Comments} (اتاق {suitableRoom.RoomNumber} تخصیص داده شد.)";

        var comments = string.IsNullOrWhiteSpace(request.Comments)
                    ? "تأیید شده توسط هتل."
                    : request.Comments;

        var historyEntry = bookingRequest.UpdateStatus(BookingStatus.HotelApproved, currentUserId.Value, comments);

        if (historyEntry != null)
        {
            await _unitOfWork.BookingStatusHistoryRepository.AddAsync(historyEntry);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("BookingRequest {Id} approved by User {UserId}", request.BookingRequestId, currentUserId.Value);
        
         var mainEmployeeUser = await _unitOfWork.UserRepository.GetByNationalCodeAsync(bookingRequest.RequestingEmployeeNationalCode, true);
        if (mainEmployeeUser != null && !string.IsNullOrEmpty(mainEmployeeUser.PhoneNumber))
        {
            try
            {
                string message=$@"درخواست رزرو شما با کد رهگیری {bookingRequest.TrackingCode} توسط هتل {bookingRequest.Hotel.Name} تایید گردید."
                                + Environment.NewLine
                                + $@"تاریخ ورود: {bookingRequest.CheckInDate}"
                                + Environment.NewLine
                                + $@"تاریخ خروج: {bookingRequest.CheckOutDate}"
                                + Environment.NewLine
                                + $@"تعداد مهمانان: {bookingRequest.TotalGuests}"
                                + Environment.NewLine
                                + $@"ساعت تحویل: 14:00";
                await _smsService.SendSmsAsync(mainEmployeeUser.PhoneNumber,message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send cancellation SMS for booking {TrackingCode}.", bookingRequest.TrackingCode);
            }
        }
    }
}

// ... سایر فایل‌ها در این Canvas بدون تغییر باقی می‌مانند ...

