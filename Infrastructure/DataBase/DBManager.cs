using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Data;

namespace Infrastructure.DataBase
{
    public class DBManager: IDBManager
    {
        private readonly AppDBContext _context;
        public ProductRepository ProductRepository { get; }
        public OrderRepository OrderRepository { get; }

        public DBManager(AppDBContext context)
        {
            _context = context;
            ProductRepository = new ProductRepository(context);
            OrderRepository = new OrderRepository(context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
