namespace HotelReservation.Application.Features.Hotels.Queries.GetAllHotelsSimple;

using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Application.DTOs.Hotel;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class GetAllHotelsSimpleQueryHandler : IRequestHandler<GetAllHotelsSimpleQuery, IEnumerable<HotelDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllHotelsSimpleQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<HotelDto>> Handle(GetAllHotelsSimpleQuery request, CancellationToken cancellationToken)
    {
        var hotels = await _unitOfWork.HotelRepository.GetAllAsync();

        if (hotels == null || !hotels.Any())
        {
            return new List<HotelDto>();
        }

        return hotels.Select(hotel => new HotelDto
        {
            Id = hotel.Id,
            Name = hotel.Name,
            Address = hotel.Address,
            ContactPerson = hotel.ContactPerson,
            PhoneNumber = hotel.PhoneNumber
        }).OrderBy(h => h.Name).ToList();
    }
}