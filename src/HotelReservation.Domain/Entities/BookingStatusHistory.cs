// src/HotelReservation.Domain/Entities/BookingStatusHistory.cs
using System;
using HotelReservation.Domain.Enums; // برای BookingStatus

namespace HotelReservation.Domain.Entities;

public class BookingStatusHistory
{
    public Guid Id { get; private set; }
    public BookingStatus? OldStatus { get; private set; } // وضعیت قبلی (می‌تواند null باشد برای اولین وضعیت)
    public BookingStatus NewStatus { get; private set; }   // وضعیت جدید
    public DateTime ChangeDate { get; private set; }
    public string? Comments { get; private set; }         // توضیحات مربوط به تغییر وضعیت

    // کلید خارجی و Navigation Property برای درخواست رزرو
    public Guid BookingRequestId { get; private set; }
    public virtual BookingRequest BookingRequest { get; private set; }

    // کلید خارجی و Navigation Property برای کاربری که وضعیت را تغییر داده
    public Guid ChangedByUserId { get; private set; }
    public virtual User ChangedByUser { get; private set; }

    private BookingStatusHistory() {} // برای EF Core

    public BookingStatusHistory(
        Guid bookingRequestId,
        BookingRequest bookingRequest,
        BookingStatus newStatus,
        Guid changedByUserId,
        User changedByUser,
        string? comments = null,
        BookingStatus? oldStatus = null)
    {
        // اعتبارسنجی ...
        Id = Guid.NewGuid();
        BookingRequestId = bookingRequestId;
        BookingRequest = bookingRequest;
        OldStatus = oldStatus;
        NewStatus = newStatus;
        ChangedByUserId = changedByUserId;
        ChangedByUser = changedByUser;
        Comments = comments;
        ChangeDate = DateTime.UtcNow;
    }
}