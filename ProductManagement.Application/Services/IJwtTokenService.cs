using ProductManagement.Domain.Entities;

namespace ProductManagement.Application.Interfaces.Services;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user);

    string GenerateRefreshToken();
}