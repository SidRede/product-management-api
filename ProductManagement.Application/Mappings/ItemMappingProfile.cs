using AutoMapper;
using ProductManagement.Application.DTOs.Items;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Application.Mapping;

public class ItemMappingProfile : Profile
{
    public ItemMappingProfile()
    {
        CreateMap<Item, ItemDto>();
    }
}