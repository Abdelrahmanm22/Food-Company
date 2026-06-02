using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Food.Domain.Models;

namespace Food.Domain.Services
{
    public interface IRedisCartService
    {
        Task<SessionCart?> GetCartAsync(string cartKey);
        Task<SessionCart?> UpdateCartAsync(SessionCart cart);
        Task<bool> DeleteCartAsync(string cartKey);
        Task<List<SessionCart>> GetAllCartsForSessionAsync(int sessionId, List<string> userIds);
        Task DeleteAllCartsForSessionAsync(int sessionId, List<string> userIds);
    }
}
