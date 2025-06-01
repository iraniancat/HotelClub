// src/HotelReservation.Domain/Entities/Hotel.cs
using System;
using System.Collections.Generic;

namespace HotelReservation.Domain.Entities;

public class Hotel
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Address { get; private set; }
    public string? ContactPerson { get; private set; }
    public string? PhoneNumber { get; private set; }

    // << حذف شد: کلید خارجی و Navigation Property برای استان >>
    // public string ProvinceCode { get; private set; }
    // public virtual Province Province { get; private set; }

    public virtual ICollection<Room> Rooms { get; private set; } = new List<Room>();
    public virtual ICollection<User> HotelUsers { get; private set; } = new List<User>();
    public virtual ICollection<BookingRequest> BookingRequests { get; private set; } = new List<BookingRequest>();

    private Hotel() {} // برای EF Core

    // سازنده اصلاح شده
    public Hotel(string name, string address, string? contactPerson = null, string? phoneNumber = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("نام هتل نمی‌تواند خالی باشد.", nameof(name));
        if (string.IsNullOrWhiteSpace(address)) // آدرس همچنان مهم است
            throw new ArgumentException("آدرس هتل نمی‌تواند خالی باشد.", nameof(address));
        // سایر اعتبارسنجی‌ها ...

        Id = Guid.NewGuid();
        Name = name;
        Address = address;
        // ProvinceCode و Province حذف شدند
        ContactPerson = contactPerson;
        PhoneNumber = phoneNumber;
    }

    public void UpdateDetails(string name, string address, string? contactPerson, string? phoneNumber)
    {
        // اعتبارسنجی ...
        Name = name;
        Address = address;
        ContactPerson = contactPerson;
        PhoneNumber = phoneNumber;
    }

    public void AddRoom(string roomNumber, int capacity, string roomType)
    {
        // سازنده Room دیگر Province را به عنوان ورودی از Hotel نمی‌گیرد
        var room = new Room(this.Id, this, roomNumber, capacity, roomType);
        Rooms.Add(room);
    }
}