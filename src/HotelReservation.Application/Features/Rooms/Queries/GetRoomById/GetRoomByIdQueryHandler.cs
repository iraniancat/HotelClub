using HotelReservation.Application.Contracts.Security;
using HotelReservation.Application.DTOs.Room;
using HotelReservation.Application.Exceptions;
using HotelReservation.Domain.Entities;
using MediatR;

namespace HotelReservation.Application.Features.Rooms.Queries.GetRoomById
{
    public class GetRoomByIdQueryHandler : IRequestHandler<GetRoomByIdQuery, RoomDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        public GetRoomByIdQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<RoomDto> Handle(GetRoomByIdQuery request, CancellationToken cancellationToken)
        {
            var room = await _unitOfWork.RoomRepository.GetByIdAsync(request.Id);
            if (room == null) throw new NotFoundException(nameof(Room), request.Id);

            if (_currentUserService.UserRole == "HotelUser" && _currentUserService.HotelId != room.HotelId)
                throw new ForbiddenAccessException("شما فقط می‌توانید اتاق‌های هتل خود را مشاهده کنید.");

            return new RoomDto { /* map entity to DTO */ };
        }
    }
}