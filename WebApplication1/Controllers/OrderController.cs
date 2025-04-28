using Core.DTOs;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Routes;
using Core.Exceptions;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [Authorize]    
    [ApiController]
    //[Route("api/Orders")]
    public class OrderController:ControllerBase
    {
     
        private readonly IOrderService _orderService;
        //private readonly ProductRedisCache _productRedisCache;
        //private readonly ILogger<OrderController> _logger;
        private readonly ILoggingService _logger;

        public OrderController(IOrderService orderService, ILoggingService  logger)
        {
            _orderService = orderService;
            //_productRedisCache = productRedisCache;
            _logger = logger;

        }

        [HttpPost(ApiRoute.Orders.Create)]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreateDTO createOrderDto)
        {
            try
            {
                //var userIdString = User.FindFirst("userId")?.Value;
                var userIdString = HttpContext.Items["UserId"]?.ToString();
                await _logger.LogInformationAsync("UserId: " + userIdString);
                if (string.IsNullOrEmpty(userIdString))
                {
                    //return Unauthorize("Invalid or missing token!!!");
                    throw new UnauthorizedException("Invalid or missing token");
                }


                var order = await _orderService.CreateOrderAsync(createOrderDto, userIdString);
                //await _productRedisCache.InvalidateProductCachesAsync();
                await  _logger.LogInformationAsync("Invalideted product cache as product stock has been changed after creating order");
                return CreatedAtAction(nameof(GetOrder), new { id = order.Id },new { message = $"Order created successfully and Order ID you can see here: {order.Id}" });
            }
            catch (Exception e)
            {
              await _logger.LogErrorAsync(e,"Eorr while creating order");
                //return BadRequest(new { error = e.Message });
                throw new Exception("unknown exception occured while ordering");
            }
        }

        [HttpGet(ApiRoute.Orders.GetById)]
        public async Task<IActionResult> GetOrder(Guid id)
        {
            try
            {
                var userIdString = HttpContext.Items["UserId"]?.ToString();
                if (string.IsNullOrEmpty(userIdString))
                {
                    throw new UnauthorizedException("Invalid or missing token!");
                }

                var order = await _orderService.GetOrderByIdAsync(id, userIdString);
                return Ok(order);
            }
            catch (Exception e)
            {
                await _logger.LogErrorAsync(e, "Error while getting product order details");
                //return BadRequest(new {error = e.Message});
                throw new Exception("Unknow error occured");
            }
        }

        [HttpDelete(ApiRoute.Orders.Delete)]
        public async Task<IActionResult> CancelOrder(Guid id)
        {
            try
            {
                var userIdString = HttpContext.Items["UserId"]?.ToString();
                if (string.IsNullOrEmpty(userIdString))
                {
                    throw new UnauthorizedException("Invalid or missing token! Check it again.");
                }
                await _orderService.CancelOrderAsync(id, userIdString);
                //await _productRedisCache.InvalidateProductCachesAsync();
               //await  _logger.LogInformationAsync("Invalidated cache when order has been deleted and product is being restored in the stock again!!");
                return Ok(new { Message = "Order cancelled successfully." });
            }
            catch (Exception e)
            {
                await _logger.LogErrorAsync(e,"Error while cancelling order!!");
                //return BadRequest(new {error=e.Message});
                throw new Exception("An unknown exception occured while cancelling order");
            }
        }
    }
}
