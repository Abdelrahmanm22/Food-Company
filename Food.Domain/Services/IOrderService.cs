using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Food.Domain.Enums.Order;
using Food.Domain.Models;

namespace Food.Domain.Services
{
    public interface IOrderService
    {
        Task<Order> ConfirmOrderAsync(int sessionId, string hostUserId);
        Task<Order> UpdateOrderStatusAsync(int orderId, OrderStatus orderStatus, string hostUserId);
    }
}
