// src/HotelReservation.Api/Controllers/HotelsController.cs
using MediatR; // برای IMediator
using Microsoft.AspNetCore.Mvc; // برای ControllerBase, HttpPost, FromBody, IActionResult و ...
using System.Threading.Tasks; // برای Task
using HotelReservation.Application.Features.Hotels.Commands.CreateHotel; // برای CreateHotelCommand
using HotelReservation.Application.DTOs.Hotel; // برای CreateHotelDto
using Microsoft.AspNetCore.Http;
using HotelReservation.Application.Features.Hotels.Queries.GetHotelById;
using HotelReservation.Application.Features.Hotels.Queries.GetAllHotels;
using HotelReservation.Application.Features.Hotels.Commands.UpdateHotel;
using HotelReservation.Application.Exceptions;
using HotelReservation.Application.Features.Hotels.Commands.DeleteHotel;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using HotelReservation.Application.DTOs.Common; // برای StatusCodes

namespace HotelReservation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HotelsController : ControllerBase
{
    private readonly IMediator _mediator;

    public HotelsController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin")] // <<-- فقط SuperAdmin
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(HotelDto))] // تغییر Type به HotelDto اگر می‌خواهیم شی کامل را برگردانیم
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateHotel([FromBody] CreateHotelDto createHotelDto)
    {
        if (createHotelDto == null)
        {
            return BadRequest("اطلاعات هتل برای ایجاد ارسال نشده است.");
        }

        var command = new CreateHotelCommand(
            createHotelDto.Name,
            createHotelDto.Address,
            createHotelDto.ContactPerson,
            createHotelDto.PhoneNumber
        );

        var hotelId = await _mediator.Send(command);

        // برای اینکه CreatedAtAction بتواند هتل کامل را برگرداند، باید آن را دوباره بخوانیم
        // یا اینکه CreateHotelCommand یک HotelDto برگرداند.
        // فعلا فقط شناسه را در مسیر و در بدنه برمی‌گردانیم.
        return CreatedAtAction(nameof(GetHotelById), new { id = hotelId }, new { id = hotelId });
    }
    [HttpGet("{id:guid}", Name = "GetHotelById")] // نام مسیر برای استفاده در CreatedAtAction
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(HotelDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHotelById(Guid id)
    {
        var query = new GetHotelByIdQuery(id);
        var hotelDto = await _mediator.Send(query);

        if (hotelDto == null)
        {
            return NotFound($"هتلی با شناسه '{id}' یافت نشد.");
        }

        return Ok(hotelDto);
    }
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<HotelDto>))]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<HotelDto>))] // <<-- نوع پاسخ تغییر کرد
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    // [Authorize] // این از کلاس به ارث می‌رسد
    public async Task<IActionResult> GetAllHotels(
        [FromQuery] string? searchTerm = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetAllHotelsQuery
        {
            SearchTerm = searchTerm,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SuperAdmin")] // <<-- فقط SuperAdmin
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateHotel(Guid id, [FromBody] UpdateHotelDto updateHotelDto)
    {
        if (updateHotelDto == null)
        {
            return BadRequest("اطلاعات هتل برای به‌روزرسانی ارسال نشده است.");
        }

        // می‌توان یک بررسی انجام داد که id در مسیر با id در DTO (اگر وجود داشت) یکی باشد
        // if (id != updateHotelDto.Id) return BadRequest("عدم تطابق شناسه در مسیر و بدنه درخواست.");

        var command = new UpdateHotelCommand(
            id, // از مسیر URL
            updateHotelDto.Name,
            updateHotelDto.Address,
            updateHotelDto.ContactPerson,
            updateHotelDto.PhoneNumber
        );


        await _mediator.Send(command);
        return NoContent(); // پاسخ 204 No Content نشان‌دهنده موفقیت‌آمیز بودن عملیات بدون بازگرداندن محتوا است.

        // catch (ValidationException ex) // در آینده برای خطاهای FluentValidation از MediatR pipeline
        // {
        //     return BadRequest(ex.Errors);
        // }
    }
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SuperAdmin")] // <<-- فقط SuperAdmin
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // برای خطاهای دیگر مانند وابستگی‌ها
    public async Task<IActionResult> DeleteHotel(Guid id)
    {
        var command = new DeleteHotelCommand(id);


        await _mediator.Send(command);
        return NoContent(); // پاسخ 204 No Content نشان‌دهنده موفقیت‌آمیز بودن عملیات حذف است.

        // سایر Exception ها باید توسط Global Exception Handler مدیریت شوند.
    }
}