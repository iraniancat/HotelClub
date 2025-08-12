namespace HotelReservation.Infrastructure.Services.Sms;

public class SmsGatewaySettings
{
    public const string SectionName = "SmsGatewaySettings";
    public bool UseFakeSmsService { get; set; } = true; // پیش‌فرض برای امنیت بیشتر
    public string AsmxServiceUrl { get; set; } = string.Empty;
    public string SoapAction { get; set; } = string.Empty;
}