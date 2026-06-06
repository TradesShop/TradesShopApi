using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;


namespace TradePlatform.Api.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public SmtpEmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            using var client = new SmtpClient(_settings.Host)
            {
                Port = _settings.Port,
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                EnableSsl = true
            };

            var mail = new MailMessage(_settings.From, to, subject, body);
            mail.IsBodyHtml = true;

            await client.SendMailAsync(mail);
        }
    }
}
