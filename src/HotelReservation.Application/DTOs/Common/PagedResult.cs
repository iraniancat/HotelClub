// src/HotelReservation.Application/DTOs/Common/PagedResult.cs
using System;
using System.Collections.Generic;

namespace HotelReservation.Application.DTOs.Common;

public class PagedResult<T> where T : class
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
    public List<T> Items { get; set; } = new List<T>();
 
    // <<-- سازنده بدون پارامتر عمومی برای دی‌سریالایزر JSON -->>
    public PagedResult()
    {
        Items = new List<T>(); // مقداردهی اولیه لیست برای جلوگیری از null بودن
    }

    // سازنده قبلی شما برای استفاده در سمت سرور همچنان می‌تواند وجود داشته باشد
    public PagedResult(List<T> items, int totalCount, int pageNumber, int pageSize)
    {
        CurrentPage = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
        Items = items ?? new List<T>();
    }
}