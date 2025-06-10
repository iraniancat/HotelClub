using HotelReservation.Application.Contracts.Security;
using HotelReservation.Application.Exceptions;

using HotelReservation.Domain.Entities;
using MediatR;
namespace HotelReservation.Application.Features.Rooms.Commands.CreateRoom;

public class CreateRoomCommandHandler : IRequestHandler<CreateRoomCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    public CreateRoomCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateRoomCommand request, CancellationToken cancellationToken)
    {
        var hotel = await _unitOfWork.HotelRepository.GetByIdAsync(request.HotelId, asNoTracking: false);
        if (hotel == null) throw new NotFoundException(nameof(Hotel), request.HotelId);

        if (_currentUserService.UserRole == "HotelUser" && _currentUserService.HotelId != request.HotelId)
            throw new ForbiddenAccessException("شما فقط می‌توانید برای هتل خودتان اتاق ایجاد کنید.");

        var room = new Room(request.RoomNumber, request.Capacity, request.PricePerNight, request.HotelId, request.IsActive);
        await _unitOfWork.RoomRepository.AddAsync(room);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return room.Id;
    }
}