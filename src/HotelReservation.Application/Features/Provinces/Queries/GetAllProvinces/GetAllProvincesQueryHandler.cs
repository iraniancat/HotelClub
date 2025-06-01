// src/HotelReservation.Application/Features/Provinces/Queries/GetAllProvinces/GetAllProvincesQueryHandler.cs
using HotelReservation.Application.Contracts.Persistence; // برای IUnitOfWork
using HotelReservation.Application.DTOs.Location; // برای ProvinceDto
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HotelReservation.Application.Features.Provinces.Queries.GetAllProvinces;

public class GetAllProvincesQueryHandler : IRequestHandler<GetAllProvincesQuery, IEnumerable<ProvinceDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    // private readonly IMapper _mapper; // اگر از AutoMapper استفاده می‌کردیم

    public GetAllProvincesQueryHandler(IUnitOfWork unitOfWork /*, IMapper mapper*/)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        // _mapper = mapper;
    }

    public async Task<IEnumerable<ProvinceDto>> Handle(GetAllProvincesQuery request, CancellationToken cancellationToken)
    {
        var provinces = await _unitOfWork.ProvinceRepository.GetAllAsync(); // متد از IGenericRepository

        if (provinces == null || !provinces.Any())
        {
            return new List<ProvinceDto>();
        }

        // نگاشت دستی
        return provinces.Select(province => new ProvinceDto
        {
            Code = province.Code,
            Name = province.Name
        }).OrderBy(p => p.Name).ToList(); // مرتب‌سازی بر اساس نام برای نمایش بهتر
    }
}