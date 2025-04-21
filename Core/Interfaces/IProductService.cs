using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Domain;
using Core.DTOs;
namespace Core.Interfaces
{
    public interface IProductService
    {
        Task<ProductDTO> CreateProductAsync(ProductCreateDTO dto);
        Task<ProductDTO> UpdateProductAsync(Guid id, ProductUpdateDTO dto);
        Task DeleteProductAsync(Guid id);
        Task<ProductDTO> GetProductByIdAsync(Guid id);
        Task<List<ProductDTO>> GetAllProductsAsync(string? category);
    }
}
