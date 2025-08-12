// src/HotelReservation.Client/Services/ApiClientService.cs
using System;
using System.Net.Http;
using System.Net.Http.Json; // برای GetFromJsonAsync, PostAsJsonAsync و ...
using System.Net.Http.Headers; // برای AuthenticationHeaderValue
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json; // برای JsonSerializerOptions
// using Microsoft.Extensions.Logging; // برای لاگ کردن (اختیاری)

namespace HotelReservation.Client.Services;

public class ApiClientService : IApiClientService
{
    private readonly HttpClient _httpClient;
    // private readonly ILogger<ApiClientService> _logger;

    private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true // برای هماهنگی با پاسخ‌های API که ممکن است با حروف کوچک شروع شوند
    };

    public ApiClientService(HttpClient httpClient /*, ILogger<ApiClientService> logger*/)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        // _logger = logger;
    }

    public async Task<TResponse?> GetAsync<TResponse>(string requestUri)
    {
        try
        {
            // _logger?.LogInformation("GET request to {RequestUri}", requestUri);
            return await _httpClient.GetFromJsonAsync<TResponse>(requestUri, _jsonSerializerOptions);
        }
        catch (HttpRequestException ex)
        {
            // _logger?.LogError(ex, "HTTP GET request failed to {RequestUri}", requestUri);
            // در اینجا می‌توانید خطای HttpRequestException را مدیریت کنید (مثلاً تبدیل به یک خطای قابل فهم‌تر برای UI)
            // فعلاً null برمی‌گردانیم یا خطا را دوباره پرتاب می‌کنیم
            Console.WriteLine($"API GET Error: {ex.Message} on {requestUri}");
            return default; // یا throw;
        }
    }

    public async Task<byte[]?> GetFileAsByteArrayAsync(string requestUri)
    {
        try
        {
            var response = await _httpClient.GetAsync(requestUri);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API GET File Error: {response.StatusCode} - {errorContent} on {requestUri}");
                throw new ApplicationException($"خطا از API: {response.StatusCode}");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"API GET File Error: {ex.Message} on {requestUri}");
            throw;
        }
    }
    public async Task<IEnumerable<TResponse>?> GetListAsync<TResponse>(string requestUri)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<TResponse>>(requestUri, _jsonSerializerOptions);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"API GET List Error: {ex.Message} on {requestUri}");
            return default;
        }
    }


    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string requestUri, TRequest data)
    {
       try
        {
            var response = await _httpClient.PostAsJsonAsync(requestUri, data, _jsonSerializerOptions);
            if (response.IsSuccessStatusCode)
            {
                if (response.Content.Headers.ContentLength > 0)
                    return await response.Content.ReadFromJsonAsync<TResponse>(_jsonSerializerOptions);
                return default;
            }
            
            await HandleErrorResponse(response);
            return default;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task PostAsync<TRequest>(string requestUri, TRequest data)
    {
         var response = await _httpClient.PutAsJsonAsync(requestUri, data, _jsonSerializerOptions);
        if (!response.IsSuccessStatusCode)
        {
            await HandleErrorResponse(response);
        }
    }


    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string requestUri, TRequest data)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync(requestUri, data, _jsonSerializerOptions);
            if (response.IsSuccessStatusCode)
            {
                if (response.Content.Headers.ContentLength > 0)
                {
                    return await response.Content.ReadFromJsonAsync<TResponse>(_jsonSerializerOptions);
                }
                return default;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API PUT Error: {response.StatusCode} - {errorContent} on {requestUri}");
                throw new ApplicationException($"Error from API: {response.StatusCode} - {errorContent}");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"API PUT Error: {ex.Message} on {requestUri}");
            throw;
        }
    }

    public async Task PutAsync<TRequest>(string requestUri, TRequest data)
    {
        var response = await _httpClient.PutAsJsonAsync(requestUri, data, _jsonSerializerOptions);
        if (!response.IsSuccessStatusCode)
        {
            await HandleErrorResponse(response);
        }
    }

    public async Task DeleteAsync(string requestUri)
    {
        try
        {
            var response = await _httpClient.DeleteAsync(requestUri);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API DELETE Error: {response.StatusCode} - {errorContent} on {requestUri}");
                throw new ApplicationException($"Error from API: {response.StatusCode} - {errorContent}");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"API DELETE Error: {ex.Message} on {requestUri}");
            throw;
        }
    }

    public async Task<TResponse?> PostAsMultipartAsync<TResponse>(string requestUri, MultipartFormDataContent content)
    {
        try
        {
            var response = await _httpClient.PostAsync(requestUri, content);
            if (response.IsSuccessStatusCode)
            {
                if (response.Content.Headers.ContentLength > 0)
                {
                    return await response.Content.ReadFromJsonAsync<TResponse>(_jsonSerializerOptions);
                }
                return default;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Multipart POST Error: {response.StatusCode} - {errorContent} on {requestUri}");
                throw new ApplicationException($"خطا از API: {response.StatusCode} - {errorContent}");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"API Multipart POST Error: {ex.Message} on {requestUri}");
            throw;
        }
    }

    public void SetAuthorizationHeader(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public void ClearAuthorizationHeader()
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    private async Task HandleErrorResponse(HttpResponseMessage response)
    {
        var errorContent = await response.Content.ReadAsStringAsync();
        try
        {
             // ابتدا خطاهای اعتبارسنجی را بررسی می‌کنیم
            // تلاش برای خواندن بدنه خطا به عنوان یک شیء با جزئیات
            var errorDto = JsonSerializer.Deserialize<ErrorResponseDto>(errorContent, _jsonSerializerOptions);
              // ابتدا خطاهای اعتبارسنجی پیش‌فرض ASP.NET Core را بررسی می‌کنیم
            if (errorDto?.Errors != null && errorDto.Errors.Count > 0)
            {
                var combinedMessage = string.Join(" ", errorDto.Errors.Values.SelectMany(v => v));
                var finalMessage = $"{errorDto.Title}: {combinedMessage}";
                throw new ApplicationException(finalMessage);
            }
            
            // سپس خطاهای اعتبارسنجی سفارشی FluentValidation را بررسی می‌کنیم
            if (errorDto?.CustomErrors != null && errorDto.CustomErrors.Any())
            {
                var combinedMessage = string.Join(" ", errorDto.CustomErrors.Select(e => e.ErrorMessage));
                var finalMessage = $"{errorDto.Title}: {combinedMessage}";
                throw new ApplicationException(finalMessage);
            }



            // اگر پیام detail وجود دارد، آن را به عنوان خطا پرتاب کن
            if (!string.IsNullOrWhiteSpace(errorDto?.Detail))
            {
                throw new ApplicationException(errorDto.Detail);
            }
            // در غیر این صورت، از title استفاده کن
            if (!string.IsNullOrWhiteSpace(errorDto?.Title))
            {
                throw new ApplicationException(errorDto.Title);
            }
        }
       catch (JsonException)
        {
            // اگر پاسخ خطا JSON معتبر نبود، خود پاسخ را نمایش بده
            throw new ApplicationException($"خطا از API: {response.StatusCode}. پاسخ سرور قابل خواندن نبود.");
        }
        throw new ApplicationException($"خطایی از سمت سرور با کد {response.StatusCode} رخ داد.");
      
    }
}
// کلاس کمکی برای خواندن پاسخ‌های خطای استاندارد از API
public class ErrorResponseDto
{
    public string? Title { get; set; }
    public int Status { get; set; }
    public string? Detail { get; set; }

    // این ساختار برای هماهنگی با پاسخ خطای پیش‌فرض ASP.NET Core است
    public Dictionary<string, string[]>? Errors { get; set; }

    // این ساختار برای هماهنگی با پاسخ خطای سفارشی FluentValidation ماست
    public List<CustomValidationError>? CustomErrors { get; set; }
}

 // کلاس کمکی برای خواندن جزئیات یک خطای اعتبارسنجی (برای پاسخ‌های سفارشی FluentValidation)
public class CustomValidationError
{
    public string PropertyName { get; set; }
    public string ErrorMessage { get; set; }
}