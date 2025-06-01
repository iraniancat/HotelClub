// src/HotelReservation.Application/Features/Authentication/Queries/LoginUser/LoginUserQueryHandler.cs
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Application.Contracts.Infrastructure; // برای IPasswordHasherService و IJwtTokenGenerator
using HotelReservation.Application.DTOs.Authentication;    // برای LoginResponseDto
using HotelReservation.Application.Exceptions;           // برای BadRequestException
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HotelReservation.Application.Features.Authentication.Queries.LoginUser;

public class LoginUserQueryHandler : IRequestHandler<LoginUserQuery, LoginResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginUserQueryHandler(
        IUnitOfWork unitOfWork,
        IPasswordHasherService passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _jwtTokenGenerator = jwtTokenGenerator ?? throw new ArgumentNullException(nameof(jwtTokenGenerator));
    }

    public async Task<LoginResponseDto> Handle(LoginUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.UserRepository.GetBySystemUserIdWithDetailsAsync(request.SystemUserId);

        if (user == null || !user.IsActive)
        {
            // _logger.LogWarning(...); // لاگ کردن تلاش برای ورود ناموفق
            throw new BadRequestException("نام کاربری یا رمز عبور نامعتبر است.");
        }

        if (user.Role == null || string.IsNullOrEmpty(user.Role.Name)) // <<-- بررسی مهم
        {
            // _logger.LogError(...);
            throw new InvalidOperationException($"نقش برای کاربر '{user.SystemUserId}' به درستی بارگذاری یا تنظیم نشده است.");
        }
        // اطمینان از اینکه Province و Department هم در صورت نیاز برای Claimها، در GetBySystemUserIdWithDetailsAsync لود شده‌اند.

        if (!_passwordHasher.VerifyPassword(user.PasswordHash, request.Password))
        {
            // _logger.LogWarning(...);
            throw new BadRequestException("نام کاربری یا رمز عبور نامعتبر است.");
        }

        var loginResponse = _jwtTokenGenerator.GenerateToken(user);
        return loginResponse;
    }
}