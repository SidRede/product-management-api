using AutoMapper;
using ProductManagement.Application.DTOs.Products;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Application.Mapping;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<Product, ProductDto>();

        CreateMap<CreateProductDto, Product>();

        CreateMap<UpdateProductDto, Product>();
    }
}