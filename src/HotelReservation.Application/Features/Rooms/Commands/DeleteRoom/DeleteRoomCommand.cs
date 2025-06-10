using MediatR;

namespace HotelReservation.Application.Features.Rooms.Commands.DeleteRoom
{
    public class DeleteRoomCommand : IRequest { public Guid Id { get; set; } }
}