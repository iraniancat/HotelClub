using HotelReservation.Application.Contracts.Infrastructure;
using HotelReservation.Application.Contracts.Security;
using HotelReservation.Application.DTOs.Booking;
using HotelReservation.Application.Exceptions;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelReservation.Application.Features.BookingRequests.Commands.SubmitByEmployee;

// SubmitBookingByEmployeeCommandHandler.cs
public class SubmitBookingByEmployeeCommandHandler : IRequestHandler<SubmitBookingByEmployeeCommand, CreateBookingRequestResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISmsService _smsService;
    private readonly ILogger<SubmitBookingByEmployeeCommandHandler> _logger;
    private readonly ICurrentUserService _currentUserService; // <<-- اضافه شد

    private const decimal EmployeeAndDependentDiscount = 0.80m;
    private const decimal CompanionDiscount = 0.65m;

    public SubmitBookingByEmployeeCommandHandler(IUnitOfWork unitOfWork,
        ISmsService smsService,
        ILogger<SubmitBookingByEmployeeCommandHandler> logger,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<CreateBookingRequestResponseDto> Handle(SubmitBookingByEmployeeCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        if (!currentUserId.HasValue) throw new UnauthorizedAccessException();

        var bookingPeriod = await _unitOfWork.BookingPeriodRepository.GetByIdAsync(request.BookingPeriodId);
        if (bookingPeriod == null || !bookingPeriod.IsActive) throw new BadRequestException("دوره زمانی انتخاب شده معتبر نیست.");

        // <<-- اعتبارسنجی جدید: تاریخ‌ها باید در بازه دوره زمانی باشند -->>
        if (request.CheckInDate < bookingPeriod.StartDate || request.CheckOutDate > bookingPeriod.EndDate)
        {
            throw new BadRequestException($"تاریخ‌های انتخابی باید در بازه دوره زمانی '{bookingPeriod.Name}' باشند.");
        }

        var currentUser = await _unitOfWork.UserRepository.GetByIdAsync(currentUserId.Value, asNoTracking: false);
        if (currentUser == null || string.IsNullOrWhiteSpace(currentUser.NationalCode))
        {
            throw new BadRequestException("اطلاعات کارمندی شما برای ثبت درخواست ناقص است.");
        }

        
        // <<-- شروع منطق جدید: بررسی لیست سیاه برای کاربر فعلی -->>
        if (currentUser.IsBlacklisted)
        {
            if (currentUser.BlacklistEndDate.HasValue && currentUser.BlacklistEndDate.Value < DateTime.UtcNow)
            {
                _logger.LogInformation("User {UserId} was blacklisted, but the restriction period has ended.", currentUserId);
            }
            else
            {
                _logger.LogWarning("A blacklisted user attempted to create a booking: {UserId}", currentUserId);
                var expiryMessage = currentUser.BlacklistEndDate.HasValue 
                    ? $"تا تاریخ {currentUser.BlacklistEndDate.Value:yyyy/MM/dd}" 
                    : "به صورت دائمی";
                throw new BadRequestException($"شما به دلیل '{currentUser.BlacklistReason}' {expiryMessage} در لیست سیاه قرار دارید و امکان ثبت رزرو برای شما وجود ندارد.");
            }
        }
        // <<-- پایان منطق جدید -->>

        var mainEmployee = await _unitOfWork.UserRepository.GetByNationalCodeAsync(request.RequestingEmployeeNationalCode, asNoTracking: true);
        if (mainEmployee == null || string.IsNullOrWhiteSpace(mainEmployee.ProvinceCode))
        {
            throw new BadRequestException("اطلاعات استان برای کارمند اصلی درخواست یافت نشد.");
        }
        var existingActiveBookings = await _unitOfWork.BookingRequestRepository.GetAsync(
           b => b.RequestingEmployeeNationalCode == request.RequestingEmployeeNationalCode &&
                (b.Status == BookingStatus.SubmittedToHotel || b.Status == BookingStatus.HotelApproved) &&
                (b.CheckInDate < request.CheckOutDate && b.CheckOutDate > request.CheckInDate)
       );

        if (existingActiveBookings.Any())
        {
            _logger.LogWarning("Attempted to create a duplicate booking for employee {NationalCode} with overlapping dates.", request.RequestingEmployeeNationalCode);
            throw new BadRequestException("برای این کارمند در بازه زمانی مشابه، یک رزرو فعال (در انتظار یا تایید شده) از قبل وجود دارد.");
        }
        // <<-- شروع منطق جدید بررسی محدودیت استان -->>
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
                br.HotelId == request.HotelId &&
                br.Status == BookingStatus.HotelApproved &&
                br.RequestingEmployee.ProvinceCode == employeeProvinceCode &&
                (br.CheckInDate < request.CheckOutDate && br.CheckOutDate > request.CheckInDate),
            cancellationToken);

        if (otherApprovedBookingsForProvince >= quota.RoomLimit)
        {
            throw new BadRequestException($"سهمیه رزرو اتاق برای استان شما ({quota.RoomLimit} اتاق) در این هتل و در تاریخ‌های درخواستی تکمیل شده است.");
        }
        var hotel = await _unitOfWork.HotelRepository.GetByIdAsync(request.HotelId, asNoTracking: false);
        if (hotel == null) throw new NotFoundException(nameof(Hotel), request.HotelId);


        // <<-- اعتبارسنجی جدید برای تاریخ‌ها -->>
        if (request.CheckInDate < bookingPeriod.StartDate || request.CheckOutDate > bookingPeriod.EndDate)
        {
            throw new BadRequestException($"تاریخ ورود و خروج باید در بازه دوره زمانی انتخاب شده ({bookingPeriod.StartDate:yyyy/MM/dd} تا {bookingPeriod.EndDate:yyyy/MM/dd}) باشد.");
        }



        var period = await _unitOfWork.BookingPeriodRepository.GetByIdAsync(request.BookingPeriodId, asNoTracking: false);
        // ... (بررسی null بودن‌ها) ...

        var bookingRequest = new BookingRequest(
            currentUser.NationalCode, // کارمند اصلی، خود کاربر لاگین کرده است
            request.BookingPeriodId,
            request.CheckInDate, request.CheckOutDate,
            request.Guests.Count,
            request.HotelId, hotel,
            currentUserId.Value, currentUser, // ثبت کننده نیز خود کاربر است
            request.Notes
        );

        // <<-- وضعیت اولیه: در انتظار تأیید استان -->>
        bookingRequest.UpdateStatus(BookingStatus.AwaitingProvinceApproval, currentUserId.Value, "درخواست توسط کارمند ثبت شد.");

        // <<-- شروع منطق کلیدی: افزودن مهمانان و محاسبه تخفیف -->>
        _logger.LogInformation("Processing {GuestCount} guests for the new personal booking request.", request.Guests.Count);
        foreach (var guestDto in request.Guests)
        {
            decimal discountPercentage;
            if (guestDto.NationalCode == currentUser.NationalCode)
            {
                discountPercentage = EmployeeAndDependentDiscount;
            }
            else
            {
                // برای کارایی بهتر، می‌توان وابستگان را یکجا با کاربر خواند
                var dependent = await _unitOfWork.DependentDataRepository.GetByEmployeeDataIdAndNationalCodeAsync(currentUser.Id, guestDto.NationalCode);
                discountPercentage = (dependent != null) ? EmployeeAndDependentDiscount : CompanionDiscount;
            }
            bookingRequest.AddGuest(guestDto.FullName, guestDto.NationalCode, guestDto.RelationshipToEmployee, discountPercentage * 100);
            _logger.LogInformation("Guest '{GuestFullName}' with discount {Discount}% added to the personal booking entity.", guestDto.FullName, discountPercentage * 100);
        }
        // <<-- پایان منطق کلیدی -->>

        await _unitOfWork.BookingRequestRepository.AddAsync(bookingRequest);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var mainEmployeeUser = await _unitOfWork.UserRepository.GetByNationalCodeAsync(bookingRequest.RequestingEmployeeNationalCode, true);
        if (mainEmployeeUser != null && !string.IsNullOrEmpty(mainEmployeeUser.PhoneNumber))
        {
            try
            {
                string message = $@"درخواست رزرو شما با کد رهگیری {bookingRequest.TrackingCode} ثبت و به استان جهت تایید ارسال گردید:"
                                + Environment.NewLine
                                + $@"تاریخ ورود: {bookingRequest.CheckInDate}"
                                + Environment.NewLine
                                + $@"تاریخ خروج: {bookingRequest.CheckOutDate}"
                                + Environment.NewLine
                                + $@"تعداد مهمانان: {bookingRequest.TotalGuests}";
                await _smsService.SendSmsAsync(mainEmployeeUser.PhoneNumber, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send cancellation SMS for booking {TrackingCode}.", bookingRequest.TrackingCode);
            }
        }

        return new CreateBookingRequestResponseDto { Id = bookingRequest.Id, TrackingCode = bookingRequest.TrackingCode };
    }
}
