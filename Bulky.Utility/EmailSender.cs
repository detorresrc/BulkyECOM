using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Bulky.Utility
{
    public class EmailSender : IEmailSender
    {
        public string? SendGridSecret { get; set; }
        private readonly ILogger<EmailSender> _logger;
        public EmailSender(ILogger<EmailSender> logger, IConfiguration config)
        {
            _logger = logger;
            SendGridSecret = config.GetValue<string>("SendGrid:SecretKey");
        }
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            _logger.LogInformation($"Email sent to {email} with subject {subject} and message {htmlMessage}");

            var client = new SendGridClient(SendGridSecret);

            var from = new EmailAddress("noreply@rommeldetorres.me", "Bulky Book");
            var to = new EmailAddress(email);
            var message = MailHelper.CreateSingleEmail(from, to, subject, htmlMessage, htmlMessage);

            return client.SendEmailAsync(message);
        }
    }
}
