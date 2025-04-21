using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Data;

namespace Infrastructure.DataBase
{
    public interface IDBManager
    {
        ProductRepository ProductRepository { get; }
        OrderRepository OrderRepository { get; }
        Task<int> SaveChangesAsync();
    }
}
