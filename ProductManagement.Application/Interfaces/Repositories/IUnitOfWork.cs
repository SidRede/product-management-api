namespace ProductManagement.Application.Interfaces.Repositories;

public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }

    IItemRepository Items { get; }

    IUserRepository Users { get; }

    IRefreshTokenRepository RefreshTokens { get; }

    Task<int> SaveChangesAsync();
}