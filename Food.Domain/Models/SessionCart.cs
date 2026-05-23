using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Food.Domain.Models
{
    public class SessionCart
    {
        public string Id { get; set; } // Format: "cart:{sessionId}:{userId}"
        public List<CartItem> Items { get; set; } = new List<CartItem>();
    }

    public class CartItem
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
