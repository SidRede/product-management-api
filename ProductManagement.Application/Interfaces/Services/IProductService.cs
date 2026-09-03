using ProductManagement.Application.DTOs.Products;

namespace ProductManagement.Application.Interfaces.Services;

public interface IProductService
{
    Task<ProductDto> CreateAsync(
        CreateProductDto createProductDto);

    Task<ProductDto> GetByIdAsync(int id);

    Task<IEnumerable<ProductDto>> GetAllAsync();

    Task<ProductDto?> UpdateAsync(
        int id,
        UpdateProductDto updateProductDto);

    Task DeleteAsync(int id);
}