using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit;
using System.Net;
using System.Net.Http.Json;
using ProductManagement.Application.DTOs.Authentication;
using System.Net.Http.Headers;
using ProductManagement.Application.DTOs.Products;

namespace ProductManagement.API.Tests.Controllers;

public class ProductIntegrationTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ProductIntegrationTests(
        WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetAllProducts_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Act
        var response =
            await _client.GetAsync("/api/v1/Products");

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "doesnotexist@test.com",
            Password = "Password@123"
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/v1/Auth/login",
            loginDto);

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ThenGetProducts_ReturnsOk()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "integrationtest@example.com",
            Password = "TestPassword@123"
        };

        // Act - Login
        var loginResponse = await _client.PostAsJsonAsync(
            "/api/v1/Auth/login",
            loginDto);

        // Assert - Login successful
        Assert.Equal(
            HttpStatusCode.OK,
            loginResponse.StatusCode);

        var authResponse =
            await loginResponse.Content
                .ReadFromJsonAsync<AuthResponseDto>();

        Assert.NotNull(authResponse);
        Assert.False(
            string.IsNullOrWhiteSpace(
                authResponse!.AccessToken));

        // Arrange authenticated request
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                authResponse.AccessToken);

        // Act - Access protected endpoint
        var productsResponse =
            await _client.GetAsync(
                "/api/v1/Products");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            productsResponse.StatusCode);
    }


    [Fact]
    public async Task GetProductById_WhenProductDoesNotExist_ReturnsNotFound()
    {
        // Arrange - Login
        var loginDto = new LoginDto
        {
            Email = "integrationtest@example.com",
            Password = "TestPassword@123"
        };

        var loginResponse = await _client.PostAsJsonAsync(
            "/api/v1/Auth/login",
            loginDto);

        Assert.Equal(
            HttpStatusCode.OK,
            loginResponse.StatusCode);

        var authResponse =
            await loginResponse.Content
                .ReadFromJsonAsync<AuthResponseDto>();

        Assert.NotNull(authResponse);

        // Add JWT token
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                authResponse!.AccessToken);

        // Act
        var response = await _client.GetAsync(
            "/api/v1/Products/999999");

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }
    [Fact]
    public async Task CreateProduct_WhenUserIsNotAdmin_ReturnsForbidden()
    {
        // Arrange - Login as normal User
        var loginDto = new LoginDto
        {
            Email = "integrationtest@example.com",
            Password = "TestPassword@123"
        };

        var loginResponse = await _client.PostAsJsonAsync(
            "/api/v1/Auth/login",
            loginDto);

        Assert.Equal(
            HttpStatusCode.OK,
            loginResponse.StatusCode);

        var authResponse =
            await loginResponse.Content
                .ReadFromJsonAsync<AuthResponseDto>();

        Assert.NotNull(authResponse);

        // Add JWT token
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                authResponse!.AccessToken);

        var createProductDto = new CreateProductDto
        {
            ProductName = "Unauthorized Test Product"
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/v1/Products",
            createProductDto);

        // Assert
        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_WhenUserIsAdmin_ReturnsCreated()
    {
        // Arrange - Login as Admin
        var loginDto = new LoginDto
        {
            Email = "testuser@example.com",
            Password = "TestPassword123!"
        };

        var loginResponse = await _client.PostAsJsonAsync(
            "/api/v1/Auth/login",
            loginDto);

        // Verify login
        Assert.Equal(
            HttpStatusCode.OK,
            loginResponse.StatusCode);

        var authResponse =
            await loginResponse.Content
                .ReadFromJsonAsync<AuthResponseDto>();

        Assert.NotNull(authResponse);

        Assert.False(
            string.IsNullOrWhiteSpace(
                authResponse!.AccessToken));

        // Add Admin JWT token
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                authResponse.AccessToken);

        // Create a unique product name
        var createProductDto = new CreateProductDto
        {
            ProductName =
                $"Admin Test Product {Guid.NewGuid()}"
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/v1/Products",
            createProductDto);

        // Assert
        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        var product =
            await response.Content
                .ReadFromJsonAsync<ProductDto>();

        Assert.NotNull(product);

        Assert.True(product!.Id > 0);

        Assert.Equal(
            createProductDto.ProductName,
            product.ProductName);
    }
}