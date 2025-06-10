// src/HotelReservation.Domain/Entities/BookingStatusHistory.cs
using System;
using HotelReservation.Domain.Enums; // برای BookingStatus

namespace HotelReservation.Domain.Entities;

public class BookingStatusHistory
{
     public Guid Id { get; private set; }
    public Guid BookingRequestId { get; private set; }
    public virtual BookingRequest BookingRequest { get; private set; }
    public BookingStatus NewStatus { get; private set; }
    public BookingStatus? OldStatus { get; private set; }
    public DateTime ChangeDate { get; private set; }
    public Guid ChangedByUserId { get; private set; }
    public virtual User ChangedByUser { get; private set; }
    public string? Comments { get; private set; }

    private BookingStatusHistory() {} // برای EF Core

    public BookingStatusHistory(
        Guid bookingRequestId, 
        BookingStatus newStatus, 
        Guid changedByUserId, 
        string? comments, 
        BookingStatus? oldStatus)
    {
        Id = Guid.NewGuid();
        BookingRequestId = bookingRequestId;
        NewStatus = newStatus;
        ChangedByUserId = changedByUserId; // فقط شناسه را ست می‌کنیم
        Comments = comments;
        OldStatus = oldStatus;
        ChangeDate = DateTime.UtcNow;
    }
}