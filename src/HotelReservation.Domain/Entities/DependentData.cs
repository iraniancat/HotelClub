// src/HotelReservation.Domain/Entities/DependentData.cs
using System;

namespace HotelReservation.Domain.Entities;

public class DependentData
{
    public Guid Id { get; private set; }
    public string NationalCode { get; private set; }
    public string FullName { get; private set; }
    public string Relationship { get; private set; }

    // کلید خارجی و Navigation Property برای کاربر اصلی
    public Guid UserId { get; private set; } // << تغییر نام از EmployeeDataId
    public virtual User UserOwner { get; private set; } // << تغییر نام از Employee

    private DependentData() {} // برای EF Core

    public DependentData(Guid userId, User userOwner, string nationalCode, string fullName, string relationship)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("شناسه کاربر اصلی نمی‌تواند خالی باشد.", nameof(userId));
        // سایر اعتبارسنجی‌ها ...

        Id = Guid.NewGuid();
        UserId = userId;
        UserOwner = userOwner;
        NationalCode = nationalCode;
        FullName = fullName;
        Relationship = relationship;
    }
    public void UpdateInfo(string fullName, string relationship)
    {
        // اعتبارسنجی ...
        FullName = fullName;
        Relationship = relationship;
    }
}