// src/HotelReservation.Domain/Entities/BookingRequest.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HotelReservation.Domain.Enums; // برای BookingStatus

namespace HotelReservation.Domain.Entities;

public class BookingRequest
{
    public Guid Id { get; private set; }
    public string TrackingCode { get; private set; }
    public string RequestingEmployeeNationalCode { get; private set; }
    public virtual User RequestingEmployee { get; private set; }
    public Guid BookingPeriodId { get; private set; }
    public virtual BookingPeriod BookingPeriod { get; private set; }
    public DateTime CheckInDate { get; private set; }
    public DateTime CheckOutDate { get; private set; }
    public int NumberOfNights => (CheckOutDate - CheckInDate).Days;
    public int TotalGuests { get; private set; }
    public Guid HotelId { get; private set; }
    public virtual Hotel Hotel { get; private set; }
    public Guid RequestSubmitterUserId { get; private set; }
    public virtual User RequestSubmitterUser { get; private set; }
    public BookingStatus Status { get; private set; }
    public DateTime SubmissionDate { get; private set; }
    public DateTime LastStatusUpdateDate { get; private set; }
    public string? Notes { get; private set; }
    public Guid? AssignedRoomId { get; private set; }
    public virtual Room? AssignedRoom { get; private set; }
    public virtual ICollection<BookingGuest> Guests { get; private set; } = new List<BookingGuest>();
    public virtual ICollection<BookingFile> Files { get; private set; } = new List<BookingFile>();
    public virtual ICollection<BookingStatusHistory> StatusHistory { get; private set; } = new List<BookingStatusHistory>();

    // <<-- خصوصیت جدید برای مدیریت همزمانی -->>
    [Timestamp]
    public byte[] RowVersion { get; private set; }

    private BookingRequest() { } // برای EF Core

    public BookingRequest(
        string requestingEmployeeNationalCode,        
        Guid bookingPeriodId, //BookingPeriod bookingPeriod,
        DateTime checkInDate,
        DateTime checkOutDate,
        int totalGuests,
        Guid hotelId,
        Hotel hotel,
        Guid requestSubmitterUserId,
        User requestSubmitterUser,
        string? notes = null)
    {
        // اعتبارسنجی‌های مهم
        if (checkOutDate <= checkInDate)
            throw new ArgumentException("تاریخ خروج باید بعد از تاریخ ورود باشد.");
        if (totalGuests <= 0)
            throw new ArgumentException("تعداد مهمانان باید حداقل یک نفر باشد.", nameof(totalGuests));
        // سایر اعتبارسنجی‌ها ...

        Id = Guid.NewGuid();
        TrackingCode = GenerateTrackingCode(); // متد کمکی برای تولید کد رهگیری
        RequestingEmployeeNationalCode = requestingEmployeeNationalCode;        
        BookingPeriodId = bookingPeriodId;
        //BookingPeriod = bookingPeriod;
        CheckInDate = checkInDate;
        CheckOutDate = checkOutDate;
        TotalGuests = totalGuests;
        HotelId = hotelId;
        Hotel = hotel;
        RequestSubmitterUserId = requestSubmitterUserId;
        RequestSubmitterUser = requestSubmitterUser;
        Notes = notes;
        Status = BookingStatus.Draft; // وضعیت اولیه
        SubmissionDate = DateTime.UtcNow;
        LastStatusUpdateDate = DateTime.UtcNow;
        // AssignedRoomId و AssignedRoom در ابتدا null هستند

        // اولین رکورد تاریخچه وضعیت
        AddStatusHistory(null, BookingStatus.Draft, requestSubmitterUserId, "ایجاد درخواست اولیه");
    }

    private string GenerateTrackingCode()
    {
        // یک نمونه ساده برای تولید کد رهگیری
        return $"HR-{DateTime.UtcNow.Ticks.ToString().Substring(10)}";
    }

    // متدهای جدید برای مدیریت اتاق تخصیص داده شده
    public void AssignRoom(Guid roomId, Room room)
    {
        if (Status != BookingStatus.HotelApproved && Status != BookingStatus.SubmittedToHotel) // فقط در این وضعیت‌ها می‌توان اتاق تخصیص داد قبل از نهایی شدن تایید
        {
            // یا می‌توان این بررسی را در Handler انجام داد
            // throw new InvalidOperationException("فقط برای درخواست‌های در حال بررسی یا تایید شده می‌توان اتاق تخصیص داد.");
        }
        AssignedRoomId = roomId;
        AssignedRoom = room;
    }

    public void ClearAssignedRoom()
    {
        AssignedRoomId = null;
        AssignedRoom = null;
    }

    public BookingStatusHistory UpdateStatus(BookingStatus newStatus, Guid changedByUserId, string? comments)
    {
        if (Status == newStatus)
        {
            // اگر وضعیت تغییر نکرده، یک تاریخچه خالی یا null برمی‌گردانیم تا در Handler پردازش نشود
            return null; // یا یک Exception پرتاب شود
        }

        var oldStatus = Status;
        Status = newStatus;
        LastStatusUpdateDate = DateTime.UtcNow;

        if (newStatus != BookingStatus.HotelApproved)
        {
            ClearAssignedRoom();
        }

        // ایجاد و بازگرداندن موجودیت تاریخچه جدید
        return new BookingStatusHistory(this.Id, newStatus, changedByUserId, comments, oldStatus);
    }
    private void AddStatusHistory(BookingStatus? oldStatus, BookingStatus newStatus, Guid changedByUserId, string? comments)
    {
        // ما دیگر شیء کامل User را اینجا پاس نمی‌دهیم، فقط شناسه آن را.
        // EF Core به طور خودکار رابطه را بر اساس کلید خارجی برقرار می‌کند.
        var historyEntry = new BookingStatusHistory(
            this.Id,
            newStatus,
            changedByUserId,
            comments,
            oldStatus);

        StatusHistory.Add(historyEntry);
    }

    public void AddGuest(string fullName, string nationalCode, string relationshipToEmployee, decimal discountPercentage)
    {
        var guest = new BookingGuest(this.Id, this, fullName, nationalCode, relationshipToEmployee, discountPercentage);
        Guests.Add(guest);
        // می‌توان TotalGuests را هم اینجا به‌روز کرد یا هنگام ایجاد درخواست محاسبه کرد
    }

    public void AddFile(string fileName, string filePathOrContent, string contentType)
    {
        var file = new BookingFile(this.Id, this, fileName, filePathOrContent, contentType);
        Files.Add(file);
    }

    private void AddStatusHistory(BookingStatusHistory historyEntry)
    {
        StatusHistory.Add(historyEntry);
    }

    public void UpdateBookingDetails(Guid bookingPeriodId, DateTime checkInDate, DateTime checkOutDate, int totalGuests, string? notes)
    {
        // اعتبارسنجی‌ها
        if (checkOutDate <= checkInDate)
            throw new ArgumentException("تاریخ خروج باید بعد از تاریخ ورود باشد.");
        if (totalGuests <= 0)
            throw new ArgumentException("تعداد مهمانان باید حداقل یک نفر باشد.", nameof(totalGuests));

        BookingPeriodId = bookingPeriodId;
        CheckInDate = checkInDate;
        CheckOutDate = checkOutDate;
        TotalGuests = totalGuests;
        Notes = notes;
        LastStatusUpdateDate = DateTime.UtcNow;
    }
}