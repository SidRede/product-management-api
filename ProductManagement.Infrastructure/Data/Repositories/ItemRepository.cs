using ProductManagement.Application.Interfaces.Repositories;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Infrastructure.Data.Repositories;

public class ItemRepository
    : GenericRepository<Item>, IItemRepository
{
    public ItemRepository(ApplicationDbContext context)
        : base(context)
    {
    }
}