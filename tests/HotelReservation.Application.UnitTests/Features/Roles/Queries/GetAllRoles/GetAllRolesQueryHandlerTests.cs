// tests/HotelReservation.Application.UnitTests/Features/Roles/Queries/GetAllRoles/GetAllRolesQueryHandlerTests.cs
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using HotelReservation.Application.Contracts.Persistence; // برای IUnitOfWork و IRoleRepository
using HotelReservation.Application.Features.Roles.Queries.GetAllRoles; // برای Query و Handler
using HotelReservation.Application.DTOs.Role; // برای RoleDto
using HotelReservation.Domain.Entities; // برای Role
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic; // برای List<T>
using System.Linq; // برای Linq

namespace HotelReservation.Application.UnitTests.Features.Roles.Queries.GetAllRoles;

[TestClass]
public class GetAllRolesQueryHandlerTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<IRoleRepository> _mockRoleRepository;
    private GetAllRolesQueryHandler _handler;

    [TestInitialize]
    public void Setup()
    {
        _mockRoleRepository = new Mock<IRoleRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        // تنظیم می‌کنیم که وقتی UnitOfWork.RoleRepository فراخوانی می‌شود، Mock ما برگردانده شود
        _mockUnitOfWork.Setup(uow => uow.RoleRepository).Returns(_mockRoleRepository.Object);

        _handler = new GetAllRolesQueryHandler(_mockUnitOfWork.Object);
    }

    [TestMethod]
    public async Task Handle_WhenRolesExist_ShouldReturnListOfRoleDtos()
    {
        // Arrange (آماده‌سازی)
        var rolesFromRepository = new List<Role>
        {
            new Role("Admin", "Administrator role"),
            new Role("User", "Standard user role")
        };
        _mockRoleRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(rolesFromRepository);

        var query = new GetAllRolesQuery();

        // Act (اجرای عملیات)
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert (بررسی نتایج)
        Assert.IsNotNull(result, "نتیجه نباید null باشد.");
        Assert.IsInstanceOfType(result, typeof(List<RoleDto>), "نوع بازگشتی باید List<RoleDto> باشد.");
        Assert.AreEqual(2, result.Count(), "تعداد نقش‌های بازگشتی باید با تعداد نقش‌های موجود در ریپازیتوری برابر باشد.");

        // بررسی صحت نگاشت (Mapping) برای یکی از آیتم‌ها
        var firstRoleInResult = result.First();
        var firstRoleInSource = rolesFromRepository.First();
        Assert.AreEqual(firstRoleInSource.Id, firstRoleInResult.Id, "شناسه نقش اول مطابقت ندارد.");
        Assert.AreEqual(firstRoleInSource.Name, firstRoleInResult.Name, "نام نقش اول مطابقت ندارد.");
        Assert.AreEqual(firstRoleInSource.Description, firstRoleInResult.Description, "توضیحات نقش اول مطابقت ندارد.");

        // بررسی اینکه متد GetAllAsync دقیقاً یک بار فراخوانی شده است
        _mockRoleRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
    }

    [TestMethod]
    public async Task Handle_WhenNoRolesExist_ShouldReturnEmptyList()
    {
        // Arrange
        var emptyListOfRoles = new List<Role>();
        _mockRoleRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(emptyListOfRoles);

        var query = new GetAllRolesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsNotNull(result, "نتیجه نباید null باشد، حتی اگر لیستی خالی باشد.");
        Assert.AreEqual(0, result.Count(), "لیست بازگشتی باید خالی باشد.");

        _mockRoleRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
    }
}