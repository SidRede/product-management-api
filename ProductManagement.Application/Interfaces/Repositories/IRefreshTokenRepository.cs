using ProductManagement.Domain.Entities;

namespace ProductManagement.Application.Interfaces.Repositories;

public interface IRefreshTokenRepository
    : IGenericRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token);
}