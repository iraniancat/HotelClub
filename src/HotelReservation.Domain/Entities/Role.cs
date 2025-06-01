// src/HotelReservation.Domain/Entities/Role.cs
using System;
using System.Collections.Generic;

namespace HotelReservation.Domain.Entities;

public class Role
{
    public Guid Id { get; private set; } // یا int اگر ترجیح می‌دهید
    public string Name { get; private set; } // نام نقش، مثلاً "SuperAdmin", "ProvinceUser", "HotelUser", "Employee" - باید Unique باشد
    public string? Description { get; private set; }

    // Navigation Property: کاربرانی که این نقش را دارند
    public virtual ICollection<User> Users { get; private set; } = new List<User>();

    private Role() {} // برای EF Core

    public Role(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("نام نقش نمی‌تواند خالی باشد.", nameof(name));

        Id = Guid.NewGuid();
        Name = name;
        Description = description;
    }

   // <<-- متد جدید برای به‌روزرسانی -->>
    public void UpdateDetails(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("نام نقش نمی‌تواند خالی باشد.", nameof(name));
        
        Name = name.Trim();
        Description = description?.Trim();
    }
}