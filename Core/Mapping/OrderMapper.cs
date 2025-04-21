using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Domain;
using Core.DTOs;

namespace Core.Mapping
{
    public class OrderMapper
    {
        public static Order MapToOrder(OrderCreateDTO createOrderDto, string userId, List<Product> products)
        {
            var productMap = products.ToDictionary(p => p.ID, p => p);

            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Items = createOrderDto.Items.Select(item => new OrderItem
                {
                    ProductID = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = productMap[item.ProductId].Price
                }).ToList()
            };

            return order;
        }
        public static OrderDTO MapToOrderDto(Order order, List<Product> products)
        {
            var productMap = products.ToDictionary(p => p.ID, p => p);

            return new OrderDTO
            {
                Id = order.Id,
                UserId = order.UserId,
                CreatedAt = order.CreatedAt,
                Items = order.Items.Select(item => new OrderItemDTO
                {
                    ProductId = item.ProductID,
                    //ProductName = productMap[item.ProductID].Name,
                    //ProductDescription = productMap[item.ProductID].Description,
                    //ProductCategory = productMap[item.ProductID].Category,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                }).ToList()
            };
        }

    }
}
