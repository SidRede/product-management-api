using AutoMapper;
using ProductManagement.Application.DTOs.Products;
using ProductManagement.Application.Interfaces.Repositories;
using ProductManagement.Application.Interfaces.Services;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Exceptions;

namespace ProductManagement.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProductService(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ProductDto> CreateAsync(
        CreateProductDto createProductDto)
    {
        var product = _mapper.Map<Product>(createProductDto);

        // Temporary value until JWT authentication is implemented.
        product.CreatedBy = "System";
        product.CreatedOn = DateTime.UtcNow;

        await _unitOfWork.Products.AddAsync(product);

        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product =
            await _unitOfWork.Products.GetProductWithItemsAsync(id);

        if (product == null)
        {
            throw new NotFoundException(
                $"Product with Id {id} was not found.");
        }

        return _mapper.Map<ProductDto>(product);
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        var products =
            await _unitOfWork.Products.GetAllAsync();

        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<ProductDto?> UpdateAsync(
        int id,
        UpdateProductDto updateProductDto)
    {
        var product =
            await _unitOfWork.Products.GetByIdAsync(id);

        if (product == null)
        {
            throw new NotFoundException(
                $"Product with Id {id} was not found.");
        }

        _mapper.Map(updateProductDto, product);

        // Temporary value until JWT authentication is implemented.
        product.ModifiedBy = "System";
        product.ModifiedOn = DateTime.UtcNow;

        _unitOfWork.Products.Update(product);

        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ProductDto>(product);
    }

    public async Task DeleteAsync(int id)
    {
        var product =
            await _unitOfWork.Products.GetByIdAsync(id);

        if (product == null)
        {
            throw new NotFoundException(
                $"Product with Id {id} was not found.");
        }

        _unitOfWork.Products.Delete(product);

        await _unitOfWork.SaveChangesAsync();

     
    }
}