using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Rexplor.Models.Email_Service
{
    public class MockEmailSender : IEmailSender
    {
        private readonly ILogger<MockEmailSender> _logger;

        public MockEmailSender(ILogger<MockEmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // در حالت توسعه فقط لاگ می‌کنیم
            _logger.LogInformation($"Email to {email}: {subject}\n{htmlMessage}");
            return Task.CompletedTask;
        }
    }
}
