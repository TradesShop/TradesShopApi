using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Google.Cloud.Vision.V1;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using Stripe;
using TradePlatform.Api.Data;
using TradePlatform.Api.Filters;
using TradePlatform.Api.Identity;
using TradePlatform.Api.Infrastructure.Serialization;
using TradePlatform.Api.Middleware;
using TradePlatform.Api.Models;
using TradePlatform.Api.Models.MagicPayLoad;
using TradePlatform.Api.Repositories;
using TradePlatform.Api.Repositories.AnswerGroups;
using TradePlatform.Api.Repositories.Answers;
using TradePlatform.Api.Repositories.Implementations;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Repositories.PublicApi;
using TradePlatform.Api.Repositories.Questions;
using TradePlatform.Api.Services;
using TradePlatform.Api.Services.Admin;
using TradePlatform.Api.Services.AESHelper;
using TradePlatform.Api.Services.BackgroundEmailQueue;
using TradePlatform.Api.Services.Bundles;
using TradePlatform.Api.Services.Business;
using TradePlatform.Api.Services.Categories;
using TradePlatform.Api.Services.Chat;
using TradePlatform.Api.Services.Cookies;
using TradePlatform.Api.Services.Credits;
using TradePlatform.Api.Services.Emails;
using TradePlatform.Api.Services.Files;
using TradePlatform.Api.Services.Google_OCR;
using TradePlatform.Api.Services.Jobs;
using TradePlatform.Api.Services.MagicUrl;
using TradePlatform.Api.Services.MasterData;
using TradePlatform.Api.Services.Payments;
using TradePlatform.Api.Services.plans;
using TradePlatform.Api.Services.Postcode;
using TradePlatform.Api.Services.PublicApi;
using TradePlatform.Api.Services.Questions;
using TradePlatform.Api.Services.Reviews;
using TradePlatform.Api.Services.stripe;
using TradePlatform.Api.Services.Subscriptions;
using TradePlatform.Api.Services.Telemetry;
using TradePlatform.Api.Services.users;

