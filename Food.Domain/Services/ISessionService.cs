using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Food.Domain.Models;

namespace Food.Domain.Services
{
    public interface ISessionService
    {
        Task<Session> CreateSessionAsync(string hostUserId, int restarantId, string? notes);
        Task<SessionJoin> JoinSessionAsync(int sessionId, string userId, List<CartItem> items);
        Task LeaveSessionAsync(int sessionId, string userId);
        Task<Session> CloseSessionAsync(int sessionId, string hostUserId);
        Task<Session> CancelSessionAsync(int sessionId, string hostUserId);
    }
}
