using MediatR;

namespace HotelReservation.Application.Features.Quotas.Commands.SetQuota;

public class SetQuotaCommand : IRequest<Guid>
{
    public Guid HotelId { get; set; }
    public string ProvinceCode { get; set; }
    public int RoomLimit { get; set; }
}
