// src/HotelReservation.Domain/Entities/BookingGuest.cs
using System;

namespace HotelReservation.Domain.Entities;

public class BookingGuest
{
    public Guid Id { get; private set; }
    public string FullName { get; private set; }
    public string NationalCode { get; private set; } // کد ملی مهمان
    public string RelationshipToEmployee { get; private set; } // نسبت با کارمند: خود کارمند، تحت تکفل، همراه عادی
    public decimal DiscountPercentage { get; private set; } // درصد تخفیف: 80 یا 65

    // کلید خارجی و Navigation Property برای درخواست رزرو
    public Guid BookingRequestId { get; private set; }
    public virtual BookingRequest BookingRequest { get; private set; }

    private BookingGuest() {} // برای EF Core

    public BookingGuest(Guid bookingRequestId, BookingRequest bookingRequest, string fullName, string nationalCode, string relationshipToEmployee, decimal discountPercentage)
    {
        // اعتبارسنجی ...
        if (discountPercentage < 0 || discountPercentage > 100)
            throw new ArgumentOutOfRangeException(nameof(discountPercentage), "درصد تخفیف نامعتبر است.");

        Id = Guid.NewGuid();
        BookingRequestId = bookingRequestId;
        BookingRequest = bookingRequest;
        FullName = fullName;
        NationalCode = nationalCode;
        RelationshipToEmployee = relationshipToEmployee;
        DiscountPercentage = discountPercentage;
    }
}