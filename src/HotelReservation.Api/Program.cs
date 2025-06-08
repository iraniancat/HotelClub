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

// خواندن مبدأ مجاز برای CORS
var allowedOrigin = builder.Configuration["AllowedOrigins"];
if (string.IsNullOrEmpty(allowedOrigin))
{
    // اگر در appsettings تعریف نشده، یک مقدار پیش‌فرض برای توسعه در نظر بگیرید
    // این آدرس باید با آدرس برنامه Blazor WASM شما مطابقت داشته باشد.
    allowedOrigin = "https://localhost:7001"; // <<-- این را با پورت کلاینت خود تنظیم کنید، یا پورت API اگر چیزی اشتباه است.
                                              // معمولاً پورت کلاینت و API متفاوت است. مثلاً اگر API روی 7001 است، کلاینت ممکن است روی 7123 باشد.
    Console.WriteLine($"Warning: AllowedOrigins not configured in appsettings.json. Using default: {allowedOrigin}");
}

// <<-- ثبت سرویس‌های لاگینگ در ابتدا -->>
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.ClearProviders(); // (اختیاری) پاک کردن Providerهای پیش‌فرض اگر می‌خواهید فقط Providerهای خودتان را اضافه کنید
    loggingBuilder.AddConsole();    // اطمینان از وجود حداقل یک Provider (مثلاً Console)
    // loggingBuilder.AddDebug();   // Provider دیگر برای محیط توسعه
});

// 1. ثبت سرویس‌های Infrastructure (شامل DbContext, UnitOfWork, Repositories)
builder.Services.AddInfrastructureServices(builder.Configuration);

// 2. ثبت سرویس‌های Application (شامل MediatR, Validators, ValidationBehavior)
builder.Services.AddApplicationServices(); // <<-- این خط جایگزین ثبت مستقیم MediatR می‌شود

// --- ثبت سرویس‌های مربوط به کاربر فعلی ---
builder.Services.AddHttpContextAccessor(); // <<-- ثبت IHttpContextAccessor
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>(); // <<-- ثبت سرویس ما
// --- پایان ثبت سرویس‌های کاربر فعلی ---

// --- افزودن و پیکربندی سرویس CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policyBuilder => // نام Policy دلخواه است
    {
        policyBuilder.WithOrigins(allowedOrigin) // <<-- استفاده از مبدأ خوانده شده از تنظیمات
                     .AllowAnyHeader()
                     .AllowAnyMethod();
                     // .AllowCredentials(); // اگر از کوکی یا Credentialهای خاصی استفاده می‌کنید (برای JWT معمولاً لازم نیست)
    });
});
// --- پایان پیکربندی سرویس CORS ---


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

// --- استفاده از Middleware CORS ---
// این باید قبل از UseRouting (اگر صریحاً استفاده شده)، UseAuthentication، UseAuthorization و MapControllers باشد.
app.UseCors("AllowBlazorClient"); // <<-- استفاده از Policy که تعریف کردیم
// --- پایان استفاده از Middleware CORS ---


app.UseHttpsRedirection();
app.MapControllers();
app.Run();