namespace HotelReservation.Application.Features.BookingRequests.Commands.InitialApprove;

using HotelReservation.Application.Contracts.Infrastructure;
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Application.Contracts.Security;
using HotelReservation.Application.Exceptions;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

public class InitialApproveCommandHandler : IRequestHandler<InitialApproveCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<InitialApproveCommandHandler> _logger;
    private readonly ISmsService _smsService;

    public InitialApproveCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<InitialApproveCommandHandler> logger,
        ISmsService smsService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
        _smsService = smsService;
    }

    public async Task Handle(InitialApproveCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        if (!currentUserId.HasValue)
        {
            throw new UnauthorizedAccessException("کاربر برای انجام این عملیات احراز هویت نشده است.");
        }
        var bookingRequest = await _unitOfWork.BookingRequestRepository.GetByIdAsync(request.BookingRequestId, asNoTracking: false);
        if (bookingRequest == null)
        {
            throw new NotFoundException(nameof(BookingRequest), request.BookingRequestId);
        }

        if (bookingRequest.Status != BookingStatus.AwaitingProvinceApproval)
        {
            throw new BadRequestException("این درخواست در وضعیت قابل تأیید اولیه نیست.");
        }

        // TODO: بررسی مجوز دقیق‌تر (آیا کاربر مدیر استان مربوطه است؟)


        var historyEntry = bookingRequest.UpdateStatus(BookingStatus.SubmittedToHotel, currentUserId.Value, "توسط مدیر استان/ارشد تأیید شد.");

        if (historyEntry != null)
        {
            await _unitOfWork.BookingStatusHistoryRepository.AddAsync(historyEntry);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("BookingRequest {Id} initially approved by User {UserId}", request.BookingRequestId, _currentUserService.UserId);
        var mainEmployeeUser = await _unitOfWork.UserRepository.GetByNationalCodeAsync(bookingRequest.RequestingEmployeeNationalCode, true);
        if (mainEmployeeUser != null && !string.IsNullOrEmpty(mainEmployeeUser.PhoneNumber))
        {
            try
            {
                await _smsService.SendSmsAsync(mainEmployeeUser.PhoneNumber,
                     $"درخواست رزرو شما با کد رهگیری {bookingRequest.TrackingCode} توسط استان تایید شد و به هتل {bookingRequest.Hotel.Name} ارسال گردید.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send cancellation SMS for booking {TrackingCode}.", bookingRequest.TrackingCode);
            }
        }
    }
}