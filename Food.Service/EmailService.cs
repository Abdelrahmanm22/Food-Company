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
        private readonly IEmailTemplateService _templateService;

        // Delay between individual email jobs (seconds) — keeps within Mailtrap free tier rate limit
        private const int EmailIntervalSeconds = 60;

        public EmailService(
            IConfiguration configuration,
            IUnitOfWork unitOfWork,
            ILogger<EmailService> logger,
            UserManager<AppUser> userManager,
            IBackgroundJobClient backgroundJobClient,
            IEmailTemplateService templateService)
        {
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _userManager = userManager;
            _backgroundJobClient = backgroundJobClient;
            _templateService = templateService;
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
                    using var smtpClient = new SmtpClient(host, int.Parse(portStr))
                    {
                        Credentials = new NetworkCredential(username, password),
                        EnableSsl = true
                    };

                    using var mailMessage = new MailMessage
                    {
                        From = new MailAddress(from!),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true,   // send as HTML
                        BodyEncoding = Encoding.UTF8
                    };
                    mailMessage.To.Add(to);

                    await smtpClient.SendMailAsync(mailMessage);

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
            var logoUrl = BuildLogoUrl();
            int delaySeconds = 2;

            foreach (var employee in employees)
            {
                if (string.IsNullOrEmpty(employee.Email)) continue;
                if (excludeUserId != null && employee.Id == excludeUserId) continue;

                var subject = "New Food Session Started! 🍽️";

                var template = _templateService.LoadTemplate("NewSessionEmail.html");
                var body = _templateService.PopulateTemplate(template, new Dictionary<string, string>
                {
                    ["UserName"]       = employee.UserName ?? "there",
                    ["RestaurantName"] = restaurantName,
                    ["SessionNotes"]   = string.IsNullOrWhiteSpace(notes) ? "No notes provided." : notes,
                    ["LogoUrl"]        = logoUrl,
                    ["Year"]           = DateTime.UtcNow.Year.ToString()
                });

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

            var logoUrl = BuildLogoUrl();
            int delaySeconds = 0;

            foreach (var participant in session.SessionJoins)
            {
                if (participant.User == null || string.IsNullOrEmpty(participant.User.Email)) continue;

                var subject = "Food Session Cancelled ❌";

                var template = _templateService.LoadTemplate("SessionCancelledEmail.html");
                var body = _templateService.PopulateTemplate(template, new Dictionary<string, string>
                {
                    ["UserName"]       = participant.User.UserName ?? "there",
                    ["RestaurantName"] = restaurantName,
                    ["HostName"]       = session.HostUser?.UserName ?? "the host",
                    ["LogoUrl"]        = logoUrl,
                    ["Year"]           = DateTime.UtcNow.Year.ToString()
                });

                var recipientEmail = participant.User.Email;
                var recipientId = participant.UserId;

                _backgroundJobClient.Schedule<IEmailService>(
                    service => service.SendEmailAsync(recipientEmail, subject, body, recipientId),
                    TimeSpan.FromSeconds(delaySeconds));

                delaySeconds += EmailIntervalSeconds;
            }
        }

        /// <summary>
        /// Orchestrator job — builds a personalized HTML email per participant (with their items,
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

            var logoUrl = BuildLogoUrl();
            int delaySeconds = 0;

            foreach (var participant in order.Session.SessionJoins)
            {
                if (participant.User == null || string.IsNullOrEmpty(participant.User.Email)) continue;

                detailsByUser.TryGetValue(participant.UserId, out var userDetails);
                var itemsSubtotal = userDetails?.Sum(d => d.Price * d.Quantity) ?? 0;
                var grandTotal = itemsSubtotal + deliveryCostPerPerson;

                // Build HTML table rows for items
                var itemRowsHtml = userDetails != null && userDetails.Count > 0
                    ? string.Join("\n", userDetails.Select(d =>
                        $"<tr>" +
                        $"<td style=\"padding:12px 14px;\"><span style=\"font-weight:600;color:#222222;\">{HtmlEncode(d.Item?.Name ?? "Item")}</span></td>" +
                        $"<td style=\"padding:12px 14px;\">{d.Quantity}</td>" +
                        $"<td style=\"padding:12px 14px;\">{d.Price:C}</td>" +
                        $"<td style=\"padding:12px 14px;text-align:right;font-weight:600;\">{d.Price * d.Quantity:C}</td>" +
                        $"</tr>"))
                    : "<tr><td colspan=\"4\" style=\"padding:12px 14px;text-align:center;color:#888888;font-style:italic;\">No items recorded.</td></tr>";

                var subject = $"Your Order from {order.Session.Restaurant.Name} is Confirmed! ✅";

                var template = _templateService.LoadTemplate("OrderConfirmedEmail.html");
                var body = _templateService.PopulateTemplate(template, new Dictionary<string, string>
                {
                    ["UserName"]       = participant.User.UserName ?? "there",
                    ["RestaurantName"] = order.Session.Restaurant.Name,
                    ["ItemRows"]       = itemRowsHtml,
                    ["ItemsSubtotal"]  = itemsSubtotal.ToString("C"),
                    ["DeliveryShare"]  = deliveryCostPerPerson.ToString("C"),
                    ["GrandTotal"]     = grandTotal.ToString("C"),
                    ["HostName"]       = order.Session.HostUser?.UserName ?? "the host",
                    ["LogoUrl"]        = logoUrl,
                    ["Year"]           = DateTime.UtcNow.Year.ToString()
                });

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

            var logoUrl = BuildLogoUrl();
            int delaySeconds = 0;

            foreach (var participant in order.Session.SessionJoins)
            {
                if (participant.User == null || string.IsNullOrEmpty(participant.User.Email)) continue;

                var subject = $"Your food from {order.Session.Restaurant.Name} has arrived! 🛵";

                var template = _templateService.LoadTemplate("OrderDeliveredEmail.html");
                var body = _templateService.PopulateTemplate(template, new Dictionary<string, string>
                {
                    ["UserName"]       = participant.User.UserName ?? "there",
                    ["RestaurantName"] = order.Session.Restaurant.Name,
                    ["HostName"]       = order.Session.HostUser?.UserName ?? "the host",
                    ["LogoUrl"]        = logoUrl,
                    ["Year"]           = DateTime.UtcNow.Year.ToString()
                });

                var recipientEmail = participant.User.Email;
                var recipientId = participant.UserId;

                _backgroundJobClient.Schedule<IEmailService>(
                    service => service.SendEmailAsync(recipientEmail, subject, body, recipientId),
                    TimeSpan.FromSeconds(delaySeconds));

                delaySeconds += EmailIntervalSeconds;
            }
        }

        // ──────────────────────────────────────────────────────────────
        // Private helpers
        // ──────────────────────────────────────────────────────────────

        /// <summary>Builds the absolute URL for the company logo using ApiBaseURL from configuration.</summary>
        private string BuildLogoUrl()
        {
            var baseUrl = _configuration["ApiBaseURL"]?.TrimEnd('/') ?? string.Empty;
            return $"{baseUrl}/images/pixelsoft-logo.png";
        }

        /// <summary>Minimal HTML encoding to safely embed user-supplied strings inside HTML.</summary>
        private static string HtmlEncode(string value)
            => System.Net.WebUtility.HtmlEncode(value);
    }
}
