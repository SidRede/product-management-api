using ProductManagement.Domain.Entities;

namespace ProductManagement.Application.Interfaces.Repositories;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<Product?> GetProductWithItemsAsync(int id);
}