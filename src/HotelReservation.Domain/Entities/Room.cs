namespace HotelReservation.Domain.Entities;

public class Room
{
    // ... خصوصیات موجود ...
    public Guid Id { get; private set; }
    public string RoomNumber { get; private set; }
    public int Capacity { get; private set; }
    public decimal PricePerNight { get; private set; }
    public bool IsActive { get; private set; }
    public Guid HotelId { get; private set; }
    public virtual Hotel Hotel { get; private set; }
    
    // ... سازنده ...
      public Room(string roomNumber, int capacity, decimal pricePerNight, Guid hotelId, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(roomNumber)) throw new ArgumentException("شماره اتاق الزامی است.", nameof(roomNumber));
        if (capacity <= 0) throw new ArgumentException("ظرفیت باید مثبت باشد.", nameof(capacity));
        if (pricePerNight <= 0) throw new ArgumentException("قیمت باید مثبت باشد.", nameof(pricePerNight));
        if (hotelId == Guid.Empty) throw new ArgumentException("شناسه هتل الزامی است.", nameof(hotelId));

        Id = Guid.NewGuid();
        RoomNumber = roomNumber.Trim();
        Capacity = capacity;
        PricePerNight = pricePerNight;
        HotelId = hotelId;
        IsActive = isActive;
        // Hotel navigation property توسط EF Core بر اساس HotelId مقداردهی می‌شود.
    }

    public void UpdateDetails(string roomNumber, int capacity, decimal pricePerNight, bool isActive)
    {
        RoomNumber = roomNumber;
        Capacity = capacity;
        PricePerNight = pricePerNight;
        IsActive = isActive;
    }
}