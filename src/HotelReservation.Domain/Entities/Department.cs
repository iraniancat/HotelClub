// src/HotelReservation.Domain/Entities/Department.cs
using System;
using System.Collections.Generic;

namespace HotelReservation.Domain.Entities;

public class Department
{
    public string Code { get; private set; } // کلید اصلی - کد اداره/شعبه
    public string Name { get; private set; }

    // Navigation Property: کاربرانی که به این اداره/شعبه تعلق دارند
    public virtual ICollection<User> Users { get; private set; } = new List<User>();

    private Department() {} // برای EF Core

    public Department(string code, string name)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("کد اداره/شعبه نمی‌تواند خالی باشد.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("نام اداره/شعبه نمی‌تواند خالی باشد.", nameof(name));

        Code = code; // یا code.ToUpper();
        Name = name;
    }

    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("نام جدید اداره/شعبه نمی‌تواند خالی باشد.", nameof(newName));
        Name = newName;
    }
}