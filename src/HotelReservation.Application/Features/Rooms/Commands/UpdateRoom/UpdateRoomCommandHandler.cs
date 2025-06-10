using HotelReservation.Application.Contracts.Security;
using HotelReservation.Application.Exceptions;
using HotelReservation.Domain.Entities;
using MediatR;
namespace HotelReservation.Application.Features.Rooms.Commands.UpdateRoom;

public class UpdateRoomCommandHandler : IRequestHandler<UpdateRoomCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    public UpdateRoomCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UpdateRoomCommand request, CancellationToken cancellationToken)
    {
        var roomToUpdate = await _unitOfWork.RoomRepository.GetByIdAsync(request.Id, asNoTracking: false);
        if (roomToUpdate == null) throw new NotFoundException(nameof(Room), request.Id);

        if (_currentUserService.UserRole == "HotelUser" && _currentUserService.HotelId != roomToUpdate.HotelId)
            throw new ForbiddenAccessException("شما فقط می‌توانید اتاق‌های هتل خود را ویرایش کنید.");

        roomToUpdate.UpdateDetails(request.RoomNumber, request.Capacity, request.PricePerNight, request.IsActive);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}