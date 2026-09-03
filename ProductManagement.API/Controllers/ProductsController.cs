using Microsoft.AspNetCore.Mvc;
using ProductManagement.Application.DTOs.Products;
using ProductManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;

namespace ProductManagement.API.Controllers;


/// <summary>
/// Provides endpoints for managing products.
/// </summary>
[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
   
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductService productService,
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }



    /// <summary>
    /// Gets a paginated list of products.
    /// </summary>
    /// <param name="pageNumber">Page number.</param>
    /// <param name="pageSize">Number of products per page.</param>
    /// <returns>A paginated list of products.</returns>

    // GET: api/products
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll()
    {
        var products = await _productService.GetAllAsync();

        return Ok(products);
    }

    /// <summary>
    /// Gets a product by its ID.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <returns>The requested product.</returns>
    // GET: api/products/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);

        _logger.LogInformation(
            "Getting product with ID {ProductId}",
            id);

        return Ok(product);
    }

    // POST: api/products
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductDto>> Create(
        CreateProductDto createProductDto)
    {
        _logger.LogInformation(
                "Creating new product with name {ProductName}",
                createProductDto.ProductName);
        var product =
            await _productService.CreateAsync(createProductDto);

        _logger.LogInformation(
            "Product created successfully with ID {ProductId}",
            product.Id);

        return CreatedAtAction(
            nameof(GetById),
            new { id = product.Id },
            product);
    }

    // PUT: api/products/1
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductDto>> Update(
        int id,
        UpdateProductDto updateProductDto)
    {
        var product =
            await _productService.UpdateAsync(
                id,
                updateProductDto);

        return Ok(product);
    }

    // DELETE: api/products/1
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation(
              "Deleting product with ID {ProductId}",
              id);
        await _productService.DeleteAsync(id);

        return NoContent();
    }
}