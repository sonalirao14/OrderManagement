using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DTOs;
using Core.Domain;
using Core.Mapping;
using Infrastructure.DataBase;
using Application.Validators;
using Core.Interfaces;
using FluentValidation;
namespace Application.Services
{
    public class OrderService :IOrderService
    {
        private readonly IDBManager _databaseManager;
        private readonly IValidator<OrderCreateDTO> _orderCreateValidator;

        public OrderService(IDBManager databaseManager,IValidator<OrderCreateDTO> validator)
        {
            _databaseManager = databaseManager;
            _orderCreateValidator = validator;
        }

        public async Task<OrderDTO> CreateOrderAsync(OrderCreateDTO createOrderDto, string userId)
        {
            //var validator = new OrderCreateValidator();
            var validationResult = await _orderCreateValidator.ValidateAsync(createOrderDto);
            if (!validationResult.IsValid)
            {
                throw new ArgumentException(validationResult.Errors.First().ErrorMessage);
            }
            var productIds = createOrderDto.Items.Select(i => i.ProductId).ToList();

            //var allProducts = await _databaseManager.ProductRepository.GetAllAsync(null);
            //var products = allProducts.Where(p => productIds.Contains(p.ID)).ToList();
            var products = new List<Product>();
            foreach(var i in productIds)
            {
                if (i == Guid.Empty)
                {
                    throw new ArgumentException("Product ID is invalid.");
                }
                var product = await _databaseManager.ProductRepository.GetByIDAsync(i);
                products.Add(product);

            }
           

            if (products.Count != productIds.Count)
            {
                throw new ArgumentException("Some products are missing!!");
            }

            var productMap = products.ToDictionary(p => p.ID, p => p);
            foreach (var item in createOrderDto.Items)
            {
                var product = productMap[item.ProductId];
                if (product.StockQuantity < item.Quantity)
                {
                    throw new ArgumentException(
                        $"Not enough stock for {product.Name}. Available: {product.StockQuantity}, Requested: {item.Quantity}");
                }
            }

            var order = OrderMapper.MapToOrder(createOrderDto, userId, products);

         
            foreach (var item in order.Items)
            {
                var product = productMap[item.ProductID];
                product.StockQuantity -= item.Quantity;
                await _databaseManager.ProductRepository.UpdateAsync(product);
            }
            await _databaseManager.OrderRepository.AddAsync(order);
            await _databaseManager.SaveChangesAsync();

          
            return OrderMapper.MapToOrderDto(order, products);
        }

        public async Task<OrderDTO> GetOrderByIdAsync(Guid orderId, string userId)
        {
           
            if (orderId == Guid.Empty)
            {
                throw new ArgumentException("Order ID is invalid.");
            }
            var order = await _databaseManager.OrderRepository.GetByIdWithDetailsAsync(orderId);

            if (order == null)
            {
                throw new KeyNotFoundException("Order not found.");
            }
            if (order.UserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorize to see this order.");
            }
            var products = order.Items.Select(i => i.Product).ToList();
            return OrderMapper.MapToOrderDto(order, products);
        }

        public async Task CancelOrderAsync(Guid orderId, string userId)
        {
            if (orderId == Guid.Empty)
            {
                throw new ArgumentException("Not a valid Order exist with this ID");
            }
            var order = await _databaseManager.OrderRepository.GetByIdWithDetailsAsync(orderId);

            if (order == null)
            {
                throw new KeyNotFoundException("Order not found.");
            }
            if (order.UserId != userId)
            {
                throw new UnauthorizedAccessException("You can only cancel your own orders.");
            }
            foreach (var item in order.Items)
            {
                var product = item.Product;
                if (product == null)
                {
                    throw new InvalidOperationException($"Product {item.ProductID} not found.");
                }
                product.StockQuantity += item.Quantity;
               await _databaseManager.ProductRepository.UpdateAsync(product);
            }
             _databaseManager.OrderRepository.Delete(order);
            await _databaseManager.SaveChangesAsync();
        }
    }
}
