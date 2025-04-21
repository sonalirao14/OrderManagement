using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
    public  class OrderDTO
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public long CreatedAt { get; set; }
        public List<OrderItemDTO> Items { get; set; }
    }
    public class OrderItemDTO
    {
        public Guid ProductId { get; set; }
        //public string ProductName { get; set; }
        //public string ProductDescription { get; set; }
        //public string ProductCategory { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
