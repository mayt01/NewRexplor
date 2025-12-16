using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace Rexplor.Models
{
    public class EmailService : Microsoft.AspNetCore.Identity.UI.Services.IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings");

            using (var client = new SmtpClient(smtpSettings["Host"], int.Parse(smtpSettings["Port"])))
            {
                client.Credentials = new NetworkCredential(smtpSettings["Username"], smtpSettings["Password"]);
                client.EnableSsl = bool.Parse(smtpSettings["EnableSSL"]);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpSettings["Username"]),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
            }
        }
    }
}














////using Microsoft.AspNetCore.Identity.UI.Services;
////using Microsoft.Extensions.Options;
////using System.Net;
////using System.Net.Mail;
////using System.Threading.Tasks;
////using Rexplor.Models;

//////ddddd
////namespace Rexplor.Models
////{
////    public class EmailService : IEmailSender
////    {
////        private readonly SmtpSettings _smtpSettings;

////        public EmailService(IOptions<SmtpSettings> smtpSettings)
////        {
////            _smtpSettings = smtpSettings.Value;
////        }

////        public Task SendEmailAsync(string email, string subject, string htmlMessage)
////        {
////            using var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
////            {
////                Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password),
////                EnableSsl = _smtpSettings.EnableSSL
////            };

////            var mailMessage = new MailMessage
////            {
////                From = new MailAddress(_smtpSettings.Username),
////                Subject = subject,
////                Body = htmlMessage,
////                IsBodyHtml = true
////            };
////            mailMessage.To.Add(email);

////            return client.SendMailAsync(mailMessage);
////        }
////    }
////}
