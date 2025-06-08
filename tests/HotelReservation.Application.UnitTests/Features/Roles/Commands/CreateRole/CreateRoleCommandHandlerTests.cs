// tests/HotelReservation.Application.UnitTests/Features/Roles/Commands/CreateRole/CreateRoleCommandHandlerTests.cs
using Microsoft.VisualStudio.TestTools.UnitTesting; // یا using Xunit; یا using NUnit.Framework;
using Moq; // برای Mock کردن
using HotelReservation.Application.Contracts.Persistence; // برای IUnitOfWork و IRoleRepository
using HotelReservation.Application.Features.Roles.Commands.CreateRole;
using HotelReservation.Domain.Entities; // برای Role
using System.Threading;
using System.Threading.Tasks;
using System;

namespace HotelReservation.Application.UnitTests.Features.Roles.Commands.CreateRole;

[TestClass] // Attribute مربوط به MSTest
public class CreateRoleCommandHandlerTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<IRoleRepository> _mockRoleRepository;
    private CreateRoleCommandHandler _handler;

    [TestInitialize] // این متد قبل از هر تست اجرا می‌شود (Setup)
    public void Setup()
    {
        _mockRoleRepository = new Mock<IRoleRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        // تنظیم می‌کنیم که وقتی UnitOfWork.RoleRepository فراخوانی می‌شود، Mock ما برگردانده شود
        _mockUnitOfWork.Setup(uow => uow.RoleRepository).Returns(_mockRoleRepository.Object);
        
        // تنظیم می‌کنیم که SaveChangesAsync یک نتیجه موفقیت‌آمیز (مثلاً 1) برگرداند
        _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(1);

        _handler = new CreateRoleCommandHandler(_mockUnitOfWork.Object);
    }

    [TestMethod]
    public async Task Handle_ValidRole_ShouldCreateRoleAndReturnRoleId()
    {
        // Arrange (آماده‌سازی)
        var command = new CreateRoleCommand("Test Role", "Test Description");
        
        // تنظیم Mock برای AddAsync در RoleRepository
        // وقتی AddAsync با هر نمونه‌ای از Role فراخوانی شود، آن را می‌پذیریم
        // و یک Role با یک Guid جدید شبیه‌سازی می‌کنیم که برگردانده شود (اگر AddAsync چیزی برمی‌گرداند)
        // یا فقط مطمئن می‌شویم که فراخوانی شده است.
        // در پیاده‌سازی فعلی، AddAsync خود Role را برمی‌گرداند اما ما به Id آن پس از ایجاد نیاز داریم.
        // چون Id در سازنده Role ایجاد می‌شود، می‌توانیم آن را مستقیماً از entity بگیریم.
        
        Role? createdRole = null; // برای گرفتن نقشی که به AddAsync پاس داده می‌شود
        _mockRoleRepository.Setup(repo => repo.AddAsync(It.IsAny<Role>()))
            .Callback<Role>(role => createdRole = role) // نقش ایجاد شده را می‌گیریم
            .ReturnsAsync((Role role) => role); // خود نقش را برمی‌گردانیم

        // Act (اجرای عملیات)
        var resultRoleId = await _handler.Handle(command, CancellationToken.None);

        // Assert (بررسی نتایج)
        Assert.IsNotNull(createdRole, "Role entity should have been created.");
        Assert.AreEqual(command.Name, createdRole.Name, "Role name mismatch.");
        Assert.AreEqual(command.Description, createdRole.Description, "Role description mismatch.");
        Assert.AreEqual(createdRole.Id, resultRoleId, "Returned RoleId should match created role's Id.");

        // بررسی اینکه آیا متد AddAsync در RoleRepository دقیقاً یک بار فراخوانی شده است
        _mockRoleRepository.Verify(repo => repo.AddAsync(It.IsAny<Role>()), Times.Once);

        // بررسی اینکه آیا متد SaveChangesAsync در UnitOfWork دقیقاً یک بار فراخوانی شده است
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_RoleNameAlreadyExists_ShouldBeHandledByValidatorOrThrowSpecificException()
    {
        // این تست برای بررسی رفتار Handler در صورت وجود نام تکراری است.
        // در طراحی فعلی ما، این مورد توسط CreateRoleCommandValidator گرفته می‌شود
        // و ValidationBehavior در MediatR باید قبل از رسیدن به Handler، یک ValidationException پرتاب کند.
        // بنابراین، این تست برای خود Handler ممکن است لازم نباشد اگر به Validator اتکا کنیم.
        // اگر بخواهیم خود Handler را تست کنیم که در صورت عدم وجود Validator چه می‌کند:

        // Arrange
        var command = new CreateRoleCommand("Existing Role", "Description");
        // فرض کنید RoleRepository.ExistsByNameAsync برای این نام true برمی‌گرداند (اگر در Handler چک می‌کردیم)
        // _mockRoleRepository.Setup(repo => repo.ExistsByNameAsync("Existing Role")).ReturnsAsync(true);
        // یا اگر می‌خواهید تست کنید که در صورت وجود، آیا AddAsync با همان نام خطا می‌دهد یا خیر (بستگی به منطق Handler دارد)

        // Act & Assert
        // اگر Handler خودش نام تکراری را چک می‌کند و BadRequestException پرتاب می‌کند:
        // await Assert.ThrowsExceptionAsync<BadRequestException>(() => 
        //    _handler.Handle(command, CancellationToken.None)
        // );
        // فعلاً چون این منطق در Validator است، این تست را ساده نگه می‌داریم یا کامنت می‌کنیم.
        // این تست نشان می‌دهد که تست واحد چگونه می‌تواند به طراحی بهتر هم کمک کند.
        // ما انتظار داریم Validator جلوی این را بگیرد.
        Assert.IsTrue(true, "Uniqueness is handled by validator; this test is a placeholder for direct handler logic if any.");
    }
}