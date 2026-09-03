using FluentValidation;
using ProductManagement.Application.DTOs.Products;

namespace ProductManagement.Application.Validators;

public class CreateProductDtoValidator
    : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(x => x.ProductName)
            .NotEmpty()
            .WithMessage(
                "Product name is required.")
            .MaximumLength(255)
            .WithMessage(
                "Product name cannot exceed 255 characters.");

       
    }
}