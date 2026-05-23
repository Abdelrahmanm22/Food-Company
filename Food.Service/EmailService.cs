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

            //try
            //{
            //    await _unitOfWork.Repository<Email>().AddAsync(email);
            //    await _unitOfWork.CompleteAsync();
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex, "Failed to save email log to the database");
            //}
        }
        public async Task NotifyEmployeesForNewSessionAsync(
            string restaurantName,
            string? notes,
            string? excludeUserId = null)
        {
            var employees = await _userManager.GetUsersInRoleAsync(UserRoles.Employee);

            var tasks = employees
                .Where(e =>
                    !string.IsNullOrEmpty(e.Email) &&
                    (excludeUserId == null || e.Id != excludeUserId))
                .Select(async employee =>
                {
                    var subject = "New Food Session Started!";

                    var body =
                        $"Hello {employee.UserName},\n\n" +
                        $"A new food session has been started at {restaurantName}.\n" +
                        $"Session Notes: {notes ?? "No notes provided"}\n\n" +
                        $"Join the session and place your order before it closes!\n\n" +
                        "Bon appétit!";

                    await SendEmailAsync(
                        employee.Email!,
                        subject,
                        body,
                        employee.Id);
                });

            await Task.WhenAll(tasks);
        }
    }
}
