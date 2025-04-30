using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.DTOs;
using Core.Domain;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Infrastructure.Caching;
using Core.Mapping;
using Core.Exceptions;

namespace Application.Services
{
    public class ProductService : IProductService
    {
        
        private readonly IProductRepository _repository;
        private readonly ICacheService<List<ProductDTO>> _listCacheService;
        private readonly IRedisConnectionFactory _redisFactory;
        //private readonly ILoggingService _logger;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IProductRepository repository,
            ICacheService<List<ProductDTO>> listCacheService,
            IRedisConnectionFactory redisFactory,
            ILogger<ProductService> logger)
        {
            _repository = repository;
            _listCacheService = listCacheService;
            _redisFactory = redisFactory;
            _logger = logger;
        }

        public async Task<List<ProductDTO>> GetAllProductsAsync(string? category)
        {
            string cacheKey = $"product_ids:{category ?? "all"}";
            var cached = await _listCacheService.GetAsync(cacheKey);
            if (cached != null)
            {
                //await _logger.LogInformationAsync("Retrieved product list from cache for category {Category}", category ?? "all");
                _logger.LogInformation("Retrieved product list from cache for category {Category}", category ?? "all");
                return cached;
            }

            var products = await _repository.GetAllAsync(category);
            var productDtos = products.Select(ProductMapper.ToDto).ToList();

            if (productDtos.Any())
            {
                await _listCacheService.SetAsync(cacheKey, productDtos, TimeSpan.FromMinutes(5));
                //await _logger.LogInformationAsync("Cached product list for category {Category}", category ?? "all");
                _logger.LogInformation("Cached product list for category {Category}", category ?? "all");
            }

            return productDtos;
        }

        public async Task<ProductDTO> GetProductByIdAsync(Guid id)
        {
            var product = await _repository.GetByIDAsync(id);
            if (product == null)
            {
                _logger.LogInformation("Invalidated Cache when order has been created!!");
                throw new NotFoundException($"Product with ID {id} not found");
            }

            return ProductMapper.ToDto(product);
        }

        public async Task<ProductDTO> CreateProductAsync(ProductCreateDTO productDTO)
        {
            var product = ProductMapper.ToEntity(productDTO);
            var existingProduct = await _repository.GetByNameAsync(product.Name);
            if (existingProduct != null)
            {
                existingProduct.StockQuantity += product.StockQuantity;
                var updated = await _repository.UpdateAsync(existingProduct);
                await InvalidateProductCachesAsync();
                _logger.LogInformation("Updated existing product {ProductId} and invalidated caches", existingProduct.ID);
                return ProductMapper.ToDto(updated);
            }

            product.ID = Guid.NewGuid();
            var created = await _repository.AddAsync(product);
            await InvalidateProductCachesAsync();
            _logger.LogInformation("Created new product {ProductId} and invalidated caches", created.ID);
            return ProductMapper.ToDto(created);
        }

        public async Task<ProductDTO> UpdateProductAsync(Guid id, ProductUpdateDTO dto)
        {
            var product = await _repository.GetByIDAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found", id);
                throw new NotFoundException($"Product with ID {id} not found");
            }

            ProductMapper.UpdateEntity(product, dto);
            var updated = await _repository.UpdateAsync(product);
            await InvalidateProductCachesAsync();
            //await _logger.LogInformationAsync("Updated product {ProductId} and invalidated caches", id);
            _logger.LogInformation("Updated product {ProductId} and Invalidated caches", id);
            return ProductMapper.ToDto(updated);
        }

        public async Task DeleteProductAsync(Guid id)
        {
            var product = await _repository.GetByIDAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found", id);
                throw new NotFoundException($"Product with ID {id} not found");
            }

            await _repository.DeleteAsync(id);
            await InvalidateProductCachesAsync();
            _logger.LogInformation("Deleted product {ProductId} and invalidated caches", id);
        }

        private async Task InvalidateProductCachesAsync()
        {
            try
            {
                await _listCacheService.InvalidateByPatternAsync("product_ids:*");
                _logger.LogInformation("Successfully invalidated product cache");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while invalidating product cache");
                throw;
            }
        }   

    }
}
