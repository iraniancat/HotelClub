
// 3. فایل جدید: src/HotelReservation.Application/Features/UserManagement/Commands/UpdateUserBlacklist/UpdateUserBlacklistCommandHandler.cs
using HotelReservation.Application.Exceptions;
using HotelReservation.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HotelReservation.Application.Features.UserManagement.Commands.UpdateUserBlacklist;

public class UpdateUserBlacklistCommandHandler : IRequestHandler<UpdateUserBlacklistCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateUserBlacklistCommandHandler> _logger;

    public UpdateUserBlacklistCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdateUserBlacklistCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(UpdateUserBlacklistCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(request.UserId, asNoTracking: false);
        if (user == null)
        {
            throw new NotFoundException(nameof(User), request.UserId);
        }

        if (request.IsBlacklisted && string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new BadRequestException("برای افزودن کاربر به لیست سیاه، ارائه دلیل الزامی است.");
        }

        user.UpdateBlacklistStatus(request.IsBlacklisted, request.Reason, request.EndDate);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Blacklist status for user {UserId} has been updated.", request.UserId);
    }
}