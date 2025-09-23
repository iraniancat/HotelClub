namespace HotelReservation.Infrastructure.Services.Sms;

public class SmsGatewaySettings
{
    public bool UseFakeSmsService { get; set; } = true; 
    public string Number { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string password { get; set; } = string.Empty;
    public string IP { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string AsmxServiceUrl { get; set; } = string.Empty;
    public string SoapAction { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
}