// tests/HotelReservation.Application.UnitTests/Features/Roles/Queries/GetRoleById/GetRoleByIdQueryHandlerTests.cs
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using HotelReservation.Application.Contracts.Persistence; // برای IUnitOfWork و IRoleRepository
using HotelReservation.Application.Features.Roles.Queries.GetRoleById; // برای Query و Handler
using HotelReservation.Application.DTOs.Role; // برای RoleDto
using HotelReservation.Domain.Entities; // برای Role
using System.Threading;
using System.Threading.Tasks;
using System;

namespace HotelReservation.Application.UnitTests.Features.Roles.Queries.GetRoleById;

[TestClass]
public class GetRoleByIdQueryHandlerTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<IRoleRepository> _mockRoleRepository;
    private GetRoleByIdQueryHandler _handler;

    [TestInitialize]
    public void Setup()
    {
        _mockRoleRepository = new Mock<IRoleRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _mockUnitOfWork.Setup(uow => uow.RoleRepository).Returns(_mockRoleRepository.Object);

        _handler = new GetRoleByIdQueryHandler(_mockUnitOfWork.Object);
        // اگر از AutoMapper استفاده می‌کردید، باید IMapper را هم Mock و به سازنده Handler پاس می‌دادید.
    }

    [TestMethod]
    public async Task Handle_ExistingRoleId_ShouldReturnRoleDto()
    {
        // Arrange (آماده‌سازی)
        var roleId = Guid.NewGuid();
        var roleName = "Test Role";
        var roleDescription = "This is a test role.";
        var existingRole = new Role(roleName, roleDescription); // فرض می‌کنیم سازنده Role به این شکل است و Id داخل آن تولید می‌شود
                                                                // یا اگر Id را خودمان ست می‌کنیم: new Role { Id = roleId, Name = roleName, ... }
                                                                // برای تست دقیق‌تر، Id را خودمان ست می‌کنیم تا بتوانیم آن را مقایسه کنیم.
                                                                // برای این کار باید Id در Role setter داشته باشد یا از reflection استفاده کنیم.
                                                                // ساده‌ترین راه: فرض می‌کنیم سازنده Role به ما اجازه کنترل Id را نمی‌دهد،
                                                                // پس ما یک شیء Role می‌سازیم و فرض می‌کنیم GetByIdAsync آن را برمی‌گرداند.
                                                                // اما بهتر است بتوانیم Id را کنترل کنیم.
                                                                // با فرض اینکه سازنده Role فقط name و description می‌گیرد و Id را خودش می‌سازد،
                                                                // نمی‌توانیم Id را از قبل بدانیم. پس رفتار Mock را تغییر می‌دهیم.

        // برای سادگی و کنترل Id، فرض می‌کنیم یک سازنده خصوصی داریم و از طریق reflection یا یک متد کارخانه‌ای Id ست می‌شود
        // یا اینکه در تست، Id را مستقیماً در شیء mock شده ست می‌کنیم.
        // روش ساده‌تر: یک Role کامل بسازیم و به Mock بدهیم.

        var mockRoleEntity = new Role(roleName, roleDescription);
        // برای اینکه بتوانیم Id را مقایسه کنیم، از یک Id ثابت در تست استفاده می‌کنیم
        // این نیاز دارد که بتوانیم Id را در Role ست کنیم، که با private set فعلی ممکن نیست مگر با ترفند
        // یک راه حل ساده‌تر برای تست:
        // 1. یک Role بسازید
        // 2. Mock را طوری تنظیم کنید که همین شیء را برگرداند
        // 3. خصوصیات شیء بازگشتی را با خصوصیات شیء اولیه مقایسه کنید.

        var expectedRoleDto = new RoleDto // چیزی که انتظار داریم از Handler برگردد
        {
            Id = mockRoleEntity.Id, // Id از شیء ساخته شده گرفته می‌شود
            Name = roleName,
            Description = roleDescription
        };
        
        _mockRoleRepository.Setup(repo => repo.GetByIdAsync(mockRoleEntity.Id))
                           .ReturnsAsync(mockRoleEntity); // شیء Role کامل را برمی‌گردانیم

        var query = new GetRoleByIdQuery(mockRoleEntity.Id);

        // Act (اجرای عملیات)
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert (بررسی نتایج)
        Assert.IsNotNull(result, "نتیجه نباید null باشد.");
        Assert.AreEqual(expectedRoleDto.Id, result.Id, "شناسه نقش مطابقت ندارد.");
        Assert.AreEqual(expectedRoleDto.Name, result.Name, "نام نقش مطابقت ندارد.");
        Assert.AreEqual(expectedRoleDto.Description, result.Description, "توضیحات نقش مطابقت ندارد.");

        _mockRoleRepository.Verify(repo => repo.GetByIdAsync(mockRoleEntity.Id), Times.Once, "متد GetByIdAsync باید دقیقاً یک بار فراخوانی شود.");
    }

    [TestMethod]
    public async Task Handle_NonExistingRoleId_ShouldReturnNull()
    {
        // Arrange
        var nonExistingRoleId = Guid.NewGuid();
        _mockRoleRepository.Setup(repo => repo.GetByIdAsync(nonExistingRoleId))
                           .ReturnsAsync((Role?)null); // مشخص می‌کنیم که برای این Id، نتیجه null است

        var query = new GetRoleByIdQuery(nonExistingRoleId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsNull(result, "نتیجه باید null باشد برای شناسه نامعتبر.");
        _mockRoleRepository.Verify(repo => repo.GetByIdAsync(nonExistingRoleId), Times.Once, "متد GetByIdAsync باید دقیقاً یک بار فراخوانی شود.");
    }
}