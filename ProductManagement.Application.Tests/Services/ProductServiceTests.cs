using AutoMapper;
using Moq;
using ProductManagement.Application.DTOs.Products;
using ProductManagement.Application.Interfaces;
using ProductManagement.Application.Interfaces.Repositories;
using ProductManagement.Application.Services;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Exceptions;
using Xunit;

namespace ProductManagement.Application.Tests.Services;

public class ProductServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;

    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _mapperMock = new Mock<IMapper>();

        _unitOfWorkMock
            .Setup(x => x.Products)
            .Returns(_productRepositoryMock.Object);

        _productService = new ProductService(
            _unitOfWorkMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductExists_ReturnsProductDto()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            ProductName = "Laptop",
            CreatedBy = "System",
            CreatedOn = DateTime.UtcNow
        };

        var productDto = new ProductDto
        {
            Id = 1,
            ProductName = "Laptop"
        };

        _productRepositoryMock
            .Setup(x => x.GetProductWithItemsAsync(1))
            .ReturnsAsync(product);

        _mapperMock
            .Setup(x => x.Map<ProductDto>(product))
            .Returns(productDto);

        // Act
        var result = await _productService.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
        Assert.Equal("Laptop", result.ProductName);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _productRepositoryMock
            .Setup(x => x.GetProductWithItemsAsync(999))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _productService.GetByIdAsync(999));

        Assert.Contains("999", exception.Message);
    }

    [Fact]
    public async Task GetAllAsync_WhenProductsExist_ReturnsProductDtos()
    {
        // Arrange
        var products = new List<Product>
    {
        new Product
        {
            Id = 1,
            ProductName = "Laptop",
            CreatedBy = "System",
            CreatedOn = DateTime.UtcNow
        },
        new Product
        {
            Id = 2,
            ProductName = "Mobile",
            CreatedBy = "System",
            CreatedOn = DateTime.UtcNow
        }
    };

        var productDtos = new List<ProductDto>
    {
        new ProductDto
        {
            Id = 1,
            ProductName = "Laptop"
        },
        new ProductDto
        {
            Id = 2,
            ProductName = "Mobile"
        }
    };

        _productRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(products);

        _mapperMock
            .Setup(x => x.Map<IEnumerable<ProductDto>>(products))
            .Returns(productDtos);

        // Act
        var result = await _productService.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task CreateAsync_WhenValidProductProvided_ReturnsProductDto()
    {
        // Arrange
        var createProductDto = new CreateProductDto
        {
            ProductName = "Laptop"
        };

        var product = new Product
        {
            ProductName = "Laptop"
        };

        var productDto = new ProductDto
        {
            Id = 1,
            ProductName = "Laptop"
        };

        _mapperMock
            .Setup(x => x.Map<Product>(createProductDto))
            .Returns(product);

        _productRepositoryMock
            .Setup(x => x.AddAsync(product))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        _mapperMock
            .Setup(x => x.Map<ProductDto>(product))
            .Returns(productDto);

        // Act
        var result = await _productService.CreateAsync(createProductDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Laptop", result.ProductName);

        _productRepositoryMock.Verify(
            x => x.AddAsync(product),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenProductExists_UpdatesAndReturnsProductDto()
    {
        // Arrange
        var id = 1;

        var updateProductDto = new UpdateProductDto
        {
            ProductName = "Updated Laptop"
        };

        var product = new Product
        {
            Id = id,
            ProductName = "Laptop",
            CreatedBy = "System",
            CreatedOn = DateTime.UtcNow
        };

        var productDto = new ProductDto
        {
            Id = id,
            ProductName = "Updated Laptop"
        };

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(product);

        _mapperMock
            .Setup(x => x.Map(updateProductDto, product));

        _productRepositoryMock
            .Setup(x => x.Update(product));

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        _mapperMock
            .Setup(x => x.Map<ProductDto>(product))
            .Returns(productDto);

        // Act
        var result = await _productService.UpdateAsync(
            id,
            updateProductDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);
        Assert.Equal("Updated Laptop", result.ProductName);

        _productRepositoryMock.Verify(
            x => x.Update(product),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenProductDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var id = 999;

        var updateProductDto = new UpdateProductDto
        {
            ProductName = "Updated Product"
        };

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _productService.UpdateAsync(
                id,
                updateProductDto));

        Assert.Contains("999", exception.Message);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenProductExists_DeletesProduct()
    {
        // Arrange
        var id = 1;

        var product = new Product
        {
            Id = id,
            ProductName = "Laptop",
            CreatedBy = "System",
            CreatedOn = DateTime.UtcNow
        };

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(product);

        _productRepositoryMock
            .Setup(x => x.Delete(product));

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        await _productService.DeleteAsync(id);

        // Assert
        _productRepositoryMock.Verify(
            x => x.Delete(product),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenProductDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var id = 999;

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _productService.DeleteAsync(id));

        Assert.Contains("999", exception.Message);

        _productRepositoryMock.Verify(
            x => x.Delete(It.IsAny<Product>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Never);
    }
}