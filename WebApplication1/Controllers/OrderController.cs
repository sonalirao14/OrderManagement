using Core.DTOs;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [Authorize]    
    [ApiController]
    [Route("api/Orders")]
    public class OrderController:ControllerBase
    {
     
        private readonly IOrderService _orderService;
        private readonly ProductRedisCache _productRedisCache;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderService orderService, ProductRedisCache productRedisCache, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _productRedisCache = productRedisCache;
            _logger = logger;

        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreateDTO createOrderDto)
        {
            try
            {
                var userIdString = User.FindFirst("userId")?.Value;
                _logger.LogInformation("UserId: " + userIdString);
                if (string.IsNullOrEmpty(userIdString))
                {
                    return Unauthorized("Invalid or missing token!!!");
                }


                var order = await _orderService.CreateOrderAsync(createOrderDto, userIdString);
                await _productRedisCache.InvalidateProductCachesAsync();
                _logger.LogInformation("Invalideted product cache as product stock has been changed after creating order");
                return CreatedAtAction(nameof(GetOrder), new { id = order.Id },new { message = $"Order created successfully and Order ID you can see here: {order.Id}" });
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest("Something went wrong");
            }
        }

        [HttpGet("GetById")]
        public async Task<IActionResult> GetOrder(Guid id)
        {
            try
            {
                var userIdString = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userIdString))
                {
                    return Unauthorized("Invalid or missing token!");
                }

                var order = await _orderService.GetOrderByIdAsync(id, userIdString);
                return Ok(order);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest("Unexpected error while getting order based on the ID provided!");
            }
        }

        [HttpDelete("DeleteById")]
        public async Task<IActionResult> CancelOrder(Guid id)
        {
            try
            {
                var userIdString = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userIdString))
                {
                    return Unauthorized("Invalid or missing token! Check it again.");
                }
                await _orderService.CancelOrderAsync(id, userIdString);
                await _productRedisCache.InvalidateProductCachesAsync();
                _logger.LogInformation("Invalidated cache when order has been deleted and product is being restored in the stock again!!");
                return Ok(new { Message = "Order cancelled successfully." });
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest("Unexpected error while cancelling order you have placed..");
            }
        }
    }
}
