using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Domain.Entities;

public class ProvinceHotelQuota
{
    public Guid Id { get; private set; }
    
    [MaxLength(10)]
    public string ProvinceCode { get; private set; }
    public virtual Province Province { get; private set; }
    
    public Guid HotelId { get; private set; }
    public virtual Hotel Hotel { get; private set; }

    public int RoomLimit { get; private set; } // تعداد اتاق‌های مجاز

    private ProvinceHotelQuota() { }

    public ProvinceHotelQuota(string provinceCode, Guid hotelId, int roomLimit)
    {
        Id = Guid.NewGuid();
        ProvinceCode = provinceCode;
        HotelId = hotelId;
        SetRoomLimit(roomLimit);
    }

    public void SetRoomLimit(int newLimit)
    {
        if (newLimit < 0)
        {
            throw new ArgumentException("تعداد اتاق مجاز نمی‌تواند منفی باشد.");
        }
        RoomLimit = newLimit;
    }
}