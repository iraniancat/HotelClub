namespace HotelReservation.Domain.Entities;

public class BookingPeriod
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public bool IsActive { get; private set; } // مشخص می‌کند آیا این دوره برای رزرو جدید فعال است یا خیر

    private BookingPeriod() { } // برای EF Core

    public BookingPeriod(string name, DateTime startDate, DateTime endDate, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("نام دوره زمانی الزامی است.", nameof(name));
        if (endDate <= startDate) throw new ArgumentException("تاریخ پایان باید بعد از تاریخ شروع باشد.", nameof(endDate));

        Id = Guid.NewGuid();
        Name = name.Trim();
        StartDate = startDate;
        EndDate = endDate;
        IsActive = isActive;
    }

    public void UpdateDetails(string name, DateTime startDate, DateTime endDate, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("نام دوره زمانی الزامی است.", nameof(name));
        if (endDate <= startDate) throw new ArgumentException("تاریخ پایان باید بعد از تاریخ شروع باشد.", nameof(endDate));

        Name = name.Trim();
        StartDate = startDate;
        EndDate = endDate;
        IsActive = isActive;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}