// src/HotelReservation.Domain/Enums/BookingStatus.cs
namespace HotelReservation.Domain.Enums;

public enum BookingStatus
{
    Draft,                  // پیش‌نویس (هنوز نهایی نشده یا توسط کارمند ثبت شده و منتظر تایید اولیه)
    SubmittedToHotel,       // ارسال شده به هتل (پس از ثبت توسط مدیر/کاربر استان یا تایید اولیه درخواست کارمند)
    HotelApproved,          // تایید شده توسط هتل
    HotelRejected,          // رد شده توسط هتل
    AwaitingEmployeeApproval, // (برای فاز آتی) منتظر تایید کارمند پس از پیشنهاد هتل یا تغییرات
    CancelledByUser,        // لغو شده توسط کاربر/کارمند
    Completed,              // تکمیل شده (پس از اقامت)
    PaymentPending          // منتظر پرداخت (در صورت نیاز به فاز پرداخت)
    // سایر وضعیت‌های مورد نیاز
}