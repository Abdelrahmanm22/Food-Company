using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Food.Domain;
using Food.Domain.Enums.Order;
using Food.Domain.Enums.Session;
using Food.Domain.Models;
using Food.Domain.Services;
using Food.Domain.Specifications;
using Food.Domain.Specifications.OrderSpec;
using Food.Domain.Specifications.SessionSpec;
using Hangfire;

namespace Food.Service
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCartService _redisCartService;
        private readonly IEmailService _emailService;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public OrderService(IUnitOfWork unitOfWork, IRedisCartService redisCartService, IEmailService emailService, IBackgroundJobClient backgroundJobClient)
        {
            _unitOfWork = unitOfWork;
            _redisCartService = redisCartService;
            _emailService = emailService;
            _backgroundJobClient = backgroundJobClient;
        }

        public async Task<Order> ConfirmOrderAsync(int sessionId, string hostUserId)
        {
            // 1. Load the session with all participants
            var sessionSpec = new SessionWithDetailsSpec(sessionId);
            var session = await _unitOfWork.Repository<Session>().GetByIdAsync(sessionSpec);

            if (session == null)
                throw new ArgumentException("Session not found.");

            if (session.HostUserId != hostUserId)
                throw new UnauthorizedAccessException("Only the session host can confirm the order.");

            if (session.Status != SessionStatus.Open && session.Status != SessionStatus.Closed)
                throw new InvalidOperationException($"Cannot confirm order for a session with status '{session.Status}'.");

            if (session.Order != null)
                throw new InvalidOperationException("An order has already been confirmed for this session.");

            // 2. Fetch all carts from Redis for every participant
            var participantIds = session.SessionJoins.Select(sj => sj.UserId).ToList();
            var allCarts = await _redisCartService.GetAllCartsForSessionAsync(sessionId, participantIds);

            if (!allCarts.Any())
                throw new InvalidOperationException("No items found in any cart for this session. At least one participant must have items.");

            // 3. Build OrderDetails from Redis carts
            var orderDetails = new List<OrderDetail>();
            decimal totalAmount = 0;

            foreach (var cart in allCarts)
            {
                // Cart Id format: "cart:{sessionId}:{userId}"
                var cartUserId = cart.Id.Split(':').LastOrDefault();
                if (string.IsNullOrEmpty(cartUserId)) continue;

                foreach (var cartItem in cart.Items)
                {
                    var detail = new OrderDetail
                    {
                        ItemId = cartItem.ItemId,
                        Quantity = cartItem.Quantity,
                        Price = cartItem.Price,
                        UserId = cartUserId
                    };
                    orderDetails.Add(detail);
                    totalAmount += cartItem.Price * cartItem.Quantity;
                }
            }

            // 4. Create the Order entity
            var order = new Order
            {
                SessionId = sessionId,
                UserId = hostUserId,
                Status = OrderStatus.Confirmed,
                TotalAmount = totalAmount,
                DeliveryCost = session.DeliveryCost,
                OrderDate = DateTime.UtcNow,
                OrderDetails = orderDetails
            };

            await _unitOfWork.Repository<Order>().AddAsync(order);

            // 5. Update session status → Ordered
            session.Status = SessionStatus.Ordered;
            session.EndDate = DateTime.UtcNow;
            _unitOfWork.Repository<Session>().Update(session);

            await _unitOfWork.CompleteAsync();

            // 6. Clear all Redis carts for this session
            await _redisCartService.DeleteAllCartsForSessionAsync(sessionId, participantIds);

            // 7. Notify all participants
            _backgroundJobClient.Enqueue<IEmailService>(service =>
                service.NotifyOrderConfirmedAsync(order.Id));

            // 8. Return the order with full details
            var fullOrderSpec = new OrderWithDetailsSpec(order.Id);
            var fullOrder = await _unitOfWork.Repository<Order>().GetByIdAsync(fullOrderSpec);
            return fullOrder!;
        }

        public async Task<Order> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, string hostUserId)
        {
            // 1. Load the order with details
            var orderSpec = new OrderWithDetailsSpec(orderId);
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(orderSpec);

            if (order == null)
                throw new ArgumentException("Order not found.");

            // 2. Verify the caller is the session host
            if (order.Session?.HostUserId != hostUserId)
                throw new UnauthorizedAccessException("Only the session host can update the order status.");

            // 3. Guard: cannot change a terminal state
            if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Cancelled)
                throw new InvalidOperationException($"Order is already '{order.Status}' and cannot be updated.");

            // 4. Validate forward-only status transitions
            var validTransitions = new Dictionary<OrderStatus, List<OrderStatus>>
            {
                { OrderStatus.Pending,   new List<OrderStatus> { OrderStatus.Confirmed, OrderStatus.Cancelled } },
                { OrderStatus.Confirmed, new List<OrderStatus> { OrderStatus.Preparing, OrderStatus.Cancelled } },
                { OrderStatus.Preparing, new List<OrderStatus> { OrderStatus.Delivered, OrderStatus.Cancelled } },
            };

            if (validTransitions.TryGetValue(order.Status, out var allowed) && !allowed.Contains(newStatus))
                throw new InvalidOperationException(
                    $"Cannot transition order from '{order.Status}' to '{newStatus}'. " +
                    $"Allowed next statuses: {string.Join(", ", allowed)}.");

            // 5. Apply the new status
            order.Status = newStatus;
            _unitOfWork.Repository<Order>().Update(order);
            await _unitOfWork.CompleteAsync();

            // 6. Send delivery notification if the order is now Delivered
            if (newStatus == OrderStatus.Delivered)
                _backgroundJobClient.Enqueue<IEmailService>(service =>
                    service.NotifyOrderDeliveredAsync(orderId));

            return order;
        }
    }
}
