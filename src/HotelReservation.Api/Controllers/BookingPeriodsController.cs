// مسیر: src/HotelReservation.Api/Controllers/BookingPeriodsController.cs
using HotelReservation.Application.DTOs.Booking;
using HotelReservation.Application.Features.BookingPeriods.Commands.CreateBookingPeriod;
using HotelReservation.Application.Features.BookingPeriods.Commands.DeleteBookingPeriod;
using HotelReservation.Application.Features.BookingPeriods.Commands.UpdateBookingPeriod;
using HotelReservation.Application.Features.BookingPeriods.Queries.GetActiveBookingPeriods;
using HotelReservation.Application.Features.BookingPeriods.Queries.GetAllBookingPeriods;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelReservation.Api.Controllers;

[ApiController]
[Route("api/booking-periods")]
[Authorize] // تمام Endpointها نیاز به احراز هویت دارند
public class BookingPeriodsController : ControllerBase
{
    private readonly IMediator _mediator;
    public BookingPeriodsController(IMediator mediator) => _mediator = mediator;

    // GET: api/booking-periods
    [HttpGet]
    [Authorize(Roles = "SuperAdmin")] // فقط مدیر ارشد می‌تواند لیست کامل را ببیند
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<BookingPeriodDto>))]
    public async Task<IActionResult> GetAll()
    {
        var periods = await _mediator.Send(new GetAllBookingPeriodsQuery());
        return Ok(periods);
    }

    // GET: api/booking-periods/active
    [HttpGet("active")]
    [Authorize(Roles = "SuperAdmin,ProvinceUser")] // کاربران استان هم برای ثبت رزرو نیاز دارند
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<BookingPeriodDto>))]
    public async Task<IActionResult> GetActive()
    {
        var activePeriods = await _mediator.Send(new GetActiveBookingPeriodsQuery());
        return Ok(activePeriods);
    }

    // POST: api/booking-periods
    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Guid))]
    public async Task<IActionResult> Create([FromBody] CreateBookingPeriodDto dto)
    {
        var command = new CreateBookingPeriodCommand 
        { 
            Name = dto.Name, 
            StartDate = dto.StartDate, 
            EndDate = dto.EndDate, 
            IsActive = dto.IsActive 
        };
        var periodId = await _mediator.Send(command);
        // در آینده یک Endpoint برای GetById ایجاد خواهیم کرد
        return CreatedAtAction(null, new { id = periodId }, new { id = periodId });
    }
    
    // PUT: api/booking-periods/{id}
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBookingPeriodDto dto)
    {
        var command = new UpdateBookingPeriodCommand
        {
            Id = id,
            Name = dto.Name,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            IsActive = dto.IsActive
        };
        await _mediator.Send(command);
        return NoContent();
    }

    // DELETE: api/booking-periods/{id}
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteBookingPeriodCommand { Id = id });
        return NoContent();
    }
}
