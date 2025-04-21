using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Core.Domain;
using Core.Interfaces;

namespace Infrastructure.Data
{
    public class OrderRepository: IOrderRepository
    {
        private readonly AppDBContext _context;

        public OrderRepository(AppDBContext context)
        {
            _context = context;
        }

        public async Task<Order> GetByIdWithDetailsAsync(Guid orderId)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }


        //public async Task<Order> GetByIdAsync(Guid id)
        //{
        //    return await _context.Orders
        //        .Include(o => o.Items)
        //        .ThenInclude(i => i.Product)
        //        .FirstOrDefaultAsync(o => o.Id == id);
        //}

        public async Task AddAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
        }

        //public void Update(Order order)
        //{
        //    _context.Orders.Update(order);
        //}

        public void Delete(Order order)
        {
            _context.Orders.Remove(order);
        }
    }
}
