// مسیر: src/HotelReservation.Application/Features/BookingRequests/Commands/CreateBookingRequest/CreateBookingRequestCommandHandler.cs
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Application.Contracts.Security;
using HotelReservation.Application.DTOs.Booking;
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

namespace HotelReservation.Application.Features.BookingRequests.Commands.CreateBookingRequest;

public class CreateBookingRequestCommandHandler : IRequestHandler<CreateBookingRequestCommand, CreateBookingRequestResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateBookingRequestCommandHandler> _logger;

    public CreateBookingRequestCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<CreateBookingRequestCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<CreateBookingRequestResponseDto> Handle(CreateBookingRequestCommand request, CancellationToken cancellationToken)
    {
        var submitterUserId = _currentUserService.UserId;
        if (!submitterUserId.HasValue)
        {
            throw new UnauthorizedAccessException("اطلاعات کاربر ثبت کننده در دسترس نیست.");
        }

        var submitterUser = await _unitOfWork.UserRepository.GetByIdAsync(submitterUserId.Value, asNoTracking: false);
        if (submitterUser == null) throw new BadRequestException("کاربر ثبت کننده یافت نشد.");

        var mainEmployee = await _unitOfWork.UserRepository.GetByNationalCodeAsync(request.RequestingEmployeeNationalCode, asNoTracking: true);
        if (mainEmployee == null || string.IsNullOrWhiteSpace(mainEmployee.ProvinceCode))
        {
            throw new BadRequestException("اطلاعات استان برای کارمند اصلی درخواست یافت نشد.");
        }

        var hotel = await _unitOfWork.HotelRepository.GetByIdAsync(request.HotelId, asNoTracking: false);
        if (hotel == null) throw new NotFoundException(nameof(Hotel), request.HotelId);

        var bookingPeriod = await _unitOfWork.BookingPeriodRepository.GetByIdAsync(request.BookingPeriodId, asNoTracking: false);
        if (bookingPeriod == null || !bookingPeriod.IsActive) throw new BadRequestException("دوره زمانی انتخاب شده معتبر نیست.");
        if (request.CheckInDate < bookingPeriod.StartDate || request.CheckOutDate > bookingPeriod.EndDate) throw new BadRequestException("تاریخ‌ها باید در بازه دوره زمانی باشند.");


        // <<-- شروع منطق بررسی محدودیت استان در زمان ایجاد -->>
        var employeeProvinceCode = mainEmployee.ProvinceCode;
        var quota = (await _unitOfWork.ProvinceHotelQuotaRepository
            .GetAsync(q => q.HotelId == request.HotelId && q.ProvinceCode == employeeProvinceCode))
            .FirstOrDefault();
        
        if (quota == null || quota.RoomLimit <= 0)
        {
            throw new BadRequestException($"هیچ سهمیه‌ای برای استان '{mainEmployee.ProvinceName}' در این هتل تعریف نشده است.");
        }

         var otherApprovedBookingsForProvince = await _unitOfWork.BookingRequestRepository.GetQueryable()
            .CountAsync(br => 
                br.Id != Guid.Empty && // یک شرط برای اطمینان
                br.HotelId == request.HotelId &&
                br.Status == BookingStatus.HotelApproved &&
                br.RequestingEmployee.ProvinceCode == mainEmployee.ProvinceCode && // <<-- حالا به راحتی قابل دسترسی است
                (br.CheckInDate < request.CheckOutDate && br.CheckOutDate > request.CheckInDate),
            cancellationToken);


        _logger.LogInformation("Quota check for Province {ProvinceCode} at Hotel {HotelId}: Limit is {Limit}, Currently approved bookings are {Count}",
            employeeProvinceCode, request.HotelId, quota.RoomLimit, otherApprovedBookingsForProvince);

        if (otherApprovedBookingsForProvince >= quota.RoomLimit)
        {
            throw new BadRequestException($"سهمیه رزرو اتاق برای استان شما در این هتل ({quota.RoomLimit} اتاق) در تاریخ‌های درخواستی تکمیل شده است.");
        }
        // <<-- پایان منطق بررسی محدودیت استان -->>
        
        var bookingRequestEntity = new BookingRequest(
            request.RequestingEmployeeNationalCode,            
            request.BookingPeriodId, bookingPeriod,
            request.CheckInDate,
            request.CheckOutDate,
            request.Guests.Count,
            request.HotelId, hotel,
            submitterUserId.Value, submitterUser,
            request.Notes
        );
        
        // ... (بقیه منطق ایجاد مهمان، ذخیره‌سازی و ارسال SMS) ...

        await _unitOfWork.BookingRequestRepository.AddAsync(bookingRequestEntity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateBookingRequestResponseDto { Id = bookingRequestEntity.Id, TrackingCode = bookingRequestEntity.TrackingCode };
    }
}
