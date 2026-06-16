using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Food.Domain;
using Food.Domain.Enums.Session;
using Food.Domain.Models;
using Food.Domain.Services;
using Food.Domain.Specifications;
using Food.Domain.Specifications.SessionSpec;
using Hangfire;
using Microsoft.AspNetCore.Http.Features;

namespace Food.Service
{
    public class SessionService : ISessionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IRedisCartService _redisCartService;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public SessionService(IUnitOfWork unitOfWork, IEmailService emailService, IRedisCartService redisCartService, IBackgroundJobClient backgroundJobClient)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _redisCartService = redisCartService;
            _backgroundJobClient = backgroundJobClient;
        }
        public async Task<Session> CreateSessionAsync(string hostUserId, int restarantId, string? notes)
        {
            var restaurantSpec = new BaseSpecifications<Restaurant>(r => r.Id == restarantId);
            var restaurant = await _unitOfWork.Repository<Restaurant>().GetByIdAsync(restaurantSpec);
            if (restaurant == null)  throw new ArgumentException("Restaurnat Not Found.");
            var session = new Session
            {
                HostUserId = hostUserId,
                RestaurantId = restarantId,
                DeliveryCost = restaurant.DefaultDeliveryCost,
                Notes = notes,
                Status = SessionStatus.Open,
                StartDate = DateTime.UtcNow
            };
            await _unitOfWork.Repository<Session>().AddAsync(session);
            await _unitOfWork.CompleteAsync();

            // Host Automatically Joins the Session
            var hostJoin = new SessionJoin
            {
                SessionId = session.Id,
                UserId = hostUserId,
                JoinedAt = DateTime.UtcNow
            };
            await _unitOfWork.Repository<SessionJoin>().AddAsync(hostJoin);
            await _unitOfWork.CompleteAsync();

            _backgroundJobClient.Enqueue<IEmailService>(service =>
                service.NotifyEmployeesForNewSessionAsync(restaurant.Name, notes, hostUserId));

            return session;

        }
        public async Task<SessionJoin> JoinSessionAsync(int sessionId, string userId, List<CartItem> items)
        {
            var sessionSpec = new SessionWithDetailsSpec(sessionId);
            var session = await _unitOfWork.Repository<Session>().GetByIdAsync(sessionSpec);
            if(session == null) throw new ArgumentException("Session Not Found.");
            if(session.Status != SessionStatus.Open) throw new InvalidOperationException("Session is not open for joining.");

            if (session.SessionJoins.Any(sj => sj.UserId == userId))
            {
                throw new InvalidOperationException("User already joined this session");
            }

            // Validate Items in Cart
            var itemIds = items.Select(i => i.ItemId).ToList();
            var itemSpec = new BaseSpecifications<Item>(i => itemIds.Contains(i.Id));
            itemSpec.Includes.Add(i => i.Category);
            var dbItemsList = await _unitOfWork.Repository<Item>().GetAllAsync(itemSpec);
            var dbItems = dbItemsList.ToList();

            if(dbItems.Count != itemIds.Distinct().Count())
            {
                throw new ArgumentException("One or more items in the cart were not found.");
            }
            foreach (var dbItem in dbItems) {
                if (dbItem.Category == null || dbItem.Category.RestaurantId != session.RestaurantId)
                {
                    throw new ArgumentException($"Item '{dbItem.Name}' does not belong to the restaurant for this session.");
                }
                if (!dbItem.IsAvailable)
                {
                    throw new InvalidOperationException($"Item '{dbItem.Name}' is not currently available.");
                }
            }

            
            var cartItems = new List<CartItem>();
            foreach (var item in items)
            {
                var dbItem = dbItems.First(i => i.Id == item.ItemId);
                cartItems.Add(new CartItem
                {
                    ItemId = item.ItemId,
                    ItemName = dbItem.Name,
                    Price = dbItem.Price,
                    Quantity = item.Quantity
                });
            }
            var cart = new SessionCart
            {
                Id = $"cart:{sessionId}:{userId}",
                Items = cartItems
            };

            await _redisCartService.UpdateCartAsync(cart);
            var sessionJoin = new SessionJoin
            {
                SessionId = sessionId,
                UserId = userId,
                JoinedAt = DateTime.UtcNow
            };
            await _unitOfWork.Repository<SessionJoin>().AddAsync(sessionJoin);
            await _unitOfWork.CompleteAsync();

            var joinSpec = new SessionJoinSpec(sessionId, userId);
            var joined = await _unitOfWork.Repository<SessionJoin>().GetByIdAsync(joinSpec);
            return joined;
        }

        public async Task LeaveSessionAsync(int sessionId, string userId)
        {
            var sessionSpec = new BaseSpecifications<Session>(s => s.Id == sessionId);
            var session = await _unitOfWork.Repository<Session>().GetByIdAsync(sessionSpec);
            if (session == null) throw new ArgumentException("Session not found");
            if (session.Status != SessionStatus.Open) throw new InvalidOperationException("Session is not open");

            if (session.HostUserId == userId)
            {
                throw new InvalidOperationException("Host cannot leave the session.");
            }

            await _redisCartService.DeleteCartAsync($"cart:{sessionId}:{userId}");

            var joinSpec = new SessionJoinSpec(sessionId, userId);
            var joinRecord = await _unitOfWork.Repository<SessionJoin>().GetByIdAsync(joinSpec);
            if (joinRecord != null)
            {
                _unitOfWork.Repository<SessionJoin>().Delete(joinRecord);
                await _unitOfWork.CompleteAsync();
            }
        }
        public async Task<Session> CancelSessionAsync(int sessionId, string hostUserId)
        {
            var sessionSpec = new SessionWithDetailsSpec(sessionId);
            var session = await _unitOfWork.Repository<Session>().GetByIdAsync(sessionSpec);
            if (session == null) throw new ArgumentException("Session not found");
            if (session.Status != SessionStatus.Open) throw new InvalidOperationException("Session is not open");
            if (session.HostUserId != hostUserId) throw new UnauthorizedAccessException("Only the host can cancel the session");


            var participantIds = session.SessionJoins.Select(sj => sj.UserId).ToList();
            await _redisCartService.DeleteAllCartsForSessionAsync(sessionId, participantIds);

            session.Status = SessionStatus.Cancelled;
            session.EndDate = DateTime.UtcNow;

            _unitOfWork.Repository<Session>().Update(session);
            await _unitOfWork.CompleteAsync();
            _backgroundJobClient.Enqueue<IEmailService>(service =>
                service.NotifyParticipantsSessionCancelledAsync(sessionId, session.Restaurant.Name));
            return session;
        }
        public async Task<Session> CloseSessionAsync(int sessionId, string hostUserId)
        {
            var sessionSpec = new BaseSpecifications<Session>(s => s.Id == sessionId);
            var session = await _unitOfWork.Repository<Session>().GetByIdAsync(sessionSpec);
            if (session == null) throw new ArgumentException("Session not found.");
            if (session.Status != SessionStatus.Open) throw new InvalidOperationException("Only open sessions can be closed.");
            if (session.HostUserId != hostUserId) throw new UnauthorizedAccessException("Only the host can close the session.");

            session.Status = SessionStatus.Closed;
            _unitOfWork.Repository<Session>().Update(session);
            await _unitOfWork.CompleteAsync();
            return session;
        }
    }
}
