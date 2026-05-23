using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Food.Domain.Enums.Order;
using Food.Domain.Models.Identity;

namespace Food.Domain.Models
{
    public class Order :BaseModel
    {
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public decimal TotalAmount { get; set; }
        public decimal DeliveryCost { get; set; }

        public int SessionId { get; set; }
        public Session Session { get; set; }

        public string UserId { get; set; }
        public AppUser User { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    }
}
