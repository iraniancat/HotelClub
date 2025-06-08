// src/HotelReservation.Api/Controllers/UserManagementController.cs
using MediatR; // برای IMediator
using Microsoft.AspNetCore.Mvc; // برای ControllerBase, HttpGet, IActionResult و ...
using System; // برای Guid
using System.Collections.Generic; // برای IEnumerable
using System.Threading.Tasks; // برای Task
using HotelReservation.Application.DTOs.UserManagement; // برای UserManagementDetailsDto و UserManagementListDto
using HotelReservation.Application.Features.UserManagement.Queries.GetUserById; // برای GetUserByIdForManagementQuery
using HotelReservation.Application.Features.UserManagement.Queries.GetAllUsers; // برای GetAllUsersForManagementQuery
using Microsoft.AspNetCore.Http;
using HotelReservation.Application.Features.UserManagement.Commands.SetUserActivation;
using HotelReservation.Application.Features.UserManagement.Commands.AssignRole;
using HotelReservation.Application.Features.UserManagement.Commands.CreateNonEmployeeUser;
using HotelReservation.Application.Features.UserManagement.Commands.UpdateUser;
using HotelReservation.Application.Features.UserManagement.Commands.SetUserPassword;
using Microsoft.AspNetCore.Authorization;
using HotelReservation.Application.DTOs.Common;
using HotelReservation.Application.Features.UserManagement.Queries.SearchUsers; // برای StatusCodes

namespace HotelReservation.Api.Controllers;

