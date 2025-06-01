// src/HotelReservation.Domain/Entities/BookingRequest.cs
using System;
using System.Collections.Generic;
using HotelReservation.Domain.Enums; // برای BookingStatus

namespace HotelReservation.Domain.Entities;

public class BookingRequest
{
    public Guid Id { get; private set; }
    public string TrackingCode { get; private set; } // کد رهگیری، Unique
    public string RequestingEmployeeNationalCode { get; private set; } // کد ملی کارمند اصلی درخواست‌دهنده
    public string BookingPeriod { get; private set; } // مثلا "بهار 1404"
    public DateTime CheckInDate { get; private set; }
    public DateTime CheckOutDate { get; private set; }
    public int NumberOfNights => (CheckOutDate - CheckInDate).Days; // محاسبه شده
    public int TotalGuests { get; private set; } // مجموع کارمند و همراهان

    // ارتباط با هتل
    public Guid HotelId { get; private set; }
    public virtual Hotel Hotel { get; private set; }

    // ارتباط با کاربری که درخواست را ثبت کرده
    public Guid RequestSubmitterUserId { get; private set; }
    public virtual User RequestSubmitterUser { get; private set; }

    public BookingStatus Status { get; private set; }
    public DateTime SubmissionDate { get; private set; }
    public DateTime LastStatusUpdateDate { get; private set; }
    public string? Notes { get; private set; } // توضیحات اضافی

      // --- فیلدهای جدید برای ارتباط با اتاق تخصیص داده شده ---
    public Guid? AssignedRoomId { get; private set; } // Nullable، چون فقط پس از تایید اتاق تخصیص داده می‌شود
    public virtual Room? AssignedRoom { get; private set; } // Navigation Property
    // --- پایان فیلدهای جدید ---

    // Navigation Properties
    public virtual ICollection<BookingGuest> Guests { get; private set; } = new List<BookingGuest>();
    public virtual ICollection<BookingFile> Files { get; private set; } = new List<BookingFile>();
    public virtual ICollection<BookingStatusHistory> StatusHistory { get; private set; } = new List<BookingStatusHistory>();

    private BookingRequest() {} // برای EF Core

    public BookingRequest(
        string requestingEmployeeNationalCode,
        string bookingPeriod,
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
        BookingPeriod = bookingPeriod;
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
        AddStatusHistory(null, BookingStatus.Draft, requestSubmitterUserId, requestSubmitterUser, "ایجاد درخواست اولیه");
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

    public void UpdateStatus(BookingStatus newStatus, Guid changedByUserId, User changedByUser, string? comments = null)
    {
        if (Status == newStatus) return; // وضعیت تکراری نیست

        var oldStatus = Status;
        Status = newStatus;
        LastStatusUpdateDate = DateTime.UtcNow;
        AddStatusHistory(oldStatus, newStatus, changedByUserId, changedByUser, comments);

         // اگر وضعیت به چیزی غیر از HotelApproved تغییر کرد، اتاق تخصیص داده شده را آزاد می‌کنیم
         if (newStatus != BookingStatus.HotelApproved)
         {
             ClearAssignedRoom();
         }
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
    
    private void AddStatusHistory(BookingStatus? oldStatus, BookingStatus newStatus, Guid changedByUserId, User changedByUser, string? comments)
    {
        var historyEntry = new BookingStatusHistory(this.Id, this, newStatus, changedByUserId, changedByUser, comments, oldStatus);
        StatusHistory.Add(historyEntry);
    }

    public void UpdateBookingDetails(string bookingPeriod, DateTime checkInDate, DateTime checkOutDate, int totalGuests, string? notes)
    {
        // اعتبارسنجی‌ها
        if (checkOutDate <= checkInDate)
            throw new ArgumentException("تاریخ خروج باید بعد از تاریخ ورود باشد.");
        if (totalGuests <= 0)
            throw new ArgumentException("تعداد مهمانان باید حداقل یک نفر باشد.", nameof(totalGuests));
        
        BookingPeriod = bookingPeriod;
        CheckInDate = checkInDate;
        CheckOutDate = checkOutDate;
        TotalGuests = totalGuests;
        Notes = notes;
        LastStatusUpdateDate = DateTime.UtcNow;
    }
}