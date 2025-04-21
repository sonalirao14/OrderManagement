using FluentValidation;
using FluentValidation.AspNetCore;
using Core.DTOs;
namespace Application.Validators
{
    public class ProductValidator : AbstractValidator<ProductCreateDTO>
    {
        public ProductValidator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty().WithMessage("Name can't be empty ").MaximumLength(10).WithMessage("Name field can contains only 10 characters");
            RuleFor(x => x.Description).NotNull().NotEmpty().WithMessage("Description Should be valid");
            RuleFor(x => x.Price).NotNull().GreaterThan(0).WithMessage("Price is required");
            RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0).WithMessage("StockQuanity should be valid atleast one product should exist");
            RuleFor(x => x.Category).NotEmpty().WithMessage("Category is required for every product");
        }
    }
}
