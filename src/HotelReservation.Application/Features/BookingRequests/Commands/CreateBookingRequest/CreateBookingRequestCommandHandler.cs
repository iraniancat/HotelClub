// src/HotelReservation.Application/Features/BookingRequests/Commands/CreateBookingRequest/CreateBookingRequestCommandHandler.cs
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Application.Contracts.Infrastructure;
using HotelReservation.Application.DTOs.Booking; // مهم: برای CreateBookingRequestResponseDto
using HotelReservation.Application.Exceptions;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Enums;
using MediatR; // مهم: برای IRequestHandler
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HotelReservation.Application.Features.BookingRequests.Commands.CreateBookingRequest;

// این خط بسیار مهم است vv                          vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
public class CreateBookingRequestCommandHandler : IRequestHandler<CreateBookingRequestCommand, CreateBookingRequestResponseDto>
{
    // ... بدنه Handler که قبلاً تعریف کردیم ...
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISmsService _smsService;
    private readonly ILogger<CreateBookingRequestCommandHandler> _logger;
    private const decimal EmployeeAndDependentDiscount = 0.80m;
    private const decimal CompanionDiscount = 0.65m;
    private const string HotelUserRoleName = "HotelUser"; 
    private const string SuperAdminRoleName = "SuperAdmin";
    private const string ProvinceUserRoleName = "ProvinceUser";


     public CreateBookingRequestCommandHandler(
        IUnitOfWork unitOfWork,
        ISmsService smsService,
        ILogger<CreateBookingRequestCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CreateBookingRequestResponseDto> Handle(CreateBookingRequestCommand request, CancellationToken cancellationToken)
    {
         var submitterUser = await _unitOfWork.UserRepository.GetUserWithFullDetailsAsync(request.RequestSubmitterUserId);
         if (submitterUser == null)
         {
             throw new BadRequestException($"کاربر ثبت کننده با شناسه '{request.RequestSubmitterUserId}' یافت نشد.");
         }
         if (submitterUser.Role?.Name != SuperAdminRoleName && submitterUser.Role?.Name != ProvinceUserRoleName)
         {
             throw new BadRequestException("کاربر فعلی مجاز به ثبت درخواست رزرو برای دیگران نیست.");
         }

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
            request.RequestSubmitterUserId,
            submitterUser,
            request.Notes
        );
        bookingRequestEntity.UpdateStatus(BookingStatus.SubmittedToHotel, submitterUser.Id, submitterUser, "درخواست توسط مدیر/کاربر استان ثبت شد");

        foreach (var guestDto in request.Guests)
        {
            decimal discountPercentage;
            if (guestDto.NationalCode == mainEmployeeUser.NationalCode)
            {
                discountPercentage = EmployeeAndDependentDiscount;
            }
            else
            {
                var dependent = await _unitOfWork.DependentDataRepository
                                      .GetByEmployeeDataIdAndNationalCodeAsync(mainEmployeeUser.Id, guestDto.NationalCode);
                if (dependent != null)
                {
                    discountPercentage = EmployeeAndDependentDiscount;
                }
                else
                {
                    discountPercentage = CompanionDiscount;
                }
            }
            bookingRequestEntity.AddGuest(
                guestDto.FullName,
                guestDto.NationalCode,
                guestDto.RelationshipToEmployee,
                discountPercentage * 100 
            );
        }

        await _unitOfWork.BookingRequestRepository.AddAsync(bookingRequestEntity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrEmpty(mainEmployeeUser.PhoneNumber))
        {
            try
            {
               await _smsService.SendSmsAsync(mainEmployeeUser.PhoneNumber, 
                    $"درخواست رزرو شما با کد رهگیری {bookingRequestEntity.TrackingCode} برای هتل {hotel.Name} در تاریخ {request.CheckInDate:yyyy/MM/dd} ثبت شد.");
               _logger.LogInformation($"Booking confirmation SMS sent to {mainEmployeeUser.PhoneNumber} for tracking code {bookingRequestEntity.TrackingCode}.");
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, $"Failed to send SMS for booking {bookingRequestEntity.TrackingCode} to {mainEmployeeUser.PhoneNumber}.");
            }
        }
        else
        {
            _logger.LogWarning($"Phone number not found for employee (NationalCode: {mainEmployeeUser.NationalCode}) to send booking confirmation SMS for tracking code {bookingRequestEntity.TrackingCode}.");
        }

        return new CreateBookingRequestResponseDto 
        { 
            Id = bookingRequestEntity.Id, 
            TrackingCode = bookingRequestEntity.TrackingCode 
        };
    }
}