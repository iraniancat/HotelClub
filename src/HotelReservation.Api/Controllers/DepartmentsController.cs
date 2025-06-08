// src/HotelReservation.Api/Controllers/DepartmentsController.cs
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelReservation.Application.DTOs.Location; // برای DepartmentDto
using HotelReservation.Application.Features.Departments.Queries.GetAllDepartments;
using HotelReservation.Application.Features.Departments.Queries.GetDepartmentByCode;

namespace HotelReservation.Api.Controllers;

[ApiController]
[Route("api/departments")]
[Authorize] // تمام کاربران احراز هویت شده می‌توانند لیست ادارات/شعب را ببینند
public class DepartmentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DepartmentsController> _logger;

    public DepartmentsController(IMediator mediator, ILogger<DepartmentsController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // GET: api/departments
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<DepartmentDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllDepartments()
    {
        var query = new GetAllDepartmentsQuery();
        var departments = await _mediator.Send(query);
        return Ok(departments);
    }

    // GET: api/departments/{code}
    [HttpGet("{code}", Name = "GetDepartmentByCode")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DepartmentDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDepartmentByCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return BadRequest(new { message = "کد اداره/شعبه نمی‌تواند خالی باشد." });
        }
        var query = new GetDepartmentByCodeQuery(code);
        var department = await _mediator.Send(query);

        if (department == null)
        {
            return NotFound(new { message = $"اداره/شعبه‌ای با کد '{code}' یافت نشد." });
        }
        return Ok(department);
    }
}