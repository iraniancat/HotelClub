// مسیر: src/HotelReservation.Application/Features/BookingRequests/Commands/CreateBookingRequest/CreateBookingRequestCommandHandler.cs
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Application.Contracts.Infrastructure;
using HotelReservation.Application.Contracts.Security;
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
    private readonly ICurrentUserService _currentUserService;

    // ... (سازنده کامل با تمام تزریق‌ها) ...
    public CreateBookingRequestCommandHandler(
        IUnitOfWork unitOfWork, ISmsService smsService, 
        ILogger<CreateBookingRequestCommandHandler> logger, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _smsService = smsService;
        _logger = logger;
        _currentUserService = currentUserService;
    }


    public async Task<CreateBookingRequestResponseDto> Handle(CreateBookingRequestCommand request, CancellationToken cancellationToken)
    {
        var submitterUserId = _currentUserService.UserId;
        if (!submitterUserId.HasValue)
        {
            throw new UnauthorizedAccessException("اطلاعات کاربر ثبت کننده در دسترس نیست.");
        }

        // واکشی تمام موجودیت‌های مرتبط با ردیابی فعال (asNoTracking: false)
        var submitterUser = await _unitOfWork.UserRepository.GetUserWithFullDetailsAsync(submitterUserId.Value, asNoTracking: false);
        if (submitterUser == null) throw new BadRequestException($"کاربر ثبت کننده با شناسه '{submitterUserId.Value}' یافت نشد.");

        var mainEmployeeUser = await _unitOfWork.UserRepository.GetByNationalCodeAsync(request.RequestingEmployeeNationalCode, asNoTracking: false);
        if (mainEmployeeUser == null) throw new BadRequestException($"کارمندی با کد ملی '{request.RequestingEmployeeNationalCode}' یافت نشد.");

        var hotel = await _unitOfWork.HotelRepository.GetByIdAsync(request.HotelId, asNoTracking: false);
        if (hotel == null) throw new NotFoundException(nameof(Hotel), request.HotelId);

        var bookingPeriod = await _unitOfWork.BookingPeriodRepository.GetByIdAsync(request.BookingPeriodId, asNoTracking: false);
        if (bookingPeriod == null || !bookingPeriod.IsActive)
        {
            throw new BadRequestException("دوره زمانی انتخاب شده معتبر یا فعال نیست.");
        }

        // ... (اعتبارسنجی تاریخ‌ها و سایر موارد همانند قبل) ...
        if (request.CheckInDate < bookingPeriod.StartDate || request.CheckOutDate > bookingPeriod.EndDate)
        {
            throw new BadRequestException($"تاریخ‌ها باید در بازه دوره زمانی باشند.");
        }
        
        // حالا ایجاد موجودیت BookingRequest با موجودیت‌های ردیابی شده
        var bookingRequestEntity = new BookingRequest(
            request.RequestingEmployeeNationalCode,
            request.BookingPeriodId, bookingPeriod,
            request.CheckInDate,
            request.CheckOutDate,
            request.Guests.Count,
            request.HotelId,
            hotel,
            submitterUserId.Value,
            submitterUser,
            request.Notes
        );
        bookingRequestEntity.UpdateStatus(BookingStatus.SubmittedToHotel, submitterUserId.Value,  "درخواست ثبت شد.");
        
        // ... (پردازش مهمانان و محاسبه تخفیف همانند قبل) ...
        foreach (var guestDto in request.Guests)
        {
            decimal discountPercentage;
            // واکشی وابستگان نیز باید ردیابی شود اگر قرار است به گراف موجودیت اضافه شوند، اما اینجا فقط برای چک کردن استفاده می‌شود، پس AsNoTracking مناسب است
            var dependent = await _unitOfWork.DependentDataRepository.GetByEmployeeDataIdAndNationalCodeAsync(mainEmployeeUser.Id, guestDto.NationalCode);

            if (guestDto.NationalCode == mainEmployeeUser.NationalCode || dependent != null)
            {
                discountPercentage = 0.80m;
            }
            else
            {
                discountPercentage = 0.65m;
            }
            bookingRequestEntity.AddGuest(guestDto.FullName, guestDto.NationalCode, guestDto.RelationshipToEmployee, discountPercentage * 100);
        }


        await _unitOfWork.BookingRequestRepository.AddAsync(bookingRequestEntity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ... (ارسال SMS همانند قبل) ...
        
        return new CreateBookingRequestResponseDto 
        { 
            Id = bookingRequestEntity.Id, 
            TrackingCode = bookingRequestEntity.TrackingCode 
        };
    }
}
