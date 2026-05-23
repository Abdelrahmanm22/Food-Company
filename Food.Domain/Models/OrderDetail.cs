using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Food.Domain.Models.Identity;

namespace Food.Domain.Models
{
    public class OrderDetail : BaseModel
    {
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public int ItemId { get; set; }
        public Item Item { get; set; }

        public string? UserId { get; set; }
        public AppUser? User { get; set; }
    }
}
