using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Domain;

namespace Core.Interfaces
{
    public interface IProductRepository
    {
        Task<Product> GetByIDAsync(Guid id);
        Task<List<Product>> GetAllAsync(string?category=null);
        Task<Product> GetByNameAsync(string name);
        Task<Product> AddAsync(Product product);
        Task<Product> UpdateAsync(Product product);
        Task DeleteAsync(Guid id);




    }
}
