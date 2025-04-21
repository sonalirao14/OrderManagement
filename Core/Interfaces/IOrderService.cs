using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DTOs;

namespace Core.Interfaces
{
    public interface IOrderService
    {

        Task<OrderDTO> CreateOrderAsync(OrderCreateDTO createOrderDto, string userId);
        Task<OrderDTO> GetOrderByIdAsync(Guid orderId, string userId);
        Task CancelOrderAsync(Guid orderId, string userId);
    }
}
