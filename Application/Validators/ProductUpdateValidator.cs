using Core.DTOs;
using FluentValidation;

namespace Application.Validators
{
    public  class ProductUpdateValidator : AbstractValidator<ProductUpdateDTO>
    {
        public ProductUpdateValidator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty().WithMessage("Product Name is required field for updation").MaximumLength(10).WithMessage("Product name should not exceed 10 character limit"); ;
            RuleFor(x => x.Description).NotNull().NotEmpty().WithMessage("Description should not be empty");
            RuleFor(x => x.Price).NotNull().GreaterThan(0);
            RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Category).NotEmpty().WithMessage("Category Should be related to product");
        }
    }
}
