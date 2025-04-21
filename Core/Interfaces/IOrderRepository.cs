using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Domain;

namespace Core.Interfaces
{
    public  interface IOrderRepository
    {
        Task<Order> GetByIdWithDetailsAsync(Guid orderId);
        //Task<Order> GetByIdAsync(Guid id);
        Task AddAsync(Order order);
        //void Update(Order order);
        void Delete(Order order);


    }
}
