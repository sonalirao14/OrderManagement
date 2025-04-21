using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Domain;
using Core.DTOs;
using Core.Interfaces;
using Core.Mapping;

namespace Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
         public ProductService(IProductRepository repository)
        {
            _repository = repository;
        }
      

        public async Task<List<ProductDTO>> GetAllProductsAsync(string? category)
        {
            var products = await _repository.GetAllAsync(category);
            return products.Select(ProductMapper.ToDto).ToList();
        }
        public async Task<ProductDTO> GetProductByIdAsync(Guid id)
        {
            var product = await _repository.GetByIDAsync(id);
            if (product is null)
            {
                throw new Exception("Product with this ID not exist");
            }
            return ProductMapper.ToDto(product);
        }
        public async Task<ProductDTO> CreateProductAsync(ProductCreateDTO productDTO)
        {
            var product = ProductMapper.ToEntity(productDTO);
            var existingProduct = await _repository.GetByNameAsync(product.Name);
            if(existingProduct != null)
            {
                existingProduct.StockQuantity += product.StockQuantity;
                var updated = await _repository.UpdateAsync(existingProduct);
                return ProductMapper.ToDto(updated);
            }
            product.ID = Guid.NewGuid();
            var created= await _repository.AddAsync(product);
            return ProductMapper.ToDto(created);

        }
        public async Task<ProductDTO> UpdateProductAsync(Guid id, ProductUpdateDTO dto)
        {
            var product = await _repository.GetByIDAsync(id);
            if(product is null)
            {
               throw new Exception("Product not found");
            }
            ProductMapper.UpdateEntity(product, dto);
            var updated = await _repository.UpdateAsync(product);
            return ProductMapper.ToDto(updated);
        }

        public async Task DeleteProductAsync(Guid id)
        {
            await _repository.DeleteAsync(id);
           
        }

       
    }
}
