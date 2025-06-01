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

    public PagedResult(List<T> items, int count, int pageNumber, int pageSize)
    {
        TotalCount = count;
        PageSize = pageSize;
        CurrentPage = pageNumber;
        Items = items;
    }
}