using System.Threading.Tasks;

namespace TradePlatform.Api.Services.Emails;

public interface IEmailSender
{
	Task SendAsync(string to, string subject, string htmlBody);
}
