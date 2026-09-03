using ProductManagement.Domain.Entities;

namespace ProductManagement.Application.Interfaces.Repositories;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
}