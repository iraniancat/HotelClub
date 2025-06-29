using HotelReservation.Application.DTOs.Quota;
using MediatR;

namespace HotelReservation.Application.Features.Quotas.Queries.GetQuotasByHotel;

public class GetQuotasByHotelQuery : IRequest<IEnumerable<ProvinceHotelQuotaDto>>
{
    public Guid HotelId { get; set; }
}
