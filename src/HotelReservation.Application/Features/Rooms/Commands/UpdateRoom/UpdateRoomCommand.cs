using MediatR;
namespace HotelReservation.Application.Features.Rooms.Commands.UpdateRoom;

public class UpdateRoomCommand : IRequest
{
    public Guid Id { get; set; }
    public string RoomNumber { get; set; }
    public int Capacity { get; set; }
    public decimal PricePerNight { get; set; }
    public bool IsActive { get; set; }
}
