
using Core.DTOs;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    [Route("api/products")]
    [ApiController]
    public class ProductController: ControllerBase
    {
     
        private readonly IProductService _productService;
        private readonly IDistributedCache _cache;
        private readonly ILogger<ProductController> _logger; 
        private readonly ProductRedisCache _redisCacheService;

        public ProductController(IProductService productService, IDistributedCache cache, ProductRedisCache redisCacheService, ILogger<ProductController> logger)
        {
            _productService = productService;
            _cache = cache;
            _redisCacheService = redisCacheService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProductAsync([FromQuery] string? category)
        {
            //_logger.LogInformation("Fetching all products, category: {Category}", category ?? "all");

            string cacheKey = "product_ids:" + (category ?? "all");
            if (string.IsNullOrEmpty(cacheKey))
            {
                _logger.LogError("Cache key for product IDs is null or empty.");
                return BadRequest("Invalid cache key.");
            }
            string cachedData = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                List<Guid> ids = JsonConvert.DeserializeObject<List<Guid>>(cachedData);

                List<ProductDTO> products = new List<ProductDTO>();

                foreach (var id in ids)
                {
                    string productCacheKey = $"product:{id}";
                    string productData = await _cache.GetStringAsync(productCacheKey);

                    if (!string.IsNullOrEmpty(productData))
                    {
                        ProductDTO product = JsonConvert.DeserializeObject<ProductDTO>(productData);
                        products.Add(product);
                    }
                }

                if (products.Count > 0)
                {
                    return Ok(products);
                }
            }

            var productsFromDb = await _productService.GetAllProductsAsync(category);

            if (productsFromDb == null || productsFromDb.Count == 0)
            {
                return Ok(new List<ProductDTO>());
            }
            List<Guid> productIds = new List<Guid>();
            foreach (var product in productsFromDb)
            {
                productIds.Add(product.Id);
                string productCacheKey = $"product:{product.Id}";
                string productJson = JsonConvert.SerializeObject(product);
                await _cache.SetStringAsync(productCacheKey, productJson, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) 
                });
            }

            string idsCache = JsonConvert.SerializeObject(productIds);
            await _cache.SetStringAsync(cacheKey, idsCache, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) 
            });

            return Ok(productsFromDb);
        }

        [HttpGet("Id")]
        public async Task<IActionResult> GetProductByID(Guid id)
        {
            var productData = await _productService.GetProductByIdAsync(id);
            if (productData == null)
            {
                _logger.LogWarning("Product not found in database: {Id}", id);
                return NotFound(new { message = "Product with this Id not found in db"});
            }


            return Ok(productData);
        }

        //[Authorize(Policy = "AdminOnly")]
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] ProductCreateDTO dto)
        {
            var product = await _productService.CreateProductAsync(dto);
            await _redisCacheService.InvalidateProductCachesAsync();
            _logger.LogInformation("Neww product addded has ID: {Id} and caches invalidated", product.Id);
            return CreatedAtAction(nameof(GetProductByID), new { id = product.Id }, product);
        }

        //[Authorize(Policy = "AdminOnly")]
        [HttpPut("Id")]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] ProductUpdateDTO dto)
        {
            //_logger.LogInformation("Updating product ID: {Id}", id);
            var product = await _productService.UpdateProductAsync(id, dto);
            await _redisCacheService.InvalidateProductCachesAsync();
            _logger.LogInformation("Product updated, ID: {Id}, caches invalidated", id);
            return Ok(product);
        }

        //[Authorize(Policy = "AdminOnly")]
        [HttpDelete("Id")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            _logger.LogInformation("Deleting product ID: {Id}", id);
            await _productService.DeleteProductAsync(id);
            await _redisCacheService.InvalidateProductCachesAsync();
            _logger.LogInformation("Product deleted, ID: {Id}, caches invalidated", id);
            return Ok(new { message = $"Product with {id} is deleted successfully" });
        }


    }
}
