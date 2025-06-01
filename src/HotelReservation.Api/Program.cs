// src/HotelReservation.Api/Program.cs
using HotelReservation.Infrastructure; // برای AddInfrastructureServices
using HotelReservation.Application; // برای AddApplicationServices و ApplicationAssemblyReference
//using HotelReservation.Infrastructure.Persistence; // برای AppDbContext
//using Microsoft.EntityFrameworkCore; // برای AddDbContext و UseSqlServer
using HotelReservation.Api.Middleware;
using HotelReservation.Api.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text; // <<-- اضافه کردن using برای Middleware
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization; // برای IAuthorizationHandler و AddAuthorization
using HotelReservation.Application.Security;
using HotelReservation.Application.Contracts.Security;
using HotelReservation.Api.Services; // برای ViewBookingRequestRequirement و ViewBookingRequestAuthorizationHandler

var builder = WebApplication.CreateBuilder(args);

// خواندن تنظیمات JWT و ثبت آن‌ها در DI
var jwtSettings = new JwtSettings();
builder.Configuration.Bind(JwtSettings.SectionName, jwtSettings); // خواندن از appsettings.json
builder.Services.AddSingleton(jwtSettings); // ثبت به عنوان Singleton برای دسترسی در سایر نقاط

// <<-- ثبت سرویس‌های لاگینگ در ابتدا -->>
builder.Services.AddLogging();

// 1. ثبت سرویس‌های Infrastructure (شامل DbContext, UnitOfWork, Repositories)
builder.Services.AddInfrastructureServices(builder.Configuration);

// 2. ثبت سرویس‌های Application (شامل MediatR, Validators, ValidationBehavior)
builder.Services.AddApplicationServices(); // <<-- این خط جایگزین ثبت مستقیم MediatR می‌شود

// --- ثبت سرویس‌های مربوط به کاربر فعلی ---
builder.Services.AddHttpContextAccessor(); // <<-- ثبت IHttpContextAccessor
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>(); // <<-- ثبت سرویس ما
// --- پایان ثبت سرویس‌های کاربر فعلی ---

// --- پیکربندی Authentication ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true, // بررسی انقضای توکن
        ValidateIssuerSigningKey = true, // بررسی کلید امضا

        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),

        // برای Clock Skew (اختلاف ساعت سرور و کلاینت)
        // ClockSkew = TimeSpan.Zero // اگر نمی‌خواهید هیچ اختلاف ساعتی را مجاز بدانید
    };
});
// --- پایان پیکربندی Authentication ---



builder.Services.AddAuthorization(options =>
{
    // تعریف Policy برای مشاهده درخواست رزرو
    options.AddPolicy("CanViewBookingRequest", policy =>
        policy.Requirements.Add(new ViewBookingRequestRequirement()));

    // <<-- تعریف Policy جدید -->>
    options.AddPolicy("CanManageBookingForOwnHotel", policy =>
        policy.Requirements.Add(new ManageBookingRequestForHotelRequirement()));
});

// ثبت Authorization Handler ها
// ASP.NET Core به طور خودکار Handlerهایی را که IAuthorizationHandler را پیاده‌سازی می‌کنند، پیدا می‌کند
// اگر در همان اسمبلی باشند یا به درستی ثبت شده باشند.
// برای اطمینان، می‌توانیم آن‌ها را به صراحت ثبت کنیم:
builder.Services.AddScoped<IAuthorizationHandler, ViewBookingRequestAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ManageBookingRequestForHotelAuthorizationHandler>();
// --- پایان پیکربندی Authorization ---


// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => // پیکربندی Swagger برای پشتیبانی از JWT
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "لطفاً توکن JWT را با پیشوند Bearer وارد کنید (مثال: 'Bearer YOUR_TOKEN')",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement {
    {
        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Reference = new Microsoft.OpenApi.Models.OpenApiReference
            {
                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        new string[] {}
    }});
});


// اضافه کردن لاگر به DI Container (معمولاً به صورت پیش‌فرض انجام شده)
builder.Services.AddLogging();

var app = builder.Build();

// <<-- ثبت Middleware مدیریت خطا در اینجا -->>
// این باید یکی از اولین Middlewareها در Pipeline باشد
app.UseCustomExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // TODO: در آینده، می‌توانیم یک Middleware برای Seed کردن داده‌های اولیه در اینجا قرار دهیم
    // app.UseItToSeedData();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();