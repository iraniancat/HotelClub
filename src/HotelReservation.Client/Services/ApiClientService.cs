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
                if (response.Content.Headers.ContentLength > 0) // بررسی اینکه آیا محتوایی برای خواندن وجود دارد
                {
                    return await response.Content.ReadFromJsonAsync<TResponse>(_jsonSerializerOptions);
                }
                return default; // برای پاسخ‌های 201 Created بدون بدنه یا پاسخ‌هایی که TResponse یک نوع ساده است
            }
            else
            {
                // مدیریت خطاهای HTTP (4xx, 5xx)
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API POST Error: {response.StatusCode} - {errorContent} on {requestUri}");
                // می‌توان یک Exception سفارشی پرتاب کرد یا یک نتیجه خطا برگرداند
                // throw new ApplicationException($"Error from API: {response.StatusCode} - {errorContent}");
                return default;
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"API POST Error: {ex.Message} on {requestUri}");
            return default;
        }
    }
    public async Task PostAsync<TRequest>(string requestUri, TRequest data)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(requestUri, data, _jsonSerializerOptions);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API POST (no response) Error: {response.StatusCode} - {errorContent} on {requestUri}");
                throw new ApplicationException($"Error from API: {response.StatusCode} - {errorContent}");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"API POST (no response) Error: {ex.Message} on {requestUri}");
            throw;
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
        try
        {
            var response = await _httpClient.PutAsJsonAsync(requestUri, data, _jsonSerializerOptions);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API PUT (no response) Error: {response.StatusCode} - {errorContent} on {requestUri}");
                throw new ApplicationException($"Error from API: {response.StatusCode} - {errorContent}");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"API PUT (no response) Error: {ex.Message} on {requestUri}");
            throw;
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
}