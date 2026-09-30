using System.IO;
using System.Threading.Tasks;
using HandlebarsDotNet;
using Microsoft.AspNetCore.Hosting;
using TradePlatform.Api.Services.Emails.Models;

namespace TradePlatform.Api.Services.Emails;

public class EmailTemplateService : IEmailTemplateService
{
	private readonly string _templatePath;

	public EmailTemplateService(IWebHostEnvironment env)
	{
		_templatePath = Path.Combine(env.ContentRootPath, "EmailTemplates");
	}

	public async Task<string> RenderAsync(string templateName, EmailModelBase model)
	{
		string baseLayout = await File.ReadAllTextAsync(Path.Combine(_templatePath, "BaseLayout.html"));
		HandlebarsTemplate<object, object> template = Handlebars.Compile(await File.ReadAllTextAsync(Path.Combine(_templatePath, templateName)));
		model.Content = template(model);
		HandlebarsTemplate<object, object> layoutTemplate = Handlebars.Compile(baseLayout);
		return layoutTemplate(model);
	}
}
