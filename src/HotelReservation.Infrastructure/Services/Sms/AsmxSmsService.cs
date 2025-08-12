namespace HotelReservation.Infrastructure.Services.Sms;

using HotelReservation.Application.Contracts.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

public class AsmxSmsService : ISmsService
{
    private readonly HttpClient _httpClient;
    private readonly SmsGatewaySettings _settings;
    private readonly ILogger<AsmxSmsService> _logger;

    public AsmxSmsService(
        HttpClient httpClient, 
        IOptions<SmsGatewaySettings> settings, 
        ILogger<AsmxSmsService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendSmsAsync(string mobileNumber, string message)
    {
        // ساخت بدنه درخواست SOAP XML
        var soapEnvelope = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Body>
    <sendsms xmlns=""{_settings.SoapAction.Replace("/sendsms", "")}"">
      <mobilenumber>{mobileNumber}</mobilenumber>
      <message>{message}</message>
    </sendsms>
  </soap:Body>
</soap:Envelope>";

        // ایجاد محتوای درخواست HTTP
        var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
        
        // افزودن هدر ضروری SOAPAction
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("SOAPAction", _settings.SoapAction);

        _logger.LogInformation("Sending REAL SMS to {MobileNumber} via ASMX service at {Url}", mobileNumber, _settings.AsmxServiceUrl);

        try
        {
            var response = await _httpClient.PostAsync(_settings.AsmxServiceUrl, content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("SMS sent successfully to {MobileNumber}. Status Code: {StatusCode}", mobileNumber, response.StatusCode);
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to send SMS to {MobileNumber}. Status Code: {StatusCode}, Response: {Response}",
                    mobileNumber, response.StatusCode, responseContent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred while calling the ASMX SMS service for {MobileNumber}.", mobileNumber);
        }
    }
}