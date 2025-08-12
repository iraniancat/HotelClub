namespace HotelReservation.Application.Features.BookingRequests.Commands.InitialApprove;

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

    public InitialApproveCommandHandler(
        IUnitOfWork unitOfWork, 
        ICurrentUserService currentUserService,
        ILogger<InitialApproveCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task Handle(InitialApproveCommand request, CancellationToken cancellationToken)
    {
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

        bookingRequest.UpdateStatus(BookingStatus.SubmittedToHotel, _currentUserService.UserId.Value, "توسط مدیر استان/ارشد تأیید شد.");
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("BookingRequest {Id} initially approved by User {UserId}", request.BookingRequestId, _currentUserService.UserId);
    }
}