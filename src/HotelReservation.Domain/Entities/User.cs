// src/HotelReservation.Domain/Entities/User.cs
using System;
using System.Collections.Generic;
// UserRole enum دیگر استفاده نمی‌شود، به جای آن از موجودیت Role استفاده می‌کنیم


namespace HotelReservation.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string SystemUserId { get; private set; } // شناسه کاربری سیستم (باید Unique باشد)
    public string FullName { get; private set; }
    public string? PhoneNumber { get; private set; } // <<-- فیلد جدید برای شماره تلفن
    public string? NationalCode { get; private set; } // کد ملی، Nullable
    public string PasswordHash { get; private set; }
    public bool IsActive { get; private set; } // این فیلد توسط مدیر ارشد کنترل می‌شود

    // اطلاعات سازمانی - Nullable برای کاربران غیر سازمانی
    public string? ProvinceCode { get; private set; } // کلید خارجی به Province
    public virtual Province? Province { get; private set; }
    public string? ProvinceName { get; private set; } // نام استان (Denormalized)

    public string? DepartmentCode { get; private set; } // کلید خارجی به Department
    public virtual Department? Department { get; private set; }
    public string? DepartmentName { get; private set; } // نام اداره/شعبه (Denormalized)

    public Guid RoleId { get; private set; }
    public virtual Role Role { get; private set; }

    public Guid? HotelId { get; private set; }
    public virtual Hotel? AssignedHotel { get; private set; }
    // ... سایر Collections ...
    public virtual ICollection<DependentData> Dependents { get; private set; } = new List<DependentData>();
    public virtual ICollection<BookingRequest> SubmittedBookingRequests { get; private set; } = new List<BookingRequest>();
    public virtual ICollection<BookingStatusHistory> StatusChangesByThisUser { get; private set; } = new List<BookingStatusHistory>();


    private User() { }

    // سازنده برای ایجاد کاربر جدید (مخصوصاً کاربران غیر سازمانی توسط مدیر ارشد)
    // سازنده را نیز برای پذیرش PhoneNumber اصلاح کنید (اختیاری در سازنده)
    public User(
        string systemUserId, string fullName, string passwordHash,
        Guid roleId, Role role, bool isActive = true,
        string? nationalCode = null, string? phoneNumber = null, // اضافه شدن phoneNumber
        string? provinceCode = null, Province? province = null, string? provinceName = null,
        string? departmentCode = null, Department? department = null, string? departmentName = null,
        Guid? hotelId = null, Hotel? assignedHotel = null)
    {
        // ... اعتبارسنجی‌های اصلی ...
        Id = Guid.NewGuid();
        SystemUserId = systemUserId;
        FullName = fullName;
        PasswordHash = passwordHash;
        RoleId = roleId;
        // ... بقیه سازنده ...
        IsActive = isActive;
        NationalCode = nationalCode;
        PhoneNumber = phoneNumber; // <<-- مقداردهی فیلد جدید
        ProvinceCode = provinceCode;
        Province = province;
        ProvinceName = provinceName;
        DepartmentCode = departmentCode;
        Department = department;
        DepartmentName = departmentName;
        if (role?.Name == "HotelUser")
        {
            if (!hotelId.HasValue || assignedHotel == null)
                throw new ArgumentException("کاربر هتل باید به یک هتل معتبر تخصیص داده شود.", nameof(hotelId));
            HotelId = hotelId;
            AssignedHotel = assignedHotel;
        }
        else
        {
            HotelId = null;
            AssignedHotel = null;
        }
    }

    // متد برای به‌روزرسانی شماره تلفن (در صورت نیاز)
    public void SetPhoneNumber(string? phoneNumber)
    {
        // می‌توان اعتبارسنجی برای فرمت شماره تلفن اضافه کرد
        PhoneNumber = phoneNumber;
    }

    // متدهای موجود برای تغییر وضعیت، نقش، هتل و ... همچنان معتبر هستند
    public void UpdateProfileDetailsForNonEmployee(string fullName, string? nationalCode, /* سایر فیلدهای قابل ویرایش توسط مدیر ارشد برای این دسته کاربران */
                                       string? provinceCode, Province? province, string? provinceName,
                                       string? departmentCode, Department? department, string? departmentName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("نام کامل نمی‌تواند خالی باشد.", nameof(fullName));
        // اعتبارسنجی برای سایر فیلدها
        FullName = fullName;
        NationalCode = nationalCode;
        ProvinceCode = provinceCode;
        Province = province;
        ProvinceName = provinceName;
        DepartmentCode = departmentCode;
        Department = department;
        DepartmentName = departmentName;
    }
    public void ChangePassword(string newPasswordHash) { /* ... */ PasswordHash = newPasswordHash; }
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
    // ... متدهای ChangeRole و AssignToHotel و ClearHotelAssignment ...
    public void ChangeRole(Guid newRoleId, Role newRole)
    {
        if (newRoleId == Guid.Empty) throw new ArgumentNullException(nameof(newRoleId));
        if (newRole == null) throw new ArgumentNullException(nameof(newRole));

        RoleId = newRoleId;
        Role = newRole;

        if (newRole.Name != "HotelUser") // فرض بر نام نقش
        {
            ClearHotelAssignment();
        }
    }

    public void AssignToHotel(Guid hotelId, Hotel hotel)
    {
        if (this.Role?.Name != "HotelUser") // بررسی نقش فعلی کاربر
            throw new InvalidOperationException("فقط کاربری با نقش کاربر هتل می‌تواند به هتل تخصیص داده شود.");
        if (hotelId == Guid.Empty) throw new ArgumentNullException(nameof(hotelId));
        if (hotel == null) throw new ArgumentNullException(nameof(hotel));

        HotelId = hotelId;
        AssignedHotel = hotel;
    }

    public void ClearHotelAssignment()
    {
        HotelId = null;
        AssignedHotel = null;
    }

    public void UpdateDetailsByAdmin(
       string fullName,
       bool isActive,
       Guid newRoleId, Role newRole,
       string? nationalCode,
       string? phoneNumber, // <<-- اضافه شد
       string? provinceCode, Province? province, string? provinceName,
       string? departmentCode, Department? department, string? departmentName,
       Guid? newHotelId, Hotel? newAssignedHotel)
    {
        // ... پیاده سازی قبلی ...
        FullName = fullName;
        NationalCode = nationalCode;
        PhoneNumber = phoneNumber; // <<-- به‌روزرسانی
                                   // ...
        if (isActive) Activate(); else Deactivate();
        ProvinceCode = provinceCode;
        Province = province;
        ProvinceName = provinceName;

        DepartmentCode = departmentCode;
        Department = department;
        DepartmentName = departmentName;

        if (this.RoleId != newRoleId || this.Role?.Name != newRole.Name)
        {
            ChangeRole(newRoleId, newRole);
        }

        if (newRole.Name == "HotelUser")
        {
            if (!newHotelId.HasValue || newHotelId.Value == Guid.Empty || newAssignedHotel == null)
            {
                throw new ArgumentException("برای نقش کاربر هتل، شناسه هتل معتبر الزامی است.");
            }
            AssignToHotel(newHotelId.Value, newAssignedHotel);
        }
        else if (this.Role?.Name == "HotelUser" && newRole.Name != "HotelUser")
        {
            // ChangeRole باید این را مدیریت کند
        }
    }
}