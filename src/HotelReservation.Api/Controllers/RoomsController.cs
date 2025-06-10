using HotelReservation.Application.DTOs.Room;
using HotelReservation.Application.Features.Rooms.Commands.CreateRoom;
using HotelReservation.Application.Features.Rooms.Commands.DeleteRoom;
using HotelReservation.Application.Features.Rooms.Commands.UpdateRoom;
using HotelReservation.Application.Features.Rooms.Queries.GetRoomById;
using HotelReservation.Application.Features.Rooms.Queries.GetRoomsByHotel;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservation.Api.Controllers;

[ApiController]
[Route("api/rooms")]
[Authorize]
public class RoomsController : ControllerBase
{
    private readonly IMediator _mediator;
    public RoomsController(IMediator mediator) => _mediator = mediator;

    // GET: api/rooms/by-hotel/{hotelId}
    [HttpGet("by-hotel/{hotelId:guid}")]
    [Authorize(Roles = "SuperAdmin,HotelUser")]
    public async Task<IActionResult> GetRoomsByHotel(Guid hotelId)
    {
        var query = new GetRoomsByHotelQuery { HotelId = hotelId };
        var rooms = await _mediator.Send(query);
        return Ok(rooms);
    }

    // GET: api/rooms/{id}
    [HttpGet("{id:guid}", Name = "GetRoomById")]
    [Authorize(Roles = "SuperAdmin,HotelUser")]
    public async Task<IActionResult> GetRoomById(Guid id)
    {
        var query = new GetRoomByIdQuery { Id = id }; // Query باید ایجاد شود
        var room = await _mediator.Send(query);
        return room != null ? Ok(room) : NotFound();
    }

    // POST: api/rooms
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,HotelUser")]
    public async Task<IActionResult> CreateRoom([FromBody] CreateOrUpdateRoomDto dto)
    {
        if (!dto.HotelId.HasValue)
            return BadRequest("شناسه هتل برای ایجاد اتاق الزامی است.");
        var command = new CreateRoomCommand
        {
            RoomNumber = dto.RoomNumber,
            Capacity = dto.Capacity,
            PricePerNight = dto.PricePerNight,
            HotelId = dto.HotelId.Value,
            IsActive = dto.IsActive
        };
        var roomId = await _mediator.Send(command);
        return CreatedAtAction(nameof(Application.Features.Rooms.Queries.GetRoomById), new { id = roomId }, new { id = roomId });
    }

    // PUT: api/rooms/{id}
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,HotelUser")]
    public async Task<IActionResult> UpdateRoom(Guid id, [FromBody] CreateOrUpdateRoomDto dto)
    {
        var command = new UpdateRoomCommand
        {
            Id = id,
            RoomNumber = dto.RoomNumber,
            Capacity = dto.Capacity,
            PricePerNight = dto.PricePerNight,
            IsActive = dto.IsActive
        };
        await _mediator.Send(command);
        return NoContent();
    }

    // DELETE: api/rooms/{id}
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,HotelUser")]
    public async Task<IActionResult> DeleteRoom(Guid id)
    {
        await _mediator.Send(new DeleteRoomCommand { Id = id }); // Command باید ایجاد شود
        return NoContent();
    }
}