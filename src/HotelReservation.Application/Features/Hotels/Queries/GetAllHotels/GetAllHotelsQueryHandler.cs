// src/HotelReservation.Application/Features/Hotels/Queries/GetAllHotels/GetAllHotelsQueryHandler.cs
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Application.DTOs.Common;     // برای PagedResult
using HotelReservation.Application.DTOs.Hotel;      // برای HotelDto
using HotelReservation.Domain.Entities;             // برای Hotel
using MediatR;
using Microsoft.EntityFrameworkCore;             // برای ToListAsync, CountAsync, Skip, Take
using Microsoft.Extensions.Logging;               // برای ILogger
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HotelReservation.Application.Features.Hotels.Queries.GetAllHotels;

public class GetAllHotelsQueryHandler : IRequestHandler<GetAllHotelsQuery, PagedResult<HotelDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAllHotelsQueryHandler> _logger;
    // private readonly IMapper _mapper; // اگر از AutoMapper استفاده می‌کردیم

    public GetAllHotelsQueryHandler(IUnitOfWork unitOfWork, ILogger<GetAllHotelsQueryHandler> logger /*, IMapper mapper*/)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        // _mapper = mapper;
    }

    public async Task<PagedResult<HotelDto>> Handle(GetAllHotelsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching hotels. Page: {PageNumber}, Size: {PageSize}, Search: '{SearchTerm}'",
            request.PageNumber, request.PageSize, request.SearchTerm);

        var query = _unitOfWork.HotelRepository.GetQueryable(); // استفاده از GetQueryable

        // ۱. اعمال فیلتر جستجو (SearchTerm)
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTermLower = request.SearchTerm.ToLower().Trim();
            query = query.Where(h =>
                (h.Name.ToLower().Contains(searchTermLower)) ||
                (h.Address.ToLower().Contains(searchTermLower)) ||
                (h.ContactPerson != null && h.ContactPerson.ToLower().Contains(searchTermLower)) ||
                (h.PhoneNumber != null && h.PhoneNumber.Contains(searchTermLower)) // شماره تلفن ممکن است case-sensitive نباشد
            );
        }

        // ۲. گرفتن تعداد کل رکوردها (پس از فیلترها، قبل از صفحه‌بندی)
        var totalCount = await query.CountAsync(cancellationToken);

        // ۳. اعمال ترتیب و صفحه‌بندی
        var hotels = await query
            .OrderBy(h => h.Name) // مرتب‌سازی بر اساس نام هتل
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .AsNoTracking() // مهم برای Queryهای فقط خواندنی
            .ToListAsync(cancellationToken);

        // ۴. نگاشت به DTO
        var hotelDtos = hotels.Select(hotel => new HotelDto
        {
            Id = hotel.Id,
            Name = hotel.Name,
            Address = hotel.Address,
            ContactPerson = hotel.ContactPerson,
            PhoneNumber = hotel.PhoneNumber
        }).ToList();

        return new PagedResult<HotelDto>(hotelDtos, totalCount, request.PageNumber, request.PageSize);
    }
}