[CompilerGenerated]
internal class Program
{
	private static void Main(string[] args)
	{
		string logPath = Path.Combine(AppContext.BaseDirectory, "Logs", "log-.txt");
		Log.Logger = new LoggerConfiguration().MinimumLevel.Information().Enrich.FromLogContext().WriteTo.Console().WriteTo.File(logPath, LogEventLevel.Verbose, "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}", null, retainedFileCountLimit: 12, fileSizeLimitBytes: 1073741824L, levelSwitch: null, buffered: false, shared: true, flushToDiskInterval: null, rollingInterval: RollingInterval.Day).CreateLogger();
		WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
		builder.Host.UseSerilog();
		builder.Configuration.AddEnvironmentVariables().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).AddJsonFile("appsettings." + builder.Environment.EnvironmentName + ".json", optional: true);
		builder.Services.AddDbContext<ApplicationDbContext>((DbContextOptionsBuilder options) =>
		{
			options.UseSqlServer(builder.Configuration.GetConnectionString("_devConnection") ?? throw new InvalidOperationException("Connection string '_devConnection' not found."));
		});
		builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
		builder.Services.AddSingleton<DapperContext>();
		builder.Services.AddScoped<ICookieService, CookieService>();
		builder.Services.AddScoped<IAnswerRepository, AnswerRepository>();
		builder.Services.AddScoped<IAnswerGroupRepository, AnswerGroupRepository>();
		builder.Services.AddScoped<IQuestionsRepository, QuestionsRepository>();
		builder.Services.AddScoped<IFileRepository, FileRepository>();
		builder.Services.AddScoped<ITdsFileService, TdsFileService>();
		builder.Services.AddScoped<IReviewsRepository, ReviewsRepository>();
		builder.Services.AddScoped<IReviewsService, ReviewsService>();
		builder.Services.AddScoped<IPublicApiRepository, PublicApiRepository>();
		builder.Services.AddScoped<IPublicApiService, PublicApiService>();
		builder.Services.AddScoped<IUsersRepository, UsersRepository>();
		builder.Services.AddScoped<IUserAddressRepository, UserAddressRepository>();
		builder.Services.AddScoped<ITradespersonsRepository, TradespersonsRepository>();
		builder.Services.AddScoped<ITradesRepository, TradesRepository>();
		builder.Services.AddScoped<IBusinessProfileRepository, BusinessProfileRepository>();
		builder.Services.AddScoped<IPlansRepository, PlansRepository>();
		builder.Services.AddScoped<PlansService>();
		builder.Services.AddScoped<IPaymentsRepository, PaymentsRepository>();
		builder.Services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
		builder.Services.AddScoped<ITdsInvoiceRepository, TdsInvoiceRepository>();
		builder.Services.AddScoped<ITdsInvoiceService, TdsInvoiceService>();
		builder.Services.AddScoped<IInvoiceItemsRepository, InvoiceItemsRepository>();
		builder.Services.AddScoped<IStripeEventsRepository, StripeEventsRepository>();
		builder.Services.AddScoped<IPaymentTshIntentService, PaymentTshIntentService>();
		builder.Services.AddScoped<ICreditRepository, CreditRepository>();
		builder.Services.AddScoped<ICreditService, CreditService>();
		builder.Services.AddScoped<IRefundServices, RefundServices>();
		builder.Services.AddScoped<IRefundRepository, RefundRepository>();
		builder.Services.AddScoped<IBillingServices, BillingServices>();
		builder.Services.AddScoped<ISubscriptionsRepository, SubscriptionsRepository>();
		builder.Services.AddScoped<IUserSubscriptionService, UserSubscriptionService>();
		builder.Services.AddScoped<ISubscriptionHistoryRepository, SubscriptionHistoryRepository>();
		builder.Services.AddScoped<IEmailVerificationRepository, EmailVerificationRepository>();
		builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
		builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
		builder.Services.AddScoped<IEmailService, SmtpEmailService>();
		builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
		builder.Services.AddScoped<ICategoryService, CategoryService>();
		builder.Services.AddScoped<subcategoryRepository>();
		builder.Services.AddScoped<QuestionRepository>();
		builder.Services.AddScoped<IQuestionsService, QuestionsService>();
		builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
		builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
		builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
		builder.Services.AddScoped<IAuthService, AuthService>();
		builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
		builder.Services.AddScoped<PasswordHashingService>();
		builder.Services.AddHttpContextAccessor();
		builder.Services.AddSingleton<IUserTimezoneResolver, UserTimezoneResolver>();
		builder.Services.AddScoped<IIdentityService, TradePlatform.Api.Services.IdentityService>();
		builder.Services.AddScoped<IBundlesCreditRepository, BundlesCreditRepository>();
		builder.Services.AddScoped<IBundlePurchaseService, BundlePurchaseService>();
		builder.Services.AddScoped<IBundlePricesRepository, BundlePricesRepository>();
		builder.Services.AddScoped<IBundleOrdersRepository, BundleOrdersRepository>();
		builder.Services.AddScoped<IBundleAdminService, BundleAdminService>();
		builder.Services.AddScoped<IJobsRepository, JobsRepository>();
		builder.Services.AddScoped<IJobsService, JobsService>();
		builder.Services.AddScoped<IUsersService, UsersService>();
		builder.Services.AddScoped<IBusinessService, BusinessService>();
		builder.Services.AddScoped<IBusinessRepository, BusinessRepository>();
		builder.Services.AddScoped<IChatRepository, ChatRepository>();
		builder.Services.AddScoped<IChatService, ChatService>();
		builder.Services.AddSingleton((IServiceProvider sp) => ImageAnnotatorClient.Create());
		builder.Services.AddScoped<DocumentTypeService>();
		builder.Services.AddScoped<OcrService>();
		builder.Services.AddScoped<GoogleVisionService>();
		builder.Services.AddScoped<MrzParserService>();
		builder.Services.AddScoped<UnifiedDocumentParserService>();
		builder.Services.AddScoped<VerificationRepository>();
		builder.Services.AddScoped<ITelemetryErrorsRepository, TelemetryErrorsRepository>();
		builder.Services.AddScoped<ITelemetryErrorsService, TelemetryErrorsService>();
		builder.Services.AddSingleton<IAesCryptoService, AesCryptoService>();
		builder.Services.AddScoped<IMagicUrlService, MagicUrlService>();
		builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
		builder.Services.AddScoped<IEmailSender, EmailSender>();
		builder.Services.AddSingleton<IBackgroundEmailQueue, BackgroundEmailQueue>();
		builder.Services.AddHostedService<EmailBackgroundService>();
		builder.Services.AddScoped<IMasterDataService, MasterDataService>();
		builder.Services.AddScoped<IMasterDataRepository, MasterDataRepository>();
		builder.Services.AddScoped<IAdminJobsService, AdminJobsService>();
		builder.Services.AddScoped<IAdminJobsRepository, AdminJobsRepository>();
		builder.Services.AddEndpointsApiExplorer();
		builder.Services.Configure<MagicLoginSettings>(builder.Configuration.GetSection("MagicLogin"));
		builder.Services.AddSingleton((IServiceProvider sp) =>
		{
			IConfiguration requiredService = sp.GetRequiredService<IConfiguration>();
			string text = requiredService["Stripe:SecretKey"];
			Console.WriteLine("\ud83d\udd25 Stripe key loaded at runtime: " + text);
			return new StripeClient(text);
		});
		builder.Services.AddScoped<IStripeService, StripeService>();
		builder.Services.AddScoped<IStripeCustomerService, StripeCustomerService>();
		builder.Services.AddScoped<IBillingServices, BillingServices>();
		builder.Services.AddScoped((IServiceProvider sp) =>
		{
			StripeClient requiredService = sp.GetRequiredService<StripeClient>();
			return new SubscriptionService(requiredService);
		});
		builder.Services.AddScoped((IServiceProvider sp) =>
		{
			StripeClient requiredService = sp.GetRequiredService<StripeClient>();
			return new InvoiceService(requiredService);
		});
		builder.Services.AddScoped((IServiceProvider sp) =>
		{
			StripeClient requiredService = sp.GetRequiredService<StripeClient>();
			return new PaymentIntentService(requiredService);
		});
		builder.Services.AddScoped<IStripeWebhookService, StripeWebhookService>();
		builder.Services.AddScoped<IStripeRefundService, StripeRefundService>();
		builder.Services.AddMemoryCache();
		builder.Services.AddHttpClient<IPostcodeLookupService, PostcodeLookupService>((HttpClient client) =>
		{
			client.Timeout = TimeSpan.FromSeconds(5.0);
			client.DefaultRequestHeaders.UserAgent.ParseAdd("MyTradesShopApp/1.0 (contact@mytradesshop.com)");
		});
		IConfigurationSection jwtSection = builder.Configuration.GetSection("Jwt");
		string jwtKey = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured.");
		string jwtIssuer = jwtSection["Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is not configured.");
		string jwtAudience = jwtSection["Audience"] ?? throw new InvalidOperationException("Jwt:Audience is not configured.");
		JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
		builder.Services.AddAuthentication((AuthenticationOptions options) =>
		{
			options.DefaultAuthenticateScheme = "Bearer";
			options.DefaultChallengeScheme = "Bearer";
		}).AddJwtBearer((JwtBearerOptions options) =>
		{
			options.RequireHttpsMetadata = false;
			options.SaveToken = true;
			options.Events = new JwtBearerEvents
			{
				OnMessageReceived = (MessageReceivedContext context) =>
				{
					if (string.IsNullOrEmpty(context.Token) && context.Request.Cookies.TryGetValue("auth_token", out string value))
					{
						context.Token = value.Trim().Trim('"');
					}
					Console.WriteLine("========== JWT ==========");
					Console.WriteLine("AUTH HEADER: " + context.Request.Headers["Authorization"]);
					Console.WriteLine("COOKIE TOKEN: " + context.Token);
					Console.WriteLine("=========================");
					return Task.CompletedTask;
				},
				OnTokenValidated = (TokenValidatedContext context) =>
				{
					Console.WriteLine("TOKEN VALIDATED");
					Console.WriteLine("UserId: " + context.Principal?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value);
					Console.WriteLine("Role: " + context.Principal?.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value);
					return Task.CompletedTask;
				},
				OnAuthenticationFailed = (AuthenticationFailedContext context) =>
				{
					Console.WriteLine("TOKEN FAILED");
					Console.WriteLine(context.Exception);
					return Task.CompletedTask;
				}
			};
			options.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuer = true,
				ValidateAudience = true,
				ValidateLifetime = true,
				ValidateIssuerSigningKey = true,
				ValidIssuer = jwtIssuer,
				ValidAudience = jwtAudience,
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
				ClockSkew = TimeSpan.FromMinutes(2.0),
				NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
				RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
			};
		});
		builder.Services.AddAuthorization();
		builder.Services.AddCors((CorsOptions options) =>
		{
			options.AddPolicy("FrontendCors", (CorsPolicyBuilder policy) =>
			{
				string[] origins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? new string[5] { "http://localhost:3000", "https://stage.tradesshop.co.uk", "https://tradesshop.co.uk", "https://stage.mytradesshop.com", "https://mytradesshop.com" };
				policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod()
					.AllowCredentials();
			});
		});
		builder.Services.AddScoped<IUserTimezoneResolver, UserTimezoneResolver>();
		builder.Services.AddControllers((MvcOptions options) =>
		{
			options.Filters.Add<ValidationFilter>();
			options.ModelBinderProviders.Insert(0, new SafeDateTimeBinderProvider());
		}).AddJsonOptions((JsonOptions options) =>
		{
			options.JsonSerializerOptions.Converters.Add(new UserTimezoneModelConverterFactory(new HttpContextAccessor()));
		});
		string credentialPath = builder.Configuration["GoogleVision:CredentialsPath"];
		Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialPath);
		IConfigurationSection aws = builder.Configuration.GetSection("AWS");
		builder.Services.AddSingleton((Func<IServiceProvider, IAmazonS3>)((IServiceProvider sp) =>
		{
			IConfiguration requiredService = sp.GetRequiredService<IConfiguration>();
			return new AmazonS3Client(requiredService["AWS:AccessKey"], requiredService["AWS:SecretKey"], RegionEndpoint.GetBySystemName(requiredService["AWS:Region"]));
		}));
		builder.Services.AddScoped<IAwsS3Service, AwsS3Service>();
		builder.Services.AddSwaggerGen();
		WebApplication app = builder.Build();
		app.UseSwagger();
		app.UseSwaggerUI();
		app.UseMiddleware<ErrorHandlingMiddleware>(Array.Empty<object>());
		app.UseRouting();
		app.UseSerilogRequestLogging();
		app.UseCors("FrontendCors");
		app.UseAuthentication();
		app.UseAuthorization();
		app.UseResponseCaching();
		ICollection<string> addresses = app.Urls;
		foreach (string addr in addresses)
		{
			Console.WriteLine("\ud83d\udd25 API is listening on: " + addr);
		}
		app.MapControllers();
		try
		{
			app.Run();
		}
		catch (Exception exception)
		{
			Log.Fatal(exception, "❌ Application terminated unexpectedly");
		}
		finally
		{
			Log.CloseAndFlush();
		}
	}
}
