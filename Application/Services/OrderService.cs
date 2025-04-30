using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DTOs;
using Core.Domain;
using Core.Mapping;
using Core.Exceptions;
using Infrastructure.DataBase;
using Application.Validators;
using Core.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Logging;
namespace Application.Services
{
    public class OrderService :IOrderService
    {
        private readonly IDBManager _databaseManager;
        private readonly IValidator<OrderCreateDTO> _orderCreateValidator;
        private readonly ICacheService<List<ProductDTO>> _listCacheService;
        //private readonly ILoggingService _logger;
        private readonly ILogger<OrderService> _logger;
        public OrderService(IDBManager databaseManager,IValidator<OrderCreateDTO> validator, ICacheService<List<ProductDTO>> listCacheService, ILogger<OrderService> logger)
        {
            _databaseManager = databaseManager;
            _orderCreateValidator = validator;
            _listCacheService = listCacheService;
            _logger = logger;
        }

        public async Task<OrderDTO> CreateOrderAsync(OrderCreateDTO createOrderDto, string userId)
        {
            //var validator = new OrderCreateValidator();
            var validationResult = await _orderCreateValidator.ValidateAsync(createOrderDto);
            if (!validationResult.IsValid)
            {
                throw new Core.Exceptions.ValidationException(validationResult.Errors.First().ErrorMessage);
            }
            var productIds = createOrderDto.Items.Select(i => i.ProductId).ToList();

            //var allProducts = await _databaseManager.ProductRepository.GetAllAsync(null);
            //var products = allProducts.Where(p => productIds.Contains(p.ID)).ToList();
            var products = new List<Product>();
            foreach(var i in productIds)
            {
                if (i == Guid.Empty)
                {
                    throw new Core.Exceptions.ValidationException ("Product ID is invalid.");
                }
                var product = await _databaseManager.ProductRepository.GetByIDAsync(i);
                if (product == null)
                {
                    throw new Core.Exceptions.ValidationException($"Product with ID {i} not found.");
                }
                products.Add(product);

            }
           

            if (products.Count != productIds.Count)
            {
                throw new NotFoundException("Some products are missing!!");
            }

            var productMap = products.ToDictionary(p => p.ID, p => p);
            foreach (var item in createOrderDto.Items)
            {
                var product = productMap[item.ProductId];
                if (product.StockQuantity < item.Quantity)
                {
                   throw new InsufficientStockException(
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
            await _listCacheService.InvalidateByPatternAsync("product_ids:*");
            //await _logger.LogInformationAsync("Invalidated cache when order has beeen created!!");
            _logger.LogInformation("Invalidated Cache when order has been created!!");



            return OrderMapper.MapToOrderDto(order, products);
        }

        public async Task<OrderDTO> GetOrderByIdAsync(Guid orderId, string userId)
        {
           
            if (orderId == Guid.Empty)
            {
                throw new Core.Exceptions.ValidationException("Order ID is invalid.");
            }
            var order = await _databaseManager.OrderRepository.GetByIdWithDetailsAsync(orderId);

            if (order == null)
            {
                throw new NotFoundException("Order not found.");
            }
            if (order.UserId != userId)
            {
                throw new UnauthorizedException("You are not authorize to see this order.");
            }
            var products = order.Items.Select(i => i.Product).ToList();
            return OrderMapper.MapToOrderDto(order, products);
        }

        public async Task CancelOrderAsync(Guid orderId, string userId)
        {
            if (orderId == Guid.Empty)
            {
                throw new Core.Exceptions.ValidationException("Not a valid Order exist with this ID");
            }
            var order = await _databaseManager.OrderRepository.GetByIdWithDetailsAsync(orderId);

            if (order == null)
            {
                throw new NotFoundException("Order not found.");
            }
            if (order.UserId != userId)
            {
                throw new UnauthorizedException("You can only cancel your own orders.");
            }
            foreach (var item in order.Items)
            {
                var product = item.Product;
                if (product == null)
                {
                    throw new NotFoundException($"Product {item.ProductID} not found.");
                }
                product.StockQuantity += item.Quantity;
               await _databaseManager.ProductRepository.UpdateAsync(product);
            }
             _databaseManager.OrderRepository.Delete(order);
            await _databaseManager.SaveChangesAsync();
            await _listCacheService.InvalidateByPatternAsync("product_ids:*");
            _logger.LogInformation("Invalidated Cache when order has been created!!");
        }
    }
}
