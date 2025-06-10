// src/HotelReservation.Infrastructure/Persistence/AppDbContext.cs
using HotelReservation.Domain.Constants;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Enums; // برای BookingStatus
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders; // برای EntityTypeBuilder

namespace HotelReservation.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    // DbSet ها برای موجودیت‌های جدید و اصلاح شده
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Province> Provinces { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Hotel> Hotels { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<DependentData> DependentData { get; set; } // نام کلاس DependentData است
    public DbSet<BookingRequest> BookingRequests { get; set; }
    public DbSet<BookingGuest> BookingGuests { get; set; }
    public DbSet<BookingFile> BookingFiles { get; set; }
    public DbSet<BookingStatusHistory> BookingStatusHistories { get; set; }
    public DbSet<BookingPeriod> BookingPeriods { get; set; }


    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUser(modelBuilder.Entity<User>());
        ConfigureRole(modelBuilder.Entity<Role>());
        ConfigureProvince(modelBuilder.Entity<Province>());
        ConfigureDepartment(modelBuilder.Entity<Department>());
        ConfigureHotel(modelBuilder.Entity<Hotel>());
        ConfigureRoom(modelBuilder.Entity<Room>());
        ConfigureDependentData(modelBuilder.Entity<DependentData>());
        ConfigureBookingRequest(modelBuilder.Entity<BookingRequest>());
        ConfigureBookingStatusHistory(modelBuilder.Entity<BookingStatusHistory>());
        ConfigureBookingFile(modelBuilder.Entity<BookingFile>());
        ConfigureBookingPeriod(modelBuilder.Entity<BookingPeriod>());
        // ... (پیکربندی تبدیل Enum ها همانند قبل) ...
        modelBuilder.Entity<BookingRequest>()
            .Property(br => br.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        modelBuilder.Entity<BookingStatusHistory>()
            .Property(bsh => bsh.NewStatus)
            .HasConversion<string>()
            .HasMaxLength(50);

        modelBuilder.Entity<BookingStatusHistory>()
            .Property(bsh => bsh.OldStatus)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired(false);
    }

    private void ConfigureUser(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.SystemUserId)
            .IsRequired()
            .HasMaxLength(100);
        builder.HasIndex(u => u.SystemUserId)
            .IsUnique();

        builder.Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.NationalCode)
            .HasMaxLength(10)
            .IsRequired(false); // <<-- NationalCode می‌تواند null باشد
        builder.HasIndex(u => u.NationalCode)
            .IsUnique()
            .HasFilter("[NationalCode] IS NOT NULL");


        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.IsActive).IsRequired();

        // رابطه با Role (اجباری)
        builder.HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- اصلاحات برای اختیاری بودن اطلاعات سازمانی ---
        builder.Property(u => u.ProvinceCode)
            .HasMaxLength(10) // اگر طول مشخصی برای کد استان دارید
            .IsRequired(false); // <<-- ProvinceCode می‌تواند null باشد
        builder.HasOne(u => u.Province)
            .WithMany(p => p.Users)
            .HasForeignKey(u => u.ProvinceCode)
            .HasPrincipalKey(p => p.Code)
            .IsRequired(false) // <<-- رابطه اختیاری است
            .OnDelete(DeleteBehavior.SetNull); // اگر استان حذف شد، ProvinceCode کاربر null شود (یا Restrict اگر نمی‌خواهید استان دارای کاربر حذف شود)

        builder.Property(u => u.ProvinceName)
            .HasMaxLength(100)
            .IsRequired(false); // <<-- ProvinceName می‌تواند null باشد

        builder.Property(u => u.DepartmentCode)
            .HasMaxLength(20) // اگر طول مشخصی برای کد اداره دارید
            .IsRequired(false); // <<-- DepartmentCode می‌تواند null باشد
        builder.HasOne(u => u.Department)
            .WithMany(d => d.Users)
            .HasForeignKey(u => u.DepartmentCode)
            .HasPrincipalKey(d => d.Code)
            .IsRequired(false) // <<-- رابطه اختیاری است
            .OnDelete(DeleteBehavior.SetNull); // اگر دپارتمان حذف شد، DepartmentCode کاربر null شود (یا Restrict)

        builder.Property(u => u.DepartmentName)
            .HasMaxLength(150)
            .IsRequired(false); // <<-- DepartmentName می‌تواند null باشد
        // --- پایان اصلاحات اطلاعات سازمانی ---

        // رابطه با Hotel (اختیاری، برای HotelUser)
        builder.HasOne(u => u.AssignedHotel)
            .WithMany(h => h.HotelUsers)
            .HasForeignKey(u => u.HotelId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(u => u.Dependents)
            .WithOne(d => d.UserOwner)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        // داخل متد ConfigureUser در AppDbContext.cs
        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(20) // یا هر طول مناسب دیگر
            .IsRequired(false); // شماره تلفن می‌تواند اختیاری باشد

        //string superAdminPasswordHashForSeed = "YOUR_PRE_COMPUTED_HASH_FOR_DEFAULT_PASSWORD"; 
        // // هشدار: مقدار بالا را با هش واقعی یک رمز عبور قوی جایگزین کنید.
        // // برای مثال، اگر از پیاده‌سازی SHA256 ساده (بدون salt) استفاده کرده بودیم:
        // // SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes("!AdminPa$$w0rd")) -> BitConverter.ToString(...).Replace("-","").ToLower()

        // // برای استان و دپارتمان مدیر ارشد، می‌توانیم مقادیر پیش‌فرض یا null در نظر بگیریم
        // // یا اگر استان/دپارتمان خاصی برای "ستاد" دارید، کد آن را استفاده کنید.
        // // فرض می‌کنیم یک استان و دپارتمان "ستادی" با کدهای "HQ_PROV" و "HQ_DEPT" داریم (که باید آنها را هم Seed کنید).
        // // اگر استان و دپارتمان برای SuperAdmin لازم نیست، می‌توانید null پاس دهید (چون فیلدها Nullable هستند)

        // builder.HasData(
        //     new {
        //         Id = Guid.Parse("E0E2C45A-B935-4C7A-9A9A-4A5B6C7D8E9F"), // یک Guid ثابت برای SuperAdmin
        //         SystemUserId = "superadmin",
        //         FullName = "مدیر ارشد سیستم",
        //         NationalCode = (string?)null, // یا یک مقدار نمونه
        //         PasswordHash = superAdminPasswordHashForSeed, // <<-- هش رمز عبور
        //         IsActive = true,
        //         RoleId = RoleConstants.SuperAdminRoleId, // شناسه نقش SuperAdmin
        //         // اطلاعات سازمانی نمونه یا null اگر برای SuperAdmin عمومی کاربرد ندارد
        //         ProvinceCode = (string?)"00", // کد استان نمونه برای ستاد (باید در جدول استان‌ها Seed شود)
        //         ProvinceName = "ستاد مرکزی",
        //         DepartmentCode = (string?)"000", // کد دپارتمان نمونه برای ستاد (باید در جدول دپارتمان‌ها Seed شود)
        //         DepartmentName = "مدیریت کل",
        //         PhoneNumber = (string?)"02100000000", // شماره تلفن نمونه
        //         HotelId = (Guid?)null, // SuperAdmin به هتل خاصی منتسب نیست
        //         // AssignedHotel = null // Navigation property در HasData ست نمی‌شود
        //         // Province = null, Department = null // Navigation properties
        //     }
        // );
    }

    // ... (متدهای ConfigureRole, ConfigureProvince, ConfigureDepartment و سایر متدهای پیکربندی بدون تغییر باقی می‌مانند مگر اینکه نیاز به اصلاح داشته باشند) ...
    // فقط مطمئن شوید که در ConfigureProvince و ConfigureDepartment، رابطه با User به درستی مدیریت شده
    // (WithMany(p => p.Users) و .HasForeignKey(u => u.ProvinceCode) در User پیکربندی شده، پس اینجا نیاز به کار خاصی نیست)

    private void ConfigureRole(EntityTypeBuilder<Role> builder) // بدون تغییر
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(50);
        builder.HasIndex(r => r.Name)
            .IsUnique();
        builder.Property(r => r.Description).HasMaxLength(250);
        // Seed کردن نقش‌های پیش‌فرض
        // builder.HasData(
        // new { Id = RoleConstants.SuperAdminRoleId, Name = RoleConstants.SuperAdminRoleName, Description = "دسترسی کامل به سیستم" },
        // new { Id = RoleConstants.ProvinceUserRoleId, Name = RoleConstants.ProvinceUserRoleName, Description = "کاربر با دسترسی در سطح استان" },
        // new { Id = RoleConstants.HotelUserRoleId, Name = RoleConstants.HotelUserRoleName, Description = "کاربر با دسترسی در سطح هتل" },
        // new { Id = RoleConstants.EmployeeRoleId, Name = RoleConstants.EmployeeRoleName, Description = "کارمند عادی سازمان (برای فازهای آتی)" }
        //);
    }

    private void ConfigureProvince(EntityTypeBuilder<Province> builder) // بدون تغییر
    {
        builder.HasKey(p => p.Code);
        builder.Property(p => p.Code).HasMaxLength(10);
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);
    }

    private void ConfigureDepartment(EntityTypeBuilder<Department> builder) // بدون تغییر
    {
        builder.HasKey(d => d.Code);
        builder.Property(d => d.Code).HasMaxLength(20);
        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(150);
    }
    private void ConfigureHotel(EntityTypeBuilder<Hotel> builder) // بدون تغییر
    {
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Name)
            .IsRequired()
            .HasMaxLength(150);
        builder.Property(h => h.Address)
            .IsRequired()
            .HasMaxLength(500);
    }

    private void ConfigureRoom(EntityTypeBuilder<Room> builder) // بدون تغییر
    {
        builder.HasKey(r => r.Id);
        builder.HasOne(r => r.Hotel)
            .WithMany(h => h.Rooms)
            .HasForeignKey(r => r.HotelId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private void ConfigureDependentData(EntityTypeBuilder<DependentData> builder) // بدون تغییر
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.NationalCode)
            .IsRequired()
            .HasMaxLength(10);
    }

    private void ConfigureBookingRequest(EntityTypeBuilder<BookingRequest> builder) // بدون تغییر
    {
        builder.HasKey(br => br.Id);
        builder.Property(br => br.TrackingCode)
            .IsRequired()
            .HasMaxLength(50);
        builder.HasIndex(br => br.TrackingCode)
            .IsUnique();

        builder.Property(br => br.RequestingEmployeeNationalCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.HasOne(br => br.Hotel)
            .WithMany(h => h.BookingRequests)
            .HasForeignKey(br => br.HotelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(br => br.RequestSubmitterUser)
            .WithMany(u => u.SubmittedBookingRequests)
            .HasForeignKey(br => br.RequestSubmitterUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(br => br.Guests)
            .WithOne(bg => bg.BookingRequest)
            .HasForeignKey(bg => bg.BookingRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(br => br.Files)
            .WithOne(bf => bf.BookingRequest)
            .HasForeignKey(bf => bf.BookingRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(br => br.StatusHistory)
            .WithOne(bsh => bsh.BookingRequest)
            .HasForeignKey(bsh => bsh.BookingRequestId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(br => br.BookingPeriod)
                    .WithMany() // یک دوره می‌تواند چندین رزرو داشته باشد
                    .HasForeignKey(br => br.BookingPeriodId)
                    .OnDelete(DeleteBehavior.Restrict); // اگر دوره‌ای رزرو دارد، قابل حذف نیست
        // --- پیکربندی رابطه جدید با Room ---
        builder.HasOne(br => br.AssignedRoom)              // هر درخواست رزرو می‌تواند به یک اتاق تخصیص داده شود
            .WithMany()                                // یک اتاق می‌تواند به چندین درخواست رزرو تخصیص داده شود (در زمان‌های مختلف)
                                                       // اگر نمی‌خواهید Navigation Property از Room به BookingRequests داشته باشید، WithMany() خالی بگذارید.
                                                       // اگر می‌خواهید، باید ICollection<BookingRequest> Bookings را به Room اضافه کنید و اینجا WithMany(r => r.Bookings) بنویسید.
            .HasForeignKey(br => br.AssignedRoomId)        // کلید خارجی در BookingRequest
            .IsRequired(false)                           // این رابطه اختیاری است (AssignedRoomId می‌تواند null باشد)
            .OnDelete(DeleteBehavior.SetNull);            // اگر اتاق حذف شد، AssignedRoomId در رزرو null شود (یا Restrict اگر نمی‌خواهید اتاق دارای رزرو حذف شود)
                                                          // --- پایان پیکربندی رابطه با Room ---
        builder.Property(b => b.RowVersion).IsRowVersion();
    }

    private void ConfigureBookingStatusHistory(EntityTypeBuilder<BookingStatusHistory> builder) // بدون تغییر
    {
        builder.HasKey(bsh => bsh.Id);

        builder.HasOne(bsh => bsh.ChangedByUser)
            .WithMany(u => u.StatusChangesByThisUser)
            .HasForeignKey(bsh => bsh.ChangedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
    private void ConfigureBookingFile(EntityTypeBuilder<BookingFile> builder) // متد جدید
    {
        builder.HasKey(bf => bf.Id);
        builder.Property(bf => bf.FileName).IsRequired().HasMaxLength(255);
        builder.Property(bf => bf.FilePathOrContentIdentifier).IsRequired().HasMaxLength(500);
        builder.Property(bf => bf.ContentType).IsRequired().HasMaxLength(100);

        // رابطه با BookingRequest قبلاً از طریق Navigation Property در BookingRequest
        // و HasMany(br => br.Files).WithOne(bf => bf.BookingRequest)...OnDelete(DeleteBehavior.Cascade)
        // پیکربندی شده است. اگر نشده، اینجا اضافه کنید.
    }
    private void ConfigureBookingPeriod(EntityTypeBuilder<BookingPeriod> builder)
    {
        builder.HasKey(bp => bp.Id);
        builder.Property(bp => bp.Name).IsRequired().HasMaxLength(150);
        builder.HasIndex(bp => bp.Name).IsUnique(); // نام دوره باید یکتا باشد
        builder.Property(bp => bp.IsActive).IsRequired();
    }
}