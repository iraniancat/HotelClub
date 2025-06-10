using HotelReservation.Application.DTOs.Room;
using MediatR;

namespace HotelReservation.Application.Features.Rooms.Queries.GetRoomById
{
    public class GetRoomByIdQuery : IRequest<RoomDto> { public Guid Id { get; set; } }
}