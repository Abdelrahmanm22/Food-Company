using System.ComponentModel.DataAnnotations;
using Food.Domain.Enums.Order;

namespace Food.APIs.DTOs
{
    public class UpdateOrderStatusDto
    {
        [Required]
        public OrderStatus Status { get; set; }
    }
}
