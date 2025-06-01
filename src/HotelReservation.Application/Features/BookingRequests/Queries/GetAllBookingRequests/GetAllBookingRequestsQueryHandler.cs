// src/HotelReservation.Application/Features/BookingRequests/Queries/GetAllBookingRequests/GetAllBookingRequestsQueryHandler.cs
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Application.DTOs.Booking;
using HotelReservation.Application.DTOs.Common; // <<-- برای PagedResult
using HotelReservation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore; // برای ToListAsync, CountAsync, Skip, Take
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using HotelReservation.Application.Contracts.Security; // برای ICurrentUserService

namespace HotelReservation.Application.Features.BookingRequests.Queries.GetAllBookingRequests;

public class GetAllBookingRequestsQueryHandler : IRequestHandler<GetAllBookingRequestsQuery, PagedResult<BookingRequestSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService; // <<-- اضافه شد
    private readonly ILogger<GetAllBookingRequestsQueryHandler> _logger;
    // ... (نام نقش‌ها) ...
    private const string SuperAdminRoleName = "SuperAdmin";
    private const string ProvinceUserRoleName = "ProvinceUser";
    private const string HotelUserRoleName = "HotelUser";

    public GetAllBookingRequestsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService, // <<-- اضافه شد
        ILogger<GetAllBookingRequestsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService)); // <<-- اضافه شد
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PagedResult<BookingRequestSummaryDto>> Handle(GetAllBookingRequestsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.UserId == null || string.IsNullOrEmpty(_currentUserService.UserRole))
        {
            _logger.LogWarning("GetAllBookingRequests: Unauthenticated user or missing essential claims.");
            throw new UnauthorizedAccessException("کاربر برای انجام این عملیات احراز هویت نشده یا اطلاعات لازم را ندارد.");
        }

        _logger.LogInformation(
             "Fetching booking requests for User: {UserId}, Role: {UserRole}, Page: {PageNumber}, Size: {PageSize}, Search: '{SearchTerm}', Status: '{StatusFilter}'",
             _currentUserService.UserId, _currentUserService.UserRole, request.PageNumber, request.PageSize, request.SearchTerm, request.StatusFilter);


        IQueryable<BookingRequest> query = _unitOfWork.BookingRequestRepository.GetQueryable()
                               .Include(br => br.Hotel)
                               .Include(br => br.RequestSubmitterUser).ThenInclude(u => u.Role);

        Expression<Func<BookingRequest, bool>>? rolePredicate = null;
        List<User> employeesInProvince = new List<User>();

        if (_currentUserService.UserRole == ProvinceUserRoleName)
        {
            if (string.IsNullOrEmpty(_currentUserService.ProvinceCode))
            {
                _logger.LogWarning("ProvinceUser {UserId} lacks ProvinceCode claim.", _currentUserService.UserId);
                return new PagedResult<BookingRequestSummaryDto>(new List<BookingRequestSummaryDto>(), 0, request.PageNumber, request.PageSize);
            }
            employeesInProvince = (await _unitOfWork.UserRepository.GetAsync(u => u.ProvinceCode == _currentUserService.ProvinceCode && u.NationalCode != null)).ToList();
            var nationalCodesInProvince = employeesInProvince.Select(e => e.NationalCode!).ToList();
            rolePredicate = br => (br.RequestingEmployeeNationalCode != null && nationalCodesInProvince.Contains(br.RequestingEmployeeNationalCode)) ||
                                  br.RequestSubmitterUserId == _currentUserService.UserId.Value; // UserId اینجا null نیست
        }
        else if (_currentUserService.UserRole == HotelUserRoleName)
        {
            if (!_currentUserService.HotelId.HasValue)
            {
                _logger.LogWarning("HotelUser {UserId} lacks HotelId claim.", _currentUserService.UserId);
                return new PagedResult<BookingRequestSummaryDto>(new List<BookingRequestSummaryDto>(), 0, request.PageNumber, request.PageSize);
            }
            rolePredicate = br => br.HotelId == _currentUserService.HotelId.Value;
        }
        // برای SuperAdmin، rolePredicate null می‌ماند

        if (rolePredicate != null)
        {
            query = query.Where(rolePredicate);
        }
        // ... (بقیه منطق فیلتر، جستجو، صفحه‌بندی و نگاشت همانند قبل) ...
        if (!string.IsNullOrEmpty(request.StatusFilter))
        {
            if (Enum.TryParse<Domain.Enums.BookingStatus>(request.StatusFilter, true, out var statusEnum))
            {
                query = query.Where(br => br.Status == statusEnum);
            }
            else
            {
                _logger.LogWarning("Invalid StatusFilter value: {StatusFilter}", request.StatusFilter);
            }
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTermLower = request.SearchTerm.ToLower().Trim();
            query = query.Where(br =>
                (br.TrackingCode != null && br.TrackingCode.ToLower().Contains(searchTermLower)) ||
                (br.RequestingEmployeeNationalCode != null && br.RequestingEmployeeNationalCode.Contains(searchTermLower)) ||
                (br.Hotel.Name != null && br.Hotel.Name.ToLower().Contains(searchTermLower)) ||
                (br.RequestSubmitterUser.FullName != null && br.RequestSubmitterUser.FullName.ToLower().Contains(searchTermLower))
            );
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var bookingRequests = await query
           .OrderByDescending(br => br.SubmissionDate)
           .Skip((request.PageNumber - 1) * request.PageSize)
           .Take(request.PageSize)
           .AsNoTracking()
           .ToListAsync(cancellationToken);

        var distinctEmployeeNationalCodes = bookingRequests
           .Select(br => br.RequestingEmployeeNationalCode)
           .Where(nc => nc != null).Distinct().ToList();

        var requestingEmployeesMap = employeesInProvince
            .Where(e => e.NationalCode != null && distinctEmployeeNationalCodes.Contains(e.NationalCode))
            .ToDictionary(e => e.NationalCode!);

        var nationalCodesToFetch = distinctEmployeeNationalCodes
            .Except(requestingEmployeesMap.Keys).ToList();

        if (nationalCodesToFetch.Any())
        {
            var additionalEmployees = await _unitOfWork.UserRepository.GetAsync(u => nationalCodesToFetch.Contains(u.NationalCode) && u.NationalCode != null);
            foreach (var emp in additionalEmployees)
            {
                if (emp.NationalCode != null && !requestingEmployeesMap.ContainsKey(emp.NationalCode))
                {
                    requestingEmployeesMap.Add(emp.NationalCode, emp);
                }
            }
        }

        var bookingRequestDtos = bookingRequests.Select(br =>
        {
            User? requestingEmployee = null;
            if (br.RequestingEmployeeNationalCode != null)
            {
                requestingEmployeesMap.TryGetValue(br.RequestingEmployeeNationalCode, out requestingEmployee);
            }
            return new BookingRequestSummaryDto
            {
                Id = br.Id,
                TrackingCode = br.TrackingCode,
                RequestingEmployeeNationalCode = br.RequestingEmployeeNationalCode ?? "نامشخص",
                RequestingEmployeeFullName = requestingEmployee?.FullName,
                HotelName = br.Hotel?.Name ?? "نامشخص",
                CheckInDate = br.CheckInDate,
                CheckOutDate = br.CheckOutDate,
                Status = br.Status.ToString(),
                SubmissionDate = br.SubmissionDate,
                SubmitterUserFullName = br.RequestSubmitterUser?.FullName ?? "نامشخص",
                TotalGuests = br.TotalGuests
            };
        }).ToList();

        return new PagedResult<BookingRequestSummaryDto>(bookingRequestDtos, totalCount, request.PageNumber, request.PageSize);
    }
    // متد AndAlso همانند قبل
    private static Expression<Func<T, bool>> AndAlso<T>(Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
    {
        var parameter = expr1.Parameters[0];
        var visitedExpr2 = new ParameterReplacer(expr2.Parameters[0], parameter).Visit(expr2.Body);
        var body = Expression.AndAlso(expr1.Body, visitedExpr2);
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}
internal class ParameterReplacer : ExpressionVisitor
 {
     private readonly ParameterExpression _source;
     private readonly ParameterExpression _target;

     public ParameterReplacer(ParameterExpression source, ParameterExpression target)
     {
         _source = source;
         _target = target;
     }

     protected override Expression VisitParameter(ParameterExpression node)
     {
         return node == _source ? _target : base.VisitParameter(node);
     }
 }