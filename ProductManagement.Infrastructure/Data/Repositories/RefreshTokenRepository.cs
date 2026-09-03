using Microsoft.EntityFrameworkCore;
using ProductManagement.Application.Interfaces.Repositories;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Infrastructure.Data.Repositories;

public class RefreshTokenRepository
    : GenericRepository<RefreshToken>,
      IRefreshTokenRepository
{
    public RefreshTokenRepository(
        ApplicationDbContext context)
        : base(context)
    {
    }

    public async Task<RefreshToken?> GetByTokenAsync(
        string token)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(
                rt => rt.Token == token);
    }
}