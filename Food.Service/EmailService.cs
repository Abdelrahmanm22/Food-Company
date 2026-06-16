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
using Food.Domain.Specifications.OrderSpec;
using Hangfire;
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
        private readonly IBackgroundJobClient _backgroundJobClient;

        // Delay between individual email jobs (seconds) — keeps within Mailtrap free tier rate limit
        private const int EmailIntervalSeconds = 60;

        public EmailService(
            IConfiguration configuration,
            IUnitOfWork unitOfWork,
            ILogger<EmailService> logger,
            UserManager<AppUser> userManager,
            IBackgroundJobClient backgroundJobClient)
        {
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _userManager = userManager;
            _backgroundJobClient = backgroundJobClient;
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

                if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(portStr))
                {
                    using (var client = new SmtpClient(host, int.Parse(portStr))
                    {
                        Credentials = new NetworkCredential(username, password),
                        EnableSsl = true
                    })
                    {
                        await client.SendMailAsync(from, to, subject, body);
                    }
                    email.IsSent = true;
                    _logger.LogInformation("Email sent successfully to {To} with subject {Subject}", to, subject);
                }
                else
                {
                    email.IsSent = false;
                    email.ErrorMessage = "SMTP settings are not fully configured.";
                    _logger.LogWarning("SMTP Settings not fully configured. Email was not sent.");
                }
            }
            catch (Exception ex)
            {
                email.IsSent = false;
                email.ErrorMessage = ex.Message;
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

        /// <summary>
        /// Orchestrator job — queries all employees then schedules one SendEmailAsync
        /// job per employee with a staggered delay to stay within Mailtrap rate limits.
        /// </summary>
        public async Task NotifyEmployeesForNewSessionAsync(string restaurantName, string? notes, string? excludeUserId = null)
        {
            var employees = await _userManager.GetUsersInRoleAsync(UserRoles.Employee);
            int delaySeconds = 2;

            foreach (var employee in employees)
            {
                if (string.IsNullOrEmpty(employee.Email)) continue;
                if (excludeUserId != null && employee.Id == excludeUserId) continue;

                var subject = "New Food Session Started!";
                var body = $"Hello {employee.UserName},\n\n" +
                           $"A new food session has been started at {restaurantName}.\n" +
                           $"Session Notes: {notes ?? "No notes provided"}\n\n" +
                           $"Join the session and place your order before it closes!\n\n" +
                           "Bon appétit!";

                // Capture loop variables for the closure
                var recipientEmail = employee.Email;
                var recipientId = employee.Id;

                _backgroundJobClient.Schedule<IEmailService>(
                    service => service.SendEmailAsync(recipientEmail, subject, body, recipientId),
                    TimeSpan.FromSeconds(delaySeconds));

                delaySeconds += EmailIntervalSeconds;
            }
        }

        /// <summary>
        /// Orchestrator job — queries all session participants then schedules one SendEmailAsync
        /// job per participant with a staggered delay to stay within Mailtrap rate limits.
        /// </summary>
        public async Task NotifyParticipantsSessionCancelledAsync(int sessionId, string restaurantName)
        {
            var sessionSpec = new SessionWithDetailsSpec(sessionId);
            var session = await _unitOfWork.Repository<Session>().GetByIdAsync(sessionSpec);
            if (session == null) return;

            int delaySeconds = 0;

            foreach (var participant in session.SessionJoins)
            {
                if (participant.User == null || string.IsNullOrEmpty(participant.User.Email)) continue;

                var subject = "Food Session Cancelled";
                var body = $"Hello {participant.User.UserName},\n\n" +
                           $"The Food session for '{restaurantName}' has been cancelled by the host ({session.HostUser.UserName}).\n" +
                           $"Any items in your cart for this session have been cleared.\n\n" +
                           $"Best regards.";

                // Capture loop variables for the closure
                var recipientEmail = participant.User.Email;
                var recipientId = participant.UserId;

                _backgroundJobClient.Schedule<IEmailService>(
                    service => service.SendEmailAsync(recipientEmail, subject, body, recipientId),
                    TimeSpan.FromSeconds(delaySeconds));

                delaySeconds += EmailIntervalSeconds;
            }
        }

        /// <summary>
        /// Orchestrator job — builds a personalized email per participant (with their items,
        /// subtotal, delivery share) then schedules one SendEmailAsync job per participant
        /// with a staggered delay to stay within Mailtrap rate limits.
        /// </summary>
        public async Task NotifyOrderConfirmedAsync(int orderId)
        {
            var orderSpec = new OrderWithDetailsSpec(orderId);
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(orderSpec);
            if (order == null) return;

            var participantCount = order.Session.SessionJoins.Count;
            var deliveryCostPerPerson = participantCount > 0
                ? order.DeliveryCost / participantCount
                : order.DeliveryCost;

            // Group order details by participant
            var detailsByUser = order.OrderDetails
                .GroupBy(od => od.UserId)
                .ToDictionary(g => g.Key, g => g.ToList());

            int delaySeconds = 0;

            foreach (var participant in order.Session.SessionJoins)
            {
                if (participant.User == null || string.IsNullOrEmpty(participant.User.Email)) continue;

                detailsByUser.TryGetValue(participant.UserId, out var userDetails);
                var itemsSubtotal = userDetails?.Sum(d => d.Price * d.Quantity) ?? 0;
                var grandTotal = itemsSubtotal + deliveryCostPerPerson;

                var itemLines = userDetails != null
                    ? string.Join("\n", userDetails.Select(d =>
                        $"  - {d.Item?.Name ?? "Item"} x{d.Quantity} @ {d.Price:C} = {d.Price * d.Quantity:C}"))
                    : "  (no items recorded)";

                var subject = $"Your Order from {order.Session.Restaurant.Name} is Confirmed!";
                var body =
                    $"Hello {participant.User.UserName},\n\n" +
                    $"Great news! The order for '{order.Session.Restaurant.Name}' has been confirmed.\n\n" +
                    $"Your Items:\n{itemLines}\n\n" +
                    $"Items Subtotal:       {itemsSubtotal:C}\n" +
                    $"Your Delivery Share:  {deliveryCostPerPerson:C}\n" +
                    $"Your Total:           {grandTotal:C}\n\n" +
                    $"The host ({order.Session.HostUser?.UserName}) will collect the full payment.\n\n" +
                    "Bon appétit!";

                // Capture loop variables for the closure
                var recipientEmail = participant.User.Email;
                var recipientId = participant.UserId;

                _backgroundJobClient.Schedule<IEmailService>(
                    service => service.SendEmailAsync(recipientEmail, subject, body, recipientId),
                    TimeSpan.FromSeconds(delaySeconds));

                delaySeconds += EmailIntervalSeconds;
            }
        }

        /// <summary>
        /// Orchestrator job — schedules one SendEmailAsync job per participant with a staggered
        /// delay to stay within Mailtrap rate limits.
        /// </summary>
        public async Task NotifyOrderDeliveredAsync(int orderId)
        {
            var orderSpec = new OrderWithDetailsSpec(orderId);
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(orderSpec);
            if (order == null) return;

            int delaySeconds = 0;

            foreach (var participant in order.Session.SessionJoins)
            {
                if (participant.User == null || string.IsNullOrEmpty(participant.User.Email)) continue;

                var subject = $"Your food from {order.Session.Restaurant.Name} has arrived!";
                var body =
                    $"Hello {participant.User.UserName},\n\n" +
                    $"Your order from '{order.Session.Restaurant.Name}' has been delivered!\n\n" +
                    $"Please contact the host ({order.Session.HostUser?.UserName}) " +
                    $"or come to collect your food.\n\n" +
                    "Enjoy your meal!";

                // Capture loop variables for the closure
                var recipientEmail = participant.User.Email;
                var recipientId = participant.UserId;

                _backgroundJobClient.Schedule<IEmailService>(
                    service => service.SendEmailAsync(recipientEmail, subject, body, recipientId),
                    TimeSpan.FromSeconds(delaySeconds));

                delaySeconds += EmailIntervalSeconds;
            }
        }
    }
}
