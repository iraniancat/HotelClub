using HotelReservation.Application.DTOs.Room;
using MediatR;

namespace HotelReservation.Application.Features.Rooms.Queries.GetRoomsByHotel
{
    public class GetRoomsByHotelQuery : IRequest<IEnumerable<RoomDto>> { public Guid HotelId { get; set; } }
}