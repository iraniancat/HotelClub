using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using HotelReservation.Client;
using MudBlazor.Services; // <<-- اضافه کنید
using HotelReservation.Client.Services;
using Blazored.LocalStorage;
using HotelReservation.Client.Services.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using HotelReservation.Client.Auth;
using Blazor.PersianDatePicker.Extensions;
using HotelReservation.Application.Contracts.Security;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddLogging(logging => logging.SetMinimumLevel(LogLevel.Debug));
//.AddBrowserConsole()); // <<-- اضافه یا تکمیل شود

//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// HttpClient را با آدرس پایه API بک‌اند پیکربندی می‌کنیم
// به جای استفاده از BaseAddress هاست برنامه کلاینت
builder.Services.AddScoped(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>(); // برای خواندن appsettings.json
    var apiBaseUrl = configuration["ApiBaseUrl"];
    if (string.IsNullOrEmpty(apiBaseUrl))
    {
        // یک مقدار پیش‌فرض یا خطا در صورت عدم وجود پیکربندی
        Console.WriteLine("Warning: ApiBaseUrl not configured in appsettings.json. Using default or potentially incorrect URL.");
        apiBaseUrl = "https://localhost:5220"; // <<-- این را با آدرس پیش‌فرض صحیح API خود جایگزین کنید
    }
    return new HttpClient { BaseAddress = new Uri(apiBaseUrl) };
});

builder.Services.AddScoped<IApiClientService, ApiClientService>();

builder.Services.AddMudServices(); // <<-- سرویس‌های MudBlazor را اضافه کنید

builder.Services.AddBlazoredLocalStorage(); // <<-- سرویس LocalStorage را اضافه کنید
builder.Services.AddAuthorizationCore(); // <<-- سرویس‌های پایه مجوزدهی Blazor را اضافه کنید

builder.Services.AddScoped<IAuthService, AuthService>();

// ثبت CustomAuthenticationStateProvider
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>(); // <<-- اضافه شد

// <<-- ثبت پیاده‌سازی کلاینت برای ICurrentUserService -->>
builder.Services.AddScoped<ICurrentUserService, ClientCurrentUserService>();
//builder.Services.AddPersianDatePicker();
await builder.Build().RunAsync();
