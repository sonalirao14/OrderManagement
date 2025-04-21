using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain
{
    public class OrderItem
    {
        public Guid ProductID { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

         public Guid OrderId { get; set; }
        public Order Order { get; set; }
        public Product Product { get; set; }

    }
}
