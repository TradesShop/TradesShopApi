using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TradePlatform.Api.Models;

namespace TradePlatform.Api.Services.Emails;

public class EmailSender : IEmailSender
{
	private readonly EmailSettings _settings;

	public EmailSender(IOptions<EmailSettings> settings)
	{
		_settings = settings.Value;
	}

	public async Task SendAsync(string to, string subject, string htmlBody)
	{
		using SmtpClient client = new SmtpClient(_settings.Host)
		{
			Port = _settings.Port,
			Credentials = new NetworkCredential(_settings.Username, _settings.Password),
			EnableSsl = true
		};
		MailMessage mail = new MailMessage(_settings.From, to, subject, htmlBody);
		mail.IsBodyHtml = true;
		await client.SendMailAsync(mail);
	}
}
