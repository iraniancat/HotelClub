// tests/HotelReservation.Api.IntegrationTests/Controllers/RolesControllerIntegrationTests.cs
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Threading.Tasks;
using HotelReservation.Application.DTOs.Role; // برای CreateRoleDto
using System.Net.Http.Json; // برای PostAsJsonAsync و ReadFromJsonAsync
using System.Net; // برای HttpStatusCode
using Microsoft.Extensions.DependencyInjection; // برای CreateScope و GetRequiredService
using HotelReservation.Infrastructure.Persistence; // برای AppDbContext
using HotelReservation.Domain.Entities; // برای Role
using System; // برای Guid

namespace HotelReservation.Api.IntegrationTests.Controllers;

[TestClass]
public class RolesControllerIntegrationTests
{
    private static CustomWebApplicationFactory<Program> _factory; // Program از پروژه Api
    private HttpClient _client;

    [ClassInitialize] // این متد یک بار قبل از اجرای تمام تست‌های این کلاس اجرا می‌شود
    public static void ClassInit(TestContext context)
    {
        _factory = new CustomWebApplicationFactory<Program>();
    }

    [TestInitialize] // این متد قبل از هر تست اجرا می‌شود
    public void TestInit()
    {
        _client = _factory.CreateClient(); // HttpClient با احراز هویت تستی (TestAuthHandler) ایجاد می‌شود
        // TestAuthHandler ما به طور پیش‌فرض کاربر SuperAdmin را شبیه‌سازی می‌کند
    }
    
    [TestCleanup] // این متد بعد از هر تست اجرا می‌شود
    public void TestCleanup()
    {
        _client?.Dispose();
    }

    [ClassCleanup] // این متد یک بار پس از اجرای تمام تست‌های این کلاس اجرا می‌شود
    public static void ClassCleanup()
    {
        _factory?.Dispose();
    }

    // یک کلاس کمکی برای خواندن پاسخ JSON از ایجاد نقش
    private class CreateRoleResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        // Description هم اگر در پاسخ بود، اینجا اضافه می‌شد
    }

    [TestMethod]
    public async Task CreateRole_ValidRole_ReturnsCreatedWithRoleDetailsAndSavesToDb()
    {
        // Arrange (آماده‌سازی)
        var createRoleDto = new CreateRoleDto
        {
            Name = "IntegrationTestRole_" + Guid.NewGuid().ToString().Substring(0, 8), // نام یکتا برای هر تست
            Description = "Role created via integration test."
        };

        // Act (اجرای عملیات)
        var response = await _client.PostAsJsonAsync("/api/roles", createRoleDto);

        // Assert (بررسی نتایج)

        // ۱. بررسی وضعیت پاسخ HTTP
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, $"Unexpected status code. Response: {await response.Content.ReadAsStringAsync()}");
        response.EnsureSuccessStatusCode(); // اگر خطا باشد، Exception پرتاب می‌کند

        // ۲. بررسی محتوای پاسخ
        var createdRoleResponse = await response.Content.ReadFromJsonAsync<CreateRoleResponse>();
        Assert.IsNotNull(createdRoleResponse, "Response content should not be null.");
        Assert.AreNotEqual(Guid.Empty, createdRoleResponse.Id, "Created Role ID should not be empty.");
        Assert.AreEqual(createRoleDto.Name, createdRoleResponse.Name, "Role name in response does not match input.");

        // ۳. (اختیاری اما بسیار مفید) بررسی هدر Location
        Assert.IsNotNull(response.Headers.Location, "Location header should not be null.");
        Assert.IsTrue(response.Headers.Location.OriginalString.Contains($"/api/roles/{createdRoleResponse.Id}"), "Location header does not point to the new resource URL.");


        // ۴. بررسی اینکه آیا نقش واقعاً در پایگاه داده (InMemory) ذخیره شده است
        using (var scope = _factory.Services.CreateScope()) // یک Scope جدید برای دسترسی به سرویس‌ها ایجاد می‌کنیم
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var roleInDb = await dbContext.Roles.FindAsync(createdRoleResponse.Id);

            Assert.IsNotNull(roleInDb, "Role should be saved in the database.");
            Assert.AreEqual(createRoleDto.Name, roleInDb.Name, "Role name in DB does not match input.");
            Assert.AreEqual(createRoleDto.Description, roleInDb.Description, "Role description in DB does not match input.");
        }
    }

    [TestMethod]
    public async Task CreateRole_DuplicateRoleName_ReturnsBadRequest()
    {
        // Arrange
        var roleName = "DuplicateTestRole_" + Guid.NewGuid().ToString().Substring(0, 8);
        var initialDto = new CreateRoleDto { Name = roleName, Description = "Initial creation." };
        
        // ابتدا نقش را یک بار ایجاد می‌کنیم
        var initialResponse = await _client.PostAsJsonAsync("/api/roles", initialDto);
        Assert.AreEqual(HttpStatusCode.Created, initialResponse.StatusCode, "Initial role creation failed.");

        // حالا سعی می‌کنیم دوباره با همان نام ایجاد کنیم
        var duplicateDto = new CreateRoleDto { Name = roleName, Description = "Attempting duplicate." };

        // Act
        var response = await _client.PostAsJsonAsync("/api/roles", duplicateDto);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, "Should return BadRequest for duplicate role name.");
        
        var errorContent = await response.Content.ReadAsStringAsync();
        // بررسی کنید که پیام خطا شامل متن مربوط به تکراری بودن نام نقش باشد
        // این بستگی به خروجی ExceptionHandlerMiddleware و ValidationException دارد
        Assert.IsTrue(errorContent.Contains("نقشی با این نام قبلاً ثبت شده است") || 
                      errorContent.Contains("خطای اعتبارسنجی"), // یا پیام عمومی‌تر از Middleware
                      $"Unexpected error message: {errorContent}");
    }

    [TestMethod]
    public async Task CreateRole_InvalidData_ReturnsBadRequest()
    {
        // Arrange
        var invalidDto = new CreateRoleDto { Name = "", Description = "Test" }; // نام خالی

        // Act
        var response = await _client.PostAsJsonAsync("/api/roles", invalidDto);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var errorContent = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(errorContent.Contains("نام نقش الزامی است"), $"Unexpected error message: {errorContent}");
    }
}