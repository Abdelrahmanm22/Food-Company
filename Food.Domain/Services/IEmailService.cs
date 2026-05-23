using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Food.Domain.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, string userId);
        Task NotifyEmployeesForNewSessionAsync(string restaurantName, string? notes, string? excludeUserId = null);
    }
}
