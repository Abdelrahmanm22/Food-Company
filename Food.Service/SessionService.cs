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

namespace Food.Service
{
    public class SessionService : ISessionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;

        public SessionService(IUnitOfWork unitOfWork,IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
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

            await _emailService.NotifyEmployeesForNewSessionAsync(restaurant.Name, notes, hostUserId);

            return session;

        }
    }
}
