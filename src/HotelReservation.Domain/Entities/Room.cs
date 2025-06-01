// src/HotelReservation.Domain/Entities/Room.cs
using System;

namespace HotelReservation.Domain.Entities;

public class Room
{
    public Guid Id { get; private set; }
    public string RoomNumber { get; private set; }
    public int Capacity { get; private set; }
    public string RoomType { get; private set; }

    // کلید خارجی و Navigation Property برای هتل
    public Guid HotelId { get; private set; }
    public virtual Hotel Hotel { get; private set; }

    private Room() {} // برای EF Core

    // سازنده می‌تواند internal باشد اگر فقط از طریق Hotel.AddRoom ایجاد شود
    public Room(Guid hotelId, Hotel hotel, string roomNumber, int capacity, string roomType)
    {
        // اعتبارسنجی ...
        Id = Guid.NewGuid();
        HotelId = hotelId;
        Hotel = hotel; // تخصیص مستقیم شیء هتل
        RoomNumber = roomNumber;
        Capacity = capacity;
        RoomType = roomType;
    }

    public void UpdateDetails(string roomNumber, int capacity, string roomType)
    {
        // اعتبارسنجی ...
        RoomNumber = roomNumber;
        Capacity = capacity;
        RoomType = roomType;
    }
}