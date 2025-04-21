using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain
{
    public class Order
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public long CreatedAt { get; set; }
        public List<OrderItem> Items { get; set; }
    }
}
