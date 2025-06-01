// src/HotelReservation.Api/Controllers/RolesController.cs
using MediatR;
using Microsoft.AspNetCore.Authorization; // برای [Authorize]
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using HotelReservation.Application.DTOs.Role; // برای CreateRoleDto, RoleDto
using HotelReservation.Application.Features.Roles.Commands.CreateRole;
using HotelReservation.Application.Features.Roles.Queries.GetAllRoles;
using HotelReservation.Application.Features.Roles.Queries.GetRoleById;
using HotelReservation.Application.Features.Roles.Commands.UpdateRole;
using HotelReservation.Application.Features.Roles.Commands.DeleteRole; // برای CreateRoleCommand
// using HotelReservation.Application.Exceptions; // اگر try-catch دستی می‌خواهید
// using FluentValidation; // اگر try-catch دستی برای ValidationException می‌خواهید

namespace HotelReservation.Api.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize(Roles = "SuperAdmin")] // <<-- تمام Actionهای این Controller فقط برای SuperAdmin
public class RolesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<RolesController> _logger;

    public RolesController(IMediator mediator, ILogger<RolesController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // در RolesController.cs، متد CreateRole را به این صورت اصلاح کنید:
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(RoleDto))] // <<-- نوع پاسخ می‌تواند RoleDto باشد
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto createRoleDto)
    {
        if (createRoleDto == null)
        {
            return BadRequest(new { message = "اطلاعات نقش برای ایجاد ارسال نشده است." });
        }

        var command = new CreateRoleCommand(createRoleDto.Name, createRoleDto.Description);
        var roleId = await _mediator.Send(command);

        _logger.LogInformation("Role with ID {RoleId} created successfully.", roleId);

        // برای اینکه CreatedAtAction شیء کامل نقش را برگرداند، باید آن را دوباره بخوانیم
        // یا اینکه CreateRoleCommand یک RoleDto برگرداند.
        // فعلاً فقط شناسه را در مسیر و یک شیء ساده در بدنه برمی‌گردانیم.
        // یک راه بهتر: CreateRoleCommand یک RoleDto برگرداند.
        // راه ساده‌تر فعلی:
        var roleToReturn = new { id = roleId, name = createRoleDto.Name, description = createRoleDto.Description };
        return CreatedAtAction(nameof(GetRoleById), new { id = roleId }, roleToReturn);
    }
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<RoleDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllRoles()
    {
        var query = new GetAllRolesQuery();
        var roles = await _mediator.Send(query);
        return Ok(roles);
    }
    [HttpGet("{id:guid}", Name = "GetRoleById")] // Name برای استفاده در CreatedAtAction
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RoleDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRoleById(Guid id)
    {
        var query = new GetRoleByIdQuery(id);
        var role = await _mediator.Send(query);

        if (role == null)
        {
            return NotFound(new { message = $"نقشی با شناسه '{id}' یافت نشد." });
        }

        return Ok(role);
    }
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // برای خطاهای اعتبارسنجی
    [ProducesResponseType(StatusCodes.Status404NotFound)]   // برای NotFoundException
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleDto updateRoleDto)
    {
        if (updateRoleDto == null)
        {
            return BadRequest(new { message = "اطلاعات نقش برای به‌روزرسانی ارسال نشده است." });
        }

        var command = new UpdateRoleCommand(id, updateRoleDto.Name, updateRoleDto.Description);

        // Exception‌ها توسط Middleware و ValidationBehavior مدیریت می‌شوند
        await _mediator.Send(command);

        return NoContent(); // پاسخ 204 No Content برای موفقیت‌آمیز بودن عملیات به‌روزرسانی
    }
    
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // برای خطاهایی مانند تخصیص نقش به کاربر
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteRole(Guid id)
    {
        var command = new DeleteRoleCommand(id);
        
        // Exception‌ها توسط Middleware و ValidationBehavior مدیریت می‌شوند
        await _mediator.Send(command); 
        
        return NoContent(); // پاسخ 204 No Content برای موفقیت‌آمیز بودن عملیات حذف
    }
    // TODO: Endpoints برای GetRoleById, GetAllRoles, UpdateRole, DeleteRole در اینجا اضافه خواهند شد.
    // [HttpGet("{id:guid}", Name = "GetRoleById")]
    // public async Task<IActionResult> GetRoleById(Guid id) { /* ... */ return Ok(); }
}