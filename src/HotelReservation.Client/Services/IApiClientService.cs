// src/HotelReservation.Client/Services/IApiClientService.cs
using System.Threading.Tasks;
using System.Collections.Generic; // برای IEnumerable

namespace HotelReservation.Client.Services;

public interface IApiClientService
{
    Task<TResponse?> GetAsync<TResponse>(string requestUri);
    Task<IEnumerable<TResponse>?> GetListAsync<TResponse>(string requestUri); // برای لیست‌ها
    Task<TResponse?> PostAsync<TRequest, TResponse>(string requestUri, TRequest data);
    Task PostAsync<TRequest>(string requestUri, TRequest data); // برای POST بدون پاسخ خاص در بدنه
    Task<TResponse?> PutAsync<TRequest, TResponse>(string requestUri, TRequest data);
    Task PutAsync<TRequest>(string requestUri, TRequest data); // برای PUT بدون پاسخ خاص در بدنه
    Task DeleteAsync(string requestUri);

   // متد جدید برای ارسال فرم چندبخشی (شامل فایل)
    Task<TResponse?> PostAsMultipartAsync<TResponse>(string requestUri, MultipartFormDataContent content);
    void SetAuthorizationHeader(string? token);
    void ClearAuthorizationHeader();
}