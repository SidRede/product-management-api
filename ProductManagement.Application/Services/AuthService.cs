using ProductManagement.Application.DTOs.Authentication;
using ProductManagement.Application.Interfaces.Repositories;
using ProductManagement.Application.Interfaces.Services;
using ProductManagement.Domain.Exceptions;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponseDto?> RegisterAsync(
        RegisterDto registerDto)
    {
        var existingUser =
            await _unitOfWork.Users.GetByEmailAsync(
                registerDto.Email);

        if (existingUser != null)
        {
            throw new BadRequestException(
              "A user with this email already exists.");
        }

        var user = new User
        {
            UserName = registerDto.UserName,
            Email = registerDto.Email,

            PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(
                    registerDto.Password),

            Role = "User",
            CreatedOn = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user);

        await _unitOfWork.SaveChangesAsync();

        return await GenerateTokensAsync(user);
    }

    public async Task<AuthResponseDto?> LoginAsync(
        LoginDto loginDto)
    {
        var user =
            await _unitOfWork.Users.GetByEmailAsync(
                loginDto.Email);

        if (user == null)
        {
            throw new UnauthorizedException(
                "Invalid email or password.");
        }

        var passwordValid =
            BCrypt.Net.BCrypt.Verify(
                loginDto.Password,
                user.PasswordHash);

        if (!passwordValid)
        {
            throw new UnauthorizedException(
                 "Invalid email or password.");
        }

        return await GenerateTokensAsync(user);
    }

    public async Task<AuthResponseDto?> RefreshTokenAsync(
        RefreshTokenRequestDto refreshTokenRequestDto)
    {
        var refreshToken =
            await _unitOfWork.RefreshTokens
                .GetByTokenAsync(
                    refreshTokenRequestDto.RefreshToken);

        if (refreshToken == null ||
            refreshToken.IsRevoked ||
            refreshToken.ExpiresOn <= DateTime.UtcNow)
        {
            return null;
        }

        // Revoke old refresh token (rotation)
        refreshToken.IsRevoked = true;

        _unitOfWork.RefreshTokens.Update(
            refreshToken);

        var response =
            await GenerateTokensAsync(
                refreshToken.User);

        return response;
    }

    private async Task<AuthResponseDto> GenerateTokensAsync(
        User user)
    {
        var accessToken =
            _jwtTokenService.GenerateAccessToken(user);

        var refreshTokenValue =
            _jwtTokenService.GenerateRefreshToken();

        var refreshTokenExpiration =
            DateTime.UtcNow.AddDays(7);

        var refreshToken = new RefreshToken
        {
            Token = refreshTokenValue,
            UserId = user.Id,
            CreatedOn = DateTime.UtcNow,
            ExpiresOn = refreshTokenExpiration,
            IsRevoked = false
        };

        await _unitOfWork.RefreshTokens
            .AddAsync(refreshToken);

        await _unitOfWork.SaveChangesAsync();

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            AccessTokenExpiresOn =
                DateTime.UtcNow.AddMinutes(15)
        };
    }
}