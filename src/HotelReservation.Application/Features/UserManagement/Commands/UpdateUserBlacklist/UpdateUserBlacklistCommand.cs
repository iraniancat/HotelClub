
// 2. فایل جدید: src/HotelReservation.Application/Features/UserManagement/Commands/UpdateUserBlacklist/UpdateUserBlacklistCommand.cs
using MediatR;

namespace HotelReservation.Application.Features.UserManagement.Commands.UpdateUserBlacklist;

public class UpdateUserBlacklistCommand : IRequest
{
    public Guid UserId { get; set; }
    public bool IsBlacklisted { get; set; }
    public string? Reason { get; set; }
    public DateTime? EndDate { get; set; }
}
