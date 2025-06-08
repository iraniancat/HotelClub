namespace HotelReservation.Application.Features.UserManagement.Queries.SearchUsers;

using HotelReservation.Application.DTOs.UserManagement;
using MediatR;
using System.Collections.Generic;

public class SearchUsersQuery : IRequest<IEnumerable<UserWithDependentsDto>>
{
    public string SearchTerm { get; }

    public SearchUsersQuery(string searchTerm)
    {
        SearchTerm = searchTerm;
    }
}