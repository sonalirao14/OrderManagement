using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Data;
using Core.Interfaces;

namespace Infrastructure.DataBase
{
    public interface IDBManager
    {
        IProductRepository ProductRepository { get; }
        IOrderRepository OrderRepository { get; }
        Task<int> SaveChangesAsync();
    }
}
