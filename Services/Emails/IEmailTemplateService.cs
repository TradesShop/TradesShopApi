using System.Threading.Tasks;
using TradePlatform.Api.Services.Emails.Models;

namespace TradePlatform.Api.Services.Emails;

public interface IEmailTemplateService
{
	Task<string> RenderAsync(string templateName, EmailModelBase model);
}
