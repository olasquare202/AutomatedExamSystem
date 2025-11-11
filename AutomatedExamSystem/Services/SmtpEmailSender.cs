using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;

namespace AutomatedExamSystem.Services
{

    
namespace AutomatedExamSystem.Services
    {
        public class SmtpEmailSender : IEmailSender
        {
            private readonly SmtpSettings _settings;
            private readonly ILogger<SmtpEmailSender> _logger;

            public SmtpEmailSender(IOptions<SmtpSettings> options, ILogger<SmtpEmailSender> logger)
            {
                _settings = options.Value;
                _logger = logger;
            }

            public async Task SendEmailAsync(string toEmail, string subject, string body)
            {
                try
                {
                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
                    message.To.Add(MailboxAddress.Parse(toEmail));
                    message.Subject = subject;
                    message.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };

                    using var client = new SmtpClient();
                    await client.ConnectAsync(_settings.Host, _settings.Port, _settings.EnableSsl);
                    await client.AuthenticateAsync(_settings.UserName, _settings.Password);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);

                    _logger.LogInformation($"✅ Email sent successfully to {toEmail}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"❌ Failed to send email to {toEmail}");
                    throw;
                }
            }
        }
    }

}
    

