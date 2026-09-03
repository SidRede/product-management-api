using ProductManagement.Application.DTOs.Authentication;

namespace ProductManagement.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);

    Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);

    Task<AuthResponseDto?> RefreshTokenAsync(
        RefreshTokenRequestDto refreshTokenRequestDto);
}