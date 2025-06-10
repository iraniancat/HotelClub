using MediatR;

namespace HotelReservation.Application.Features.Rooms.Commands.CreateRoom;

public class CreateRoomCommand : IRequest<Guid>
{
    public string RoomNumber { get; set; }
    public int Capacity { get; set; }
    public decimal PricePerNight { get; set; }
    public Guid HotelId { get; set; }
    public bool IsActive { get; set; }
}
