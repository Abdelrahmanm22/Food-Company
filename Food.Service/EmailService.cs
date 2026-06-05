using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Food.Domain;
using Food.Domain.Models;
using Food.Domain.Models.Identity;
using Food.Domain.Services;
using Food.Domain.Specifications.SessionSpec;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Food.Service
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EmailService> _logger;
        private readonly UserManager<AppUser> _userManager;

        public EmailService(IConfiguration configuration,IUnitOfWork unitOfWork,ILogger<EmailService> logger, UserManager<AppUser> userManager)
        {
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _userManager = userManager;
        }
        public async Task SendEmailAsync(string to, string subject, string body, string userId)
        {
            var email = new Email
            {
                To = to,
                Subject = subject,
                Body = body,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                var smtpSettings = _configuration.GetSection("SmtpSettings");
                var host = smtpSettings["Host"];
                var portStr = smtpSettings["Port"];
                var username = smtpSettings["Username"];
                var password = smtpSettings["Password"];
                var from = smtpSettings["From"];

                if(!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(portStr))
                {
                    using (var client = new SmtpClient(host, int.Parse(portStr))
                    {
                        Credentials = new NetworkCredential(username, password),
                        EnableSsl = true
                    })
                    {
                        await client.SendMailAsync(from, to, subject, body);
                    }
                    _logger.LogInformation("Email sent successfully to {To} with subject {Subject}", to, subject);
                }
                else
                {
                    _logger.LogWarning("SMTP Settings not fully configured. Email was not sent.");
                }

            }catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To} with subject {Subject}", to, subject);
            }

            try
            {
                await _unitOfWork.Repository<Email>().AddAsync(email);
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save email log to the database");
            }
        }
        public async Task NotifyEmployeesForNewSessionAsync(string restaurantName, string? notes, string? excludeUserId = null)
        {
            var employees = await _userManager.GetUsersInRoleAsync(UserRoles.Employee);
            foreach(var employee in employees)
            {
                if (string.IsNullOrEmpty(employee.Email)) continue;
                if (excludeUserId != null && employee.Id == excludeUserId) continue;
                var subject = "New Food Session Started!";
                var body = $"Hello {employee.UserName},\n\n" +
                           $"A new food session has been started at {restaurantName}.\n" +
                           $"Session Notes: {notes ?? "No notes provided"}\n\n" +
                           $"Join the session and place your order before it closes!\n\n" +
                           "Bon appétit!";
                await SendEmailAsync(employee.Email, subject, body, employee.Id);
            }
        }
        public async Task NotifyParticipantsSessionCancelledAsync(int sessionId, string restaurantName)
        {
            var sessionSpec = new SessionWithDetailsSpec(sessionId);
            var session = await _unitOfWork.Repository<Session>().GetByIdAsync(sessionSpec);
            if (session == null) return;

            foreach (var participant in session.SessionJoins)
            {
                if (participant.User == null || string.IsNullOrEmpty(participant.User.Email)) continue;

                var subject = "Food Session Cancelled";
                var body = $"Hello {participant.User.UserName},\n\n" +
                           $"The Food session for '{restaurantName}' has been cancelled by the host ({session.HostUser.UserName}).\n" +
                           $"Any items in your cart for this session have been cleared.\n\n" +
                           $"Best regards.";
                await SendEmailAsync(participant.User.Email, subject, body, participant.UserId);
            }
        }
    }
}
