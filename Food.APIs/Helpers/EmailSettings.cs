using System.Net;
using System.Net.Mail;
using Food.Domain.Models;

namespace Food.APIs.Helpers
{
    public static class EmailSettings
    {
        public static void SendEmail(IConfiguration configuration,Email email)
        {
            var smtpSettings = configuration.GetSection("SmtpSettings");
            var Client = new SmtpClient(smtpSettings["Host"], int.Parse(smtpSettings["Port"]))
            {
                Credentials = new NetworkCredential(smtpSettings["Username"], smtpSettings["Password"]),
                EnableSsl = true
            };
            Client.Send(smtpSettings["From"], email.To, email.Subject, email.Body);
        }
    }
}
