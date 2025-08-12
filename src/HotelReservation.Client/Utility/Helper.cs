using System.Globalization;
using MudBlazor;

public static class Helper
{
    public static string ToPersianDateString(DateTime dt)
    {
        if (dt == default) return string.Empty;
        var pc = new PersianCalendar();
        return $"{pc.GetYear(dt)}/{pc.GetMonth(dt):D2}/{pc.GetDayOfMonth(dt):D2}";
    }

    public static string GetPersianStatus(string status)
    {
        if (Enum.TryParse<HotelReservation.Domain.Enums.BookingStatus>(status, true, out var statusEnum))
        {
            return statusEnum switch
            {
                HotelReservation.Domain.Enums.BookingStatus.Draft => "پیش‌نویس",
                HotelReservation.Domain.Enums.BookingStatus.SubmittedToHotel => "ارسال به هتل",
                HotelReservation.Domain.Enums.BookingStatus.AwaitingProvinceApproval => "منتظر تایید استان",
                HotelReservation.Domain.Enums.BookingStatus.ProvinceRejected => "عدم تایید توسط استان",
                HotelReservation.Domain.Enums.BookingStatus.HotelApproved => "تأیید شده",
                HotelReservation.Domain.Enums.BookingStatus.HotelRejected => "رد شده",
                HotelReservation.Domain.Enums.BookingStatus.CancelledByUser => "لغو شده",
                HotelReservation.Domain.Enums.BookingStatus.Completed => "تکمیل شده",
                _ => status
            };
        }
        return status;
    }
    public static string GetPersianStatus(HotelReservation.Domain.Enums.BookingStatus status)
    {
        return status switch
        {
            HotelReservation.Domain.Enums.BookingStatus.Draft => "پیش‌نویس",
            HotelReservation.Domain.Enums.BookingStatus.SubmittedToHotel => "ارسال به هتل",
            HotelReservation.Domain.Enums.BookingStatus.AwaitingProvinceApproval => "منتظر تایید استان",
            HotelReservation.Domain.Enums.BookingStatus.ProvinceRejected => "عدم تایید توسط استان",
            HotelReservation.Domain.Enums.BookingStatus.HotelApproved => "تأیید شده",
            HotelReservation.Domain.Enums.BookingStatus.HotelRejected => "رد شده",
            HotelReservation.Domain.Enums.BookingStatus.CancelledByUser => "لغو شده",
            HotelReservation.Domain.Enums.BookingStatus.Completed => "تکمیل شده",
            _ => status.ToString()
        };
    }
    public static Color GetStatusColor(string status)
    {
        if (Enum.TryParse<HotelReservation.Domain.Enums.BookingStatus>(status, true, out var statusEnum))
        {
            return statusEnum switch
            {
                HotelReservation.Domain.Enums.BookingStatus.HotelApproved => Color.Success,
                HotelReservation.Domain.Enums.BookingStatus.HotelRejected => Color.Error,
                HotelReservation.Domain.Enums.BookingStatus.CancelledByUser => Color.Warning,
                HotelReservation.Domain.Enums.BookingStatus.SubmittedToHotel => Color.Info,
                HotelReservation.Domain.Enums.BookingStatus.AwaitingProvinceApproval => Color.Primary,
                HotelReservation.Domain.Enums.BookingStatus.ProvinceRejected => Color.Error,
                _ => Color.Default
            };
        }
        return Color.Default;
    }

    public static string ConvertPersianToEnglishNumerals(string persianStr)
    {
        if (string.IsNullOrWhiteSpace(persianStr)) return persianStr;
        return persianStr.Replace('۰', '0').Replace('۱', '1').Replace('۲', '2').Replace('۳', '3').Replace('۴', '4')
                         .Replace('۵', '5').Replace('۶', '6').Replace('۷', '7').Replace('۸', '8').Replace('۹', '9')
                         .Replace('٠', '0').Replace('١', '1').Replace('٢', '2').Replace('٣', '3').Replace('٤', '4')
                         .Replace('٥', '5').Replace('٦', '6').Replace('٧', '7').Replace('٨', '8').Replace('٩', '9');
    }

    public static DateTime ToGregorianDateTime(string persianDate)
    {
        if (string.IsNullOrWhiteSpace(persianDate)) throw new ArgumentNullException(nameof(persianDate));
        string englishNumeralsDate = ConvertPersianToEnglishNumerals(persianDate);
        var persianCulture = new CultureInfo("fa-IR") { DateTimeFormat = { Calendar = new PersianCalendar() } };
        try
        {
            return DateTime.Parse(englishNumeralsDate, persianCulture);
        }
        catch (FormatException ex)
        {
            throw new FormatException("فرمت تاریخ شمسی وارد شده نامعتبر است.", ex);
        }
    }

}