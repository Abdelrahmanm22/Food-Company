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
    }
}
