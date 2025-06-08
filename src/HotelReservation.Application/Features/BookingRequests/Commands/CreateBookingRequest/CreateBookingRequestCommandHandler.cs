using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Application.Contracts.Infrastructure;
using HotelReservation.Application.Contracts.Security; // <<-- برای ICurrentUserService
using HotelReservation.Application.DTOs.Booking;
using HotelReservation.Application.Exceptions;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HotelReservation.Application.Features.BookingRequests.Commands.CreateBookingRequest;

public class CreateBookingRequestCommandHandler : IRequestHandler<CreateBookingRequestCommand, CreateBookingRequestResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISmsService _smsService;
    private readonly ILogger<CreateBookingRequestCommandHandler> _logger;
    private readonly ICurrentUserService _currentUserService; // <<-- اضافه شد

    private const decimal EmployeeAndDependentDiscount = 0.80m;
    private const decimal CompanionDiscount = 0.65m;
    private const string SuperAdminRoleName = "SuperAdmin";
    private const string ProvinceUserRoleName = "ProvinceUser";

    public CreateBookingRequestCommandHandler(
        IUnitOfWork unitOfWork,
        ISmsService smsService,
        ILogger<CreateBookingRequestCommandHandler> logger,
        ICurrentUserService currentUserService) // <<-- اضافه شد
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService)); // <<-- اضافه شد
    }

    public async Task<CreateBookingRequestResponseDto> Handle(CreateBookingRequestCommand request, CancellationToken cancellationToken)
    {
        // ۱. دریافت شناسه کاربر ثبت کننده از ICurrentUserService
        var submitterUserId = _currentUserService.UserId;
        if (!submitterUserId.HasValue)
        {
            throw new UnauthorizedAccessException("اطلاعات کاربر ثبت کننده برای این عملیات در دسترس نیست.");
        }
        
        var submitterUser = await _unitOfWork.UserRepository.GetUserWithFullDetailsAsync(submitterUserId.Value);
        if (submitterUser == null)
        {
            throw new BadRequestException($"کاربر ثبت کننده با شناسه '{submitterUserId.Value}' یافت نشد.");
        }

        // ۲. بررسی نقش کاربر ثبت کننده
        if (submitterUser.Role?.Name != SuperAdminRoleName && submitterUser.Role?.Name != ProvinceUserRoleName)
        {
            throw new ForbiddenAccessException("کاربر فعلی مجاز به ثبت درخواست رزرو برای دیگران نیست.");
        }

        // ... بقیه منطق Handler که قبلاً داشتیم و حالا از submitterUserId.Value و submitterUser استفاده می‌کند ...
        var mainEmployeeUser = await _unitOfWork.UserRepository.GetByNationalCodeAsync(request.RequestingEmployeeNationalCode);
        if (mainEmployeeUser == null)
        {
            throw new BadRequestException($"کارمندی با کد ملی '{request.RequestingEmployeeNationalCode}' در سیستم یافت نشد.");
        }

        var hotel = await _unitOfWork.HotelRepository.GetByIdAsync(request.HotelId);
        if (hotel == null)
        {
            throw new NotFoundException(nameof(Hotel), request.HotelId);
        }
        
        var bookingRequestEntity = new BookingRequest(
            request.RequestingEmployeeNationalCode,
            request.BookingPeriod,
            request.CheckInDate,
            request.CheckOutDate,
            request.Guests.Count,
            request.HotelId,
            hotel,
            submitterUserId.Value, // استفاده از شناسه کاربر از ICurrentUserService
            submitterUser,
            request.Notes
        );
        bookingRequestEntity.UpdateStatus(BookingStatus.SubmittedToHotel, submitterUserId.Value, submitterUser, "درخواست توسط مدیر/کاربر استان ثبت شد");

        // پردازش مهمانان و محاسبه تخفیف...
        foreach (var guestDto in request.Guests)
        {
            decimal discountPercentage;
            if (guestDto.NationalCode == mainEmployeeUser.NationalCode)
            {
                discountPercentage = EmployeeAndDependentDiscount;
            }
            else
            {
                var dependent = await _unitOfWork.DependentDataRepository.GetByEmployeeDataIdAndNationalCodeAsync(mainEmployeeUser.Id, guestDto.NationalCode);
                discountPercentage = (dependent != null) ? EmployeeAndDependentDiscount : CompanionDiscount;
            }
            bookingRequestEntity.AddGuest(guestDto.FullName, guestDto.NationalCode, guestDto.RelationshipToEmployee, discountPercentage * 100);
        }

        await _unitOfWork.BookingRequestRepository.AddAsync(bookingRequestEntity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ارسال SMS...
        if (!string.IsNullOrEmpty(mainEmployeeUser.PhoneNumber))
        {
            try
            {
                await _smsService.SendSmsAsync(mainEmployeeUser.PhoneNumber, 
                     $"درخواست رزرو شما با کد رهگیری {bookingRequestEntity.TrackingCode} برای هتل {hotel.Name} در تاریخ {request.CheckInDate:yyyy/MM/dd} ثبت شد.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SMS for booking {TrackingCode}.", bookingRequestEntity.TrackingCode);
            }
        }
        
        return new CreateBookingRequestResponseDto 
        { 
            Id = bookingRequestEntity.Id, 
            TrackingCode = bookingRequestEntity.TrackingCode 
        };
    }
}