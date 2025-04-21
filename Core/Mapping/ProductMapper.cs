using Core.DTOs;
using Core.Domain;
namespace Core.Mapping
{
    public class ProductMapper
    {
        public static ProductDTO ToDto(Product product)
        {
            return new ProductDTO
            {
                Id = product.ID,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                Category = product.Category

            };
        }

        public static Product ToEntity(ProductCreateDTO productdto)
        {
            return new Product
            {
                Name = productdto.Name,
                Description = productdto.Description,
                Price = productdto.Price,
                StockQuantity = productdto.StockQuantity,
                Category = productdto.Category

            };

        }

        public static void UpdateEntity(Product entity, ProductUpdateDTO productUpdateDTO)
        {
            entity.Name = productUpdateDTO.Name;
            entity.Description = productUpdateDTO.Description;
            entity.Price = productUpdateDTO.Price;
            entity.Category = productUpdateDTO.Category;
            entity.StockQuantity = productUpdateDTO.StockQuantity;
        }
    }
}
