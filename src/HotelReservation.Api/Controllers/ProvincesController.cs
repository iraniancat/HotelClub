// src/HotelReservation.Api/Controllers/ProvincesController.cs
using MediatR;
using Microsoft.AspNetCore.Authorization; // برای [Authorize]
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic; // برای IEnumerable
using System.Threading.Tasks;
using HotelReservation.Application.DTOs.Location; // برای ProvinceDto
using HotelReservation.Application.Features.Provinces.Queries.GetAllProvinces;
using HotelReservation.Application.Features.Provinces.Queries.GetProvinceByCode;

namespace HotelReservation.Api.Controllers;

[ApiController]
[Route("api/provinces")]
[Authorize] // <<-- تمام کاربران احراز هویت شده می‌توانند لیست استان‌ها را ببینند (یا نقش خاصی تعیین کنید)
public class ProvincesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DepartmentsController> _logger;

    public ProvincesController(IMediator mediator, ILogger<DepartmentsController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // GET: api/provinces
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ProvinceDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    // [ProducesResponseType(StatusCodes.Status403Forbidden)] // اگر نقش خاصی لازم بود
    public async Task<IActionResult> GetAllProvinces()
    {
        var query = new GetAllProvincesQuery();
        var provinces = await _mediator.Send(query);
        return Ok(provinces);
    }

    // GET: api/provinces/{code}
    [HttpGet("{code}", Name = "GetProvinceByCode")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProvinceDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    // [ProducesResponseType(StatusCodes.Status403Forbidden)] // اگر نقش خاصی لازم بود
    public async Task<IActionResult> GetProvinceByCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return BadRequest(new { message = "کد استان نمی‌تواند خالی باشد." });
        }
        var query = new GetProvinceByCodeQuery(code);
        var province = await _mediator.Send(query);

        if (province == null)
        {
            return NotFound(new { message = $"استانی با کد '{code}' یافت نشد." });
        }
        return Ok(province);
    }
}