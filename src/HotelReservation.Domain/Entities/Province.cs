// src/HotelReservation.Domain/Entities/Province.cs
using System.Collections.Generic;

namespace HotelReservation.Domain.Entities;

public class Province
{
    public string Code { get; private set; } // کلید اصلی - کد استان
    public string Name { get; private set; }

    // Navigation Property: کاربرانی که به این استان منتسب هستند
    public virtual ICollection<User> Users { get; private set; } = new List<User>();

    private Province() {}

    public Province(string code, string name)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("کد استان نمی‌تواند خالی باشد.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("نام استان نمی‌تواند خالی باشد.", nameof(name));

        Code = code.ToUpper();
        Name = name;
    }

    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("نام جدید استان نمی‌تواند خالی باشد.", nameof(newName));
        Name = newName;
    }
}