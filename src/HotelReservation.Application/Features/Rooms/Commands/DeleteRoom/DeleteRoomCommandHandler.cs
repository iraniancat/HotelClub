using HotelReservation.Application.Contracts.Security;
using HotelReservation.Application.Exceptions;
using HotelReservation.Domain.Entities;
using MediatR;

namespace HotelReservation.Application.Features.Rooms.Commands.DeleteRoom
{
    public class DeleteRoomCommandHandler : IRequestHandler<DeleteRoomCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        public DeleteRoomCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }
        public async Task Handle(DeleteRoomCommand request, CancellationToken cancellationToken)
        {
            var roomToDelete = await _unitOfWork.RoomRepository.GetByIdAsync(request.Id, asNoTracking: false);
            if (roomToDelete == null) throw new NotFoundException(nameof(Room), request.Id);

            if (_currentUserService.UserRole == "HotelUser" && _currentUserService.HotelId != roomToDelete.HotelId)
                throw new ForbiddenAccessException("شما فقط می‌توانید اتاق‌های هتل خود را حذف کنید.");

            // TODO: بررسی اینکه آیا رزرو فعالی برای این اتاق وجود دارد یا خیر

            await _unitOfWork.RoomRepository.DeleteAsync(roomToDelete);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}