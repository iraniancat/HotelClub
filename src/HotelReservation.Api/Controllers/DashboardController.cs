// مسیر: src/HotelReservation.Api/Controllers/DashboardController.cs
using HotelReservation.Infrastructure.Persistence; // برای AppDbContext
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace HotelReservation.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = "SuperAdmin")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var userCount = await _context.Users.CountAsync();
        var hotelCount = await _context.Hotels.CountAsync();
        var bookingRequestCount = await _context.BookingRequests.CountAsync();
        // می‌توان آمار دقیق‌تری مانند رزروهای در انتظار را هم اضافه کرد
        var pendingBookingCount = await _context.BookingRequests
            .CountAsync(b => b.Status == Domain.Enums.BookingStatus.SubmittedToHotel);

        var stats = new 
        {
            UserCount = userCount,
            HotelCount = hotelCount,
            TotalBookings = bookingRequestCount,
            PendingBookings = pendingBookingCount
        };

        return Ok(stats);
    }
}
