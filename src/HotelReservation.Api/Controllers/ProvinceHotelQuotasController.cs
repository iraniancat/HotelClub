using HotelReservation.Application.DTOs.Quota;
using HotelReservation.Application.Features.Quotas.Commands.SetQuota;
using HotelReservation.Application.Features.Quotas.Queries.GetQuotasByHotel;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservation.Api.Controllers;

[ApiController]
[Route("api/province-hotel-quotas")]
[Authorize(Roles = "SuperAdmin")] // فقط مدیر ارشد می‌تواند سهمیه‌ها را مدیریت کند
public class ProvinceHotelQuotasController : ControllerBase
{
    private readonly IMediator _mediator;
    public ProvinceHotelQuotasController(IMediator mediator) => _mediator = mediator;

    // GET: api/province-hotel-quotas/by-hotel/{hotelId}
    [HttpGet("by-hotel/{hotelId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ProvinceHotelQuotaDto>))]
    public async Task<IActionResult> GetQuotasByHotel(Guid hotelId)
    {
        var query = new GetQuotasByHotelQuery { HotelId = hotelId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
    
    // POST: api/province-hotel-quotas
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Guid))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetQuota([FromBody] SetQuotaDto dto)
    {
        var command = new SetQuotaCommand
        {
            HotelId = dto.HotelId,
            ProvinceCode = dto.ProvinceCode,
            RoomLimit = dto.RoomLimit
        };
        var id = await _mediator.Send(command);
        // اینجا می‌توان یک پاسخ Created با آدرس منبع جدید برگرداند
        return StatusCode(StatusCodes.Status201Created, new { id });
    }
}