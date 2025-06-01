// src/HotelReservation.Domain/Entities/BookingFile.cs
using System;

namespace HotelReservation.Domain.Entities;

public class BookingFile
{
    public Guid Id { get; private set; }
    public string FileName { get; private set; }
    public string FilePathOrContentIdentifier { get; private set; } // مسیر فایل یا شناسه در Blob Storage یا محتوای Base64 کوچک
    public string ContentType { get; private set; } // Word, PDF
    public DateTime UploadedDate { get; private set; }

    // کلید خارجی و Navigation Property برای درخواست رزرو
    public Guid BookingRequestId { get; private set; }
    public virtual BookingRequest BookingRequest { get; private set; }

    private BookingFile() {} // برای EF Core

    public BookingFile(Guid bookingRequestId, BookingRequest bookingRequest, string fileName, string filePathOrContentIdentifier, string contentType)
    {
        // اعتبارسنجی ...
        Id = Guid.NewGuid();
        BookingRequestId = bookingRequestId;
        BookingRequest = bookingRequest;
        FileName = fileName;
        FilePathOrContentIdentifier = filePathOrContentIdentifier;
        ContentType = contentType;
        UploadedDate = DateTime.UtcNow;
    }
}