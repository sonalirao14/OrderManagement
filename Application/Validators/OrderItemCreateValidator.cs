using Core.DTOs;
using FluentValidation;

namespace Application.Validators
{
    
    public class OrderCreateValidator : AbstractValidator<OrderCreateDTO>
    {
        public OrderCreateValidator()
        {
            RuleFor(x => x.Items)
                .NotNull()
                .WithMessage("Order items are required.")
                .Must(items => items != null && items.Count > 0)
                .WithMessage("Order must contain at least one item.");

            // Rule for each item
            RuleForEach(x => x.Items)
                .SetValidator(new OrderItemCreateValidator());
        }
    }
    public class OrderItemCreateValidator : AbstractValidator<CreateOrderItemDTO>
    {

        public OrderItemCreateValidator()
        {

            RuleFor(x => x.ProductId)
                .NotEmpty()
                .WithMessage("Product ID is required.");

            // Rule for Quantity
            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than zero.");
        }

    }

}
