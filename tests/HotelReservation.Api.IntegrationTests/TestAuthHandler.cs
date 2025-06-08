// tests/HotelReservation.Api.IntegrationTests/TestAuthHandler.cs
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace HotelReservation.Api.IntegrationTests;

public class TestAuthHandlerOptions : AuthenticationSchemeOptions { }

public class TestAuthHandler : AuthenticationHandler<TestAuthHandlerOptions>
{
    public const string AuthenticationScheme = "TestScheme"; // نام Scheme تستی ما

    // این لیست Claimها می‌تواند به صورت استاتیک یا از طریق Options مقداردهی شود
    // تا بتوانیم در هر تست، کاربر با Claimهای متفاوتی را شبیه‌سازی کنیم.
    // فعلاً یک کاربر SuperAdmin را شبیه‌سازی می‌کنیم.
    public static List<Claim> DefaultClaims { get; } = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()), // یک شناسه کاربر تستی
        new Claim(ClaimTypes.Name, "test_superadmin"),
        new Claim(ClaimTypes.GivenName, "Test SuperAdmin User"),
        new Claim(ClaimTypes.Role, "SuperAdmin"), // نقش SuperAdmin
        // سایر Claimهای لازم مانند "province_code", "hotel_id" می‌توانند اینجا اضافه شوند اگر نیاز به تست با آن‌ها باشد
    };

    public TestAuthHandler(
        IOptionsMonitor<TestAuthHandlerOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var identity = new ClaimsIdentity(DefaultClaims, AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AuthenticationScheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}