[ApiController]
[Route("api/management/users")] // مسیر پایه: api/management/users
[Authorize(Roles = "SuperAdmin")] // <<-- تمام Actionهای این Controller فقط برای SuperAdmin
public class UserManagementController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DepartmentsController> _logger;



    public UserManagementController(IMediator mediator, ILogger<DepartmentsController> logger)

    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // GET: api/management/users
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<UserManagementListDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    // [Authorize(Roles = "SuperAdmin")] // این از کلاس به ارث می‌رسد
    public async Task<IActionResult> GetAllUsers(
       [FromQuery] string? searchTerm = null,
       [FromQuery] Guid? roleIdFilter = null, // <<-- اضافه شد
       [FromQuery] bool? isActiveFilter = null, // <<-- اضافه شد
       [FromQuery] int pageNumber = 1,
       [FromQuery] int pageSize = 10)
    {
        // شناسه کاربر لاگین کرده برای این Query خاص لازم نیست، چون مدیر ارشد همه را می‌بیند
        // اما اگر در آینده نیاز به فیلتر بر اساس دسترسی‌های خاص مدیر ارشد بود، می‌توان آن را اضافه کرد.

        var query = new GetAllUsersForManagementQuery
        {
            SearchTerm = searchTerm,
            RoleIdFilter = roleIdFilter,
            IsActiveFilter = isActiveFilter,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    // GET: api/management/users/{id}
    [HttpGet("{id:guid}", Name = "GetUserForManagementById")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserManagementDetailsDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var query = new GetUserByIdForManagementQuery(id);
        var userDto = await _mediator.Send(query);

        if (userDto == null)
        {
            // NotFoundException توسط Global Exception Handler مدیریت خواهد شد اگر Handler آن را پرتاب کند.
            // اگر Handler مستقیماً null برگرداند، ما اینجا NotFound() را برمی‌گردانیم.
            return NotFound(new { message = $"کاربری با شناسه '{id}' یافت نشد." });
        }

        return Ok(userDto);
    }

    [HttpPut("{id:guid}/activation")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // برای خطاهای اعتبارسنجی
    [ProducesResponseType(StatusCodes.Status404NotFound)]   // برای NotFoundException
    public async Task<IActionResult> SetUserActivationStatus(Guid id, [FromBody] SetUserActivationDto dto)
    {
        var command = new SetUserActivationCommand(id, dto.IsActive);
        await _mediator.Send(command); // NotFoundException و ValidationException توسط Middleware مدیریت می‌شوند
        return NoContent();
    }

    [HttpPut("{id:guid}/role")] // PUT api/management/users/{id}/role
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // برای خطاهای اعتبارسنجی و BadRequestException
    [ProducesResponseType(StatusCodes.Status404NotFound)]   // برای NotFoundException
    public async Task<IActionResult> AssignRoleToUser(Guid id, [FromBody] AssignRoleToUserDto dto)
    {
        if (dto == null)
        {
            return BadRequest("اطلاعات تخصیص نقش ارسال نشده است.");
        }

        var command = new AssignRoleToUserCommand(id, dto.RoleId, dto.HotelId, dto.ProvinceCode);
        await _mediator.Send(command); // Exceptionها توسط Middleware مدیریت می‌شوند

        return NoContent();
    }

    [HttpPost] // POST api/management/users
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(object))] // بازگشت شناسه کاربر جدید
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // برای خطاهای اعتبارسنجی و BadRequestException
    public async Task<IActionResult> CreateNonEmployeeUser([FromBody] CreateNonEmployeeUserDto dto)
    {
        if (dto == null)
        {
            return BadRequest("اطلاعات کاربر برای ایجاد ارسال نشده است.");
        }

        var command = new CreateNonEmployeeUserCommand(
            dto.SystemUserId, dto.FullName, dto.Password, dto.RoleId, dto.IsActive,
            dto.NationalCode, dto.PhoneNumber, dto.ProvinceCode, dto.ProvinceName,
            dto.DepartmentCode, dto.DepartmentName, dto.HotelId
        );

        var userId = await _mediator.Send(command);

        // بازگرداندن پاسخ CreatedAtAction با ارجاع به GetUserById (که قبلاً ایجاد کردیم)
        return CreatedAtAction(nameof(GetUserById), new { id = userId }, new { id = userId });
    }

    [HttpPut("{id:guid}")] // PUT api/management/users/{id}
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto dto)
    {
        if (dto == null)
        {
            return BadRequest("اطلاعات کاربر برای به‌روزرسانی ارسال نشده است.");
        }

        var command = new UpdateUserCommand
        {
            UserId = id,
            FullName = dto.FullName,
            IsActive = dto.IsActive,
            RoleId = dto.RoleId,
            NationalCode = dto.NationalCode,
            PhoneNumber = dto.PhoneNumber, // <<-- اضافه شد
            ProvinceCode = dto.ProvinceCode,
            DepartmentCode = dto.DepartmentCode,
            HotelId = dto.HotelId
        };
        await _mediator.Send(command); // Exceptionها توسط Middleware مدیریت می‌شوند

        return NoContent();
    }

    [HttpPut("{id:guid}/set-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // برای خطاهای اعتبارسنجی DTO و Command
    [ProducesResponseType(StatusCodes.Status404NotFound)]   // برای NotFoundException
    public async Task<IActionResult> SetUserPassword(Guid id, [FromBody] SetUserPasswordDto dto)
    {
        if (dto == null) // بررسی اولیه DTO
        {
            return BadRequest("اطلاعات لازم برای تغییر رمز عبور ارسال نشده است.");
        }
        // اعتبارسنجی DataAnnotations روی DTO (مانند Compare) توسط [ApiController] انجام می‌شود.
        // اعتبارسنجی FluentValidation روی Command توسط Pipeline Behavior انجام می‌شود.

        var command = new SetUserPasswordCommand(id, dto.NewPassword, dto.ConfirmPassword);
        await _mediator.Send(command); // Exception‌ها توسط Middleware و ValidationBehavior مدیریت می‌شوند

        return NoContent();
    }
     [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UserWithDependentsDto>))]
    public async Task<IActionResult> SearchUsers([FromQuery] string term)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return Ok(new List<UserWithDependentsDto>());
        }
        
        var query = new SearchUsersQuery(term);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}