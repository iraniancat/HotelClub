namespace HotelReservation.Application.Features.BookingRequests.Commands.InitialReject;

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

public class InitialRejectCommandHandler : IRequestHandler<InitialRejectCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<InitialRejectCommandHandler> _logger;
    private readonly ISmsService _smsService;


    public InitialRejectCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<InitialRejectCommandHandler> logger,
        ISmsService smsService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
        _smsService = smsService;
    }

    public async Task Handle(InitialRejectCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        if (!currentUserId.HasValue) throw new UnauthorizedAccessException();
        var bookingRequest = await _unitOfWork.BookingRequestRepository.GetByIdAsync(request.BookingRequestId, asNoTracking: false);
        if (bookingRequest == null)
        {
            throw new NotFoundException(nameof(BookingRequest), request.BookingRequestId);
        }
        if (bookingRequest.Status != BookingStatus.AwaitingProvinceApproval) throw new BadRequestException("این درخواست در وضعیت قابل رد کردن اولیه نیست.");

        // TODO: بررسی مجوز دقیق‌تر

        var reason = $"رد شده توسط مدیر استان/ارشد. دلیل: {request.RejectionReason}";
        var historyEntry = bookingRequest.UpdateStatus(BookingStatus.ProvinceRejected, currentUserId.Value, reason);

        if (historyEntry != null)
        {
            await _unitOfWork.BookingStatusHistoryRepository.AddAsync(historyEntry);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("BookingRequest {Id} initially rejected by User {UserId}", request.BookingRequestId, _currentUserService.UserId);
         var mainEmployeeUser = await _unitOfWork.UserRepository.GetByNationalCodeAsync(bookingRequest.RequestingEmployeeNationalCode, true);
         if (mainEmployeeUser != null && !string.IsNullOrEmpty(mainEmployeeUser.PhoneNumber))
        {
            try
            {
                await _smsService.SendSmsAsync(mainEmployeeUser.PhoneNumber,
                     $"درخواست رزرو شما با کد رهگیری {bookingRequest.TrackingCode}  توسط استان به علت {request.RejectionReason} رد شد.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send cancellation SMS for booking {TrackingCode}.", bookingRequest.TrackingCode);
            }
        }
    }
}
