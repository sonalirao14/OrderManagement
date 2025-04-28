
using Application.Services;
using Core.DTOs;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using WebApplication1.Routes;
using WebApplication1.Services;
using Core.Exceptions;

namespace WebApplication1.Controllers
{
    //[Authorize(Policy = "AdminOnly")]
    //[Route("api/products")]
    [ApiController]
    public class ProductController: ControllerBase
    {

       
        private readonly IProductService _productService;
        //private readonly ILogger<ProductController> _logger;
        private readonly ILoggingService _logger;

        public ProductController(IProductService productService, ILoggingService logger)
        {
            _productService = productService;
            _logger = logger;
            //_redisCacheService = redisCacheService;
        }

        [HttpGet(ApiRoute.Products.GetAll)]
        public async Task<IActionResult> GetAllProductAsync([FromQuery] string? category)
        {
            var products = await _productService.GetAllProductsAsync(category);

            if (products == null || products.Count == 0)
            {
              await  _logger.LogInformationAsync("No products found for category: {Category}", category ?? "all");
                return Ok(new List<ProductDTO>());
            }

            return Ok(products);
        }

        [HttpGet(ApiRoute.Products.GetById)]
        public async Task<IActionResult> GetProductByID(Guid id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
               await _logger.LogWarningAsync("Product not found: {Id}", id);
                //return NotFound(new { message = "Product with this Id not found in db" });
                throw new NotFoundException("Product with this Id not found in db");
            }

            return Ok(product);
        }

        //[Authorize(Policy = "AdminOnly")]
        [Authorize(Roles = "admin")]
        [HttpPost(ApiRoute.Products.Create)]
        public async Task<IActionResult> CreateProduct([FromBody] ProductCreateDTO dto)
        {
            var product = await _productService.CreateProductAsync(dto);

            await _logger.LogInformationAsync("New product created with ID: {Id}", product.Id);
            //await _redisCacheService.InvalidateProductCachesAsync();
            return CreatedAtAction(nameof(GetProductByID), new { id = product.Id }, product);
        }

        //[Authorize(Policy = "AdminOnly")]
        [Authorize(Roles = "admin")]
        [HttpPut(ApiRoute.Products.Update)]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] ProductUpdateDTO dto)
        {
            var product = await _productService.UpdateProductAsync(id, dto);

            await _logger.LogInformationAsync("Product updated: {Id}", id);
            return Ok(product);
        }

        //[Authorize(Policy = "AdminOnly")]
        [Authorize(Roles = "admin")]
        [HttpDelete(ApiRoute.Products.Delete)]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            await _logger.LogInformationAsync("Deleting product ID: {Id}", id);
                await _productService.DeleteProductAsync(id);
            //await _redisCacheService.InvalidateProductCachesAsync();
            await _logger.LogInformationAsync("Product deleted, ID: {Id}, caches invalidated", id);
            return Ok(new { message = $"Product with {id} is deleted successfully" });

        }
     


    }
}
