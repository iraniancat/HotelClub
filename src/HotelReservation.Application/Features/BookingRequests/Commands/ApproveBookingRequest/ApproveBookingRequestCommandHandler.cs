// src/HotelReservation.Application/Features/BookingRequests/Commands/ApproveBookingRequest/ApproveBookingRequestCommandHandler.cs
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Application.Contracts.Infrastructure;
using HotelReservation.Application.Exceptions;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.AspNetCore.Authorization; // برای IAuthorizationService
using System.Security.Claims;
using HotelReservation.Application.Contracts.Security;          // برای ساخت ClaimsPrincipal موقت

namespace HotelReservation.Application.Features.BookingRequests.Commands.ApproveBookingRequest;

public class ApproveBookingRequestCommandHandler : IRequestHandler<ApproveBookingRequestCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISmsService _smsService;
    private readonly ILogger<ApproveBookingRequestCommandHandler> _logger;
    private readonly IAuthorizationService _authorizationService; // <<-- اضافه شد

    public ApproveBookingRequestCommandHandler(
        IUnitOfWork unitOfWork, 
        ISmsService smsService, 
        ILogger<ApproveBookingRequestCommandHandler> logger,
        IAuthorizationService authorizationService) // <<-- اضافه شد
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService)); // <<-- اضافه شد
    }

    public async Task Handle(ApproveBookingRequestCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing ApproveBookingRequest for BookingRequestId: {BookingRequestId} by HotelUser: {HotelUserId}",
            request.BookingRequestId, request.HotelUserId);

        var bookingRequestToApprove = await _unitOfWork.BookingRequestRepository.GetBookingRequestWithDetailsAsync(request.BookingRequestId);
        if (bookingRequestToApprove == null)
        {
            _logger.LogWarning("ApproveBookingRequest: BookingRequest with ID {BookingRequestId} not found.", request.BookingRequestId);
            throw new NotFoundException(nameof(BookingRequest), request.BookingRequestId);
        }

        var hotelUserEntity = await _unitOfWork.UserRepository.GetByIdAsync(request.HotelUserId); // برای ساخت ClaimsPrincipal
        if (hotelUserEntity == null)
        {
             _logger.LogWarning("ApproveBookingRequest: HotelUser entity for ID {HotelUserId} not found for building ClaimsPrincipal.", request.HotelUserId);
            throw new BadRequestException($"اطلاعات کاربر هتل برای بررسی مجوز یافت نشد.");
        }
         // واکشی نقش کاربر برای ClaimsPrincipal
         var hotelUserRole = hotelUserEntity.RoleId != Guid.Empty ? (await _unitOfWork.RoleRepository.GetByIdAsync(hotelUserEntity.RoleId))?.Name : null;


        // --- ساخت ClaimsPrincipal از اطلاعات کاربر هتل ---
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, request.HotelUserId.ToString()),
            new Claim(ClaimTypes.Role, hotelUserRole ?? string.Empty) // استفاده از نقش واکشی شده
        };
        if (hotelUserEntity.HotelId.HasValue) // اطمینان از وجود HotelId برای کاربر هتل
        {
            claims.Add(new Claim(CustomClaimTypes.HotelId, hotelUserEntity.HotelId.Value.ToString()));
        }
        var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "CustomAuthForHandler"));

        // --- بررسی مجوز با استفاده از Policy ---
        var authorizationResult = await _authorizationService.AuthorizeAsync(
            userPrincipal,
            bookingRequestToApprove, // منبع
            "CanManageBookingForOwnHotel" // نام Policy
        );

        if (!authorizationResult.Succeeded)
        {
            _logger.LogWarning(
                "ApproveBookingRequest: Authorization failed for HotelUser {HotelUserId} on BookingRequest {BookingRequestId}. Policy: CanManageBookingForOwnHotel.",
                request.HotelUserId, request.BookingRequestId);
            throw new ForbiddenAccessException("شما مجاز به تأیید این درخواست رزرو برای این هتل نیستید.");
        }
        _logger.LogInformation("ApproveBookingRequest: HotelUser {HotelUserId} authorized for BookingRequest {BookingRequestId}.", request.HotelUserId, request.BookingRequestId);

        // <<-- حذف بررسی مجوز دستی قبلی -->>
        // if (hotelUser.HotelId == null || hotelUser.AssignedHotel?.Id != bookingRequestToApprove.HotelId) ...

        if (bookingRequestToApprove.Status != BookingStatus.SubmittedToHotel)
        {
            // ... (همانند قبل) ...
             _logger.LogWarning(
             "ApproveBookingRequest: BookingRequest {BookingRequestId} is in status {Status} and cannot be approved. Expected status: {ExpectedStatus}.",
             request.BookingRequestId, bookingRequestToApprove.Status, BookingStatus.SubmittedToHotel);
             throw new BadRequestException($"این درخواست رزرو در وضعیت '{bookingRequestToApprove.Status}' بوده و قابل تأیید نیست. فقط درخواست‌های در وضعیت '{BookingStatus.SubmittedToHotel}' قابل تأیید هستند.");
        }

        // ... (بقیه منطق بررسی ظرفیت، تخصیص اتاق، به‌روزرسانی وضعیت و ارسال SMS همانند قبل) ...
         // ۱. واکشی تمام اتاق‌های هتل مربوطه 
         var allHotelRooms = (await _unitOfWork.RoomRepository.GetAsyncWithIncludes(
             r => r.HotelId == bookingRequestToApprove.HotelId,
             disableTracking: false 
         )).ToList();

         if (!allHotelRooms.Any())
         {
             _logger.LogWarning("ApproveBookingRequest: No rooms found for HotelId: {HotelId} (Hotel: {HotelName}) associated with BookingRequestId: {BookingRequestId}.",
                 bookingRequestToApprove.HotelId, bookingRequestToApprove.Hotel.Name, bookingRequestToApprove.Id);
             throw new BadRequestException($"هتل '{bookingRequestToApprove.Hotel.Name}' هیچ اتاقی برای تخصیص ندارد.");
         }
         _logger.LogInformation("ApproveBookingRequest: Found {RoomCount} total rooms for HotelId: {HotelId}.", allHotelRooms.Count, bookingRequestToApprove.HotelId);

         var overlappingApprovedBookings = (await _unitOfWork.BookingRequestRepository.GetAsync(
             br => br.HotelId == bookingRequestToApprove.HotelId &&      
                   br.Id != bookingRequestToApprove.Id &&                
                   br.Status == BookingStatus.HotelApproved &&           
                   br.AssignedRoomId != null &&                          
                   (br.CheckInDate < bookingRequestToApprove.CheckOutDate && br.CheckOutDate > bookingRequestToApprove.CheckInDate)
         )).ToList();
         _logger.LogInformation("ApproveBookingRequest: Found {OverlappingCount} other approved bookings overlapping with the requested period for BookingRequestId: {BookingRequestId}.",
             overlappingApprovedBookings.Count, bookingRequestToApprove.Id);

         var takenRoomIdsInPeriod = overlappingApprovedBookings
             .Select(br => br.AssignedRoomId!.Value) 
             .Distinct()
             .ToList();
         _logger.LogInformation("ApproveBookingRequest: Taken Room IDs during the period for BookingRequestId: {BookingRequestId}: [{TakenRoomIds}]",
              bookingRequestToApprove.Id, string.Join(", ", takenRoomIdsInPeriod));

         var availableRoomsInPeriod = allHotelRooms
             .Where(r => !takenRoomIdsInPeriod.Contains(r.Id))
             .ToList();
         _logger.LogInformation("ApproveBookingRequest: Potentially available rooms (not taken in period) for BookingRequestId: {BookingRequestId}: {AvailableRoomsCount}. IDs: [{AvailableRoomIds}]",
             bookingRequestToApprove.Id, availableRoomsInPeriod.Count, string.Join(", ", availableRoomsInPeriod.Select(r=>r.Id)));

         var suitableRoom = availableRoomsInPeriod
             .Where(r => r.Capacity >= bookingRequestToApprove.TotalGuests) 
             .OrderBy(r => r.Capacity)                                     
             .ThenBy(r => r.RoomNumber)                                     
             .FirstOrDefault();

         if (suitableRoom == null)
         {
             _logger.LogWarning(
                 "ApproveBookingRequest: No suitable room found for BookingRequestId: {BookingRequestId}. Required capacity: {TotalGuests}. Number of available rooms in period (before capacity check): {AvailableRoomsInPeriodCount}. Checked rooms: [{AvailableRoomDetails}]",
                 bookingRequestToApprove.Id, 
                 bookingRequestToApprove.TotalGuests, 
                 availableRoomsInPeriod.Count,
                 string.Join(" | ", availableRoomsInPeriod.Select(r => $"ID:{r.Id},Cap:{r.Capacity}"))
                 );
             throw new BadRequestException($"متاسفانه، هیچ اتاق خالی با ظرفیت کافی ({bookingRequestToApprove.TotalGuests} نفر) برای تاریخ‌های درخواستی شما در هتل '{bookingRequestToApprove.Hotel.Name}' موجود نیست.");
         }
         _logger.LogInformation(
             "ApproveBookingRequest: Suitable room found for BookingRequestId: {BookingRequestId}. RoomId: {RoomId} (Number: {RoomNumber}), RoomCapacity: {RoomCapacity}, RequiredCapacity: {RequiredCapacity}",
             bookingRequestToApprove.Id, suitableRoom.Id, suitableRoom.RoomNumber, suitableRoom.Capacity, bookingRequestToApprove.TotalGuests);

         bookingRequestToApprove.AssignRoom(suitableRoom.Id, suitableRoom);

         var approvalComments = string.IsNullOrWhiteSpace(request.Comments) 
             ? $"تأیید شده توسط هتل. اتاق {suitableRoom.RoomNumber} (ظرفیت: {suitableRoom.Capacity}) تخصیص داده شد." 
             : $"{request.Comments} (اتاق {suitableRoom.RoomNumber}, ظرفیت: {suitableRoom.Capacity} تخصیص داده شد.)";
         
         // hotelUser که از قبل واکشی شده بود را اینجا استفاده می‌کنیم
         var userWhoApproved = await _unitOfWork.UserRepository.GetByIdAsync(request.HotelUserId);
         if(userWhoApproved == null) {  /* Handle error, should not happen if previous checks passed */
              _logger.LogError("User who approved the request (HotelUserId: {HotelUserId}) not found right before updating status.", request.HotelUserId);
              throw new ApplicationException("خطا در واکشی اطلاعات کاربر تأیید کننده.");
         }

         bookingRequestToApprove.UpdateStatus(BookingStatus.HotelApproved, request.HotelUserId, userWhoApproved, approvalComments);
         
         await _unitOfWork.SaveChangesAsync(cancellationToken);
         _logger.LogInformation(
             "ApproveBookingRequest: BookingRequestId: {BookingRequestId} approved successfully. Room {RoomId} (Number: {RoomNumber}) assigned. Status updated to {NewStatus}.", 
             bookingRequestToApprove.Id, suitableRoom.Id, suitableRoom.RoomNumber, BookingStatus.HotelApproved);

         var mainEmployeeUser = await _unitOfWork.UserRepository.GetByNationalCodeAsync(bookingRequestToApprove.RequestingEmployeeNationalCode);
         if (mainEmployeeUser != null && !string.IsNullOrEmpty(mainEmployeeUser.PhoneNumber))
         {
             try
             {
                 await _smsService.SendSmsAsync(mainEmployeeUser.PhoneNumber,
                     $"درخواست رزرو شما با کد رهگیری {bookingRequestToApprove.TrackingCode} برای هتل {bookingRequestToApprove.Hotel.Name} تأیید و اتاق شماره {suitableRoom.RoomNumber} به شما تخصیص داده شد.");
                 _logger.LogInformation(
                     "ApproveBookingRequest: Approval and room assignment SMS sent to {PhoneNumber} for booking {TrackingCode}.", 
                     mainEmployeeUser.PhoneNumber, bookingRequestToApprove.TrackingCode);
             }
             catch(Exception ex)
             {
                 _logger.LogError(ex, 
                     "ApproveBookingRequest: Failed to send approval and room assignment SMS for booking {TrackingCode} to {PhoneNumber}.", 
                     bookingRequestToApprove.TrackingCode, mainEmployeeUser.PhoneNumber);
             }
         }
         else
         {
             _logger.LogWarning(
                 "ApproveBookingRequest: Phone number for main employee (NationalCode: {NationalCode}) not found or empty for booking {TrackingCode}. Approval SMS not sent.", 
                 bookingRequestToApprove.RequestingEmployeeNationalCode, bookingRequestToApprove.TrackingCode);
         }
    }
}