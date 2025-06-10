using HotelReservation.Application.Contracts.Security;
using HotelReservation.Application.DTOs.Room;
using HotelReservation.Application.Exceptions;
using HotelReservation.Domain.Entities;
using MediatR;

namespace HotelReservation.Application.Features.Rooms.Queries.GetRoomsByHotel
{
    public class GetRoomsByHotelQueryHandler : IRequestHandler<GetRoomsByHotelQuery, IEnumerable<RoomDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        public GetRoomsByHotelQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }
        public async Task<IEnumerable<RoomDto>> Handle(GetRoomsByHotelQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserRole == "HotelUser" && _currentUserService.HotelId != request.HotelId)
                throw new ForbiddenAccessException("شما فقط می‌توانید اتاق‌های هتل خود را مشاهده کنید.");

            // <<-- اصلاح کلیدی: Include کردن Hotel برای دسترسی به نام آن -->>
            var rooms = await _unitOfWork.RoomRepository
                .GetAsyncWithIncludes(r => r.HotelId == request.HotelId, includes: new List<System.Linq.Expressions.Expression<Func<Room, object>>> { r => r.Hotel });

            return rooms.Select(r => new RoomDto
            {
                Id = r.Id,
                RoomNumber = r.RoomNumber,
                Capacity = r.Capacity,
                PricePerNight = r.PricePerNight,
                HotelId = r.HotelId,
                HotelName = r.Hotel.Name, // <<-- حالا r.Hotel دیگر null نیست
                IsActive = r.IsActive
            }).OrderBy(r => r.RoomNumber).ToList();
        }
    }
}