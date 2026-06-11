using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens;
using Stripe;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TradePlatform.Api.Data;
using TradePlatform.Api.DTOs.Invoices;
using TradePlatform.Api.Filters;
using TradePlatform.Api.Identity;
using TradePlatform.Api.Middleware;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories;
using TradePlatform.Api.Repositories.Implementations;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services;
using TradePlatform.Api.Services.Azure_OCR;
using TradePlatform.Api.Services.Bundles;
using TradePlatform.Api.Services.Business;
using TradePlatform.Api.Services.Categories;
using TradePlatform.Api.Services.Chat;
using TradePlatform.Api.Services.Credits;
using TradePlatform.Api.Services.Files;
using TradePlatform.Api.Services.Jobs;
using TradePlatform.Api.Services.Payments;
using TradePlatform.Api.Services.Questions;
using TradePlatform.Api.Services.Reviews;
using TradePlatform.Api.Services.Subscriptions;
using TradePlatform.Api.Services.users;


var builder = WebApplication.CreateBuilder(args);

// ---------- Database ----------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("_devConnection")
        ?? throw new InvalidOperationException("Connection string '_devConnection' not found.")
    ));

// ---------- Identity ----------
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// ---------- DI / Repositories ----------
//builder.Services.AddSingleton<AzureBlobService>();
builder.Services.AddSingleton<DapperContext>();
//builder.Services.AddSingleton<DapperContext>();

builder.Services.AddScoped<IFileRepository, FileRepository>();
builder.Services.AddScoped<ITdsFileService, TdsFileService>();

builder.Services.AddScoped<IReviewsRepository, ReviewsRepository>();
builder.Services.AddScoped<IReviewsService, ReviewsService>();

builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<IUserAddressRepository, UserAddressRepository>();
builder.Services.AddScoped<ITradespersonsRepository, TradespersonsRepository>();
builder.Services.AddScoped<ITradesRepository, TradesRepository>();
builder.Services.AddScoped<IBusinessProfileRepository, BusinessProfileRepository>();
builder.Services.AddScoped<IPlansRepository, PlansRepository>();
builder.Services.AddScoped<PlansService>();
//builder.Services.AddScoped<IBillingRepository, BillingRepository>();
builder.Services.AddScoped<IPaymentsRepository, PaymentsRepository>();
builder.Services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
builder.Services.AddScoped<IInvoicesTshRepository, InvoicesTshRepository>();
builder.Services.AddScoped<IInvoiceItemsRepository, InvoiceItemsRepository>();
builder.Services.AddScoped<IStripeEventsRepository, StripeEventsRepository>();
builder.Services.AddScoped<IPaymentTshIntentService, PaymentTshIntentService>();

builder.Services.AddScoped<ICreditRepository, CreditRepository>();
builder.Services.AddScoped<ICreditService, CreditService>();

builder.Services.AddScoped<IRefundServices, RefundServices>();
builder.Services.AddScoped<IBillingServices, TradePlatform.Api.Services.BillingServices>();

builder.Services.AddScoped<ISubscriptionsRepository, SubscriptionsRepository>();
builder.Services.AddScoped<IUserSubscriptionService, UserSubscriptionService>();



builder.Services.AddScoped<ISubscriptionHistoryRepository, SubscriptionHistoryRepository>();
builder.Services.AddScoped<IEmailVerificationRepository, EmailVerificationRepository>();

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings")
);
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

builder.Services.AddScoped<ICategoryRepository,CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

builder.Services.AddScoped<subcategoryRepository>();
builder.Services.AddScoped<QuestionRepository>();
builder.Services.AddScoped<IQuestionsService,QuestionsService>();

builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<PasswordHashingService>();

builder.Services.AddHttpContextAccessor();
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

builder.Services.AddScoped<AzureVisionService>();
builder.Services.AddScoped<DocumentTypeService>();
builder.Services.AddScoped<MrzParserService>();
builder.Services.AddScoped<UnifiedDocumentParserService>();
builder.Services.AddScoped<VerificationRepository>();


// ---------- Stripe ----------
builder.Services.AddSingleton<StripeClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var secretKey = config["Stripe:SecretKey"];
    return new StripeClient(secretKey);
});
Console.WriteLine("STRIPE KEY FROM PROGRAM.CS: " + builder.Configuration["Stripe:SecretKey"]);

builder.Services.AddScoped<IStripeService, StripeService>();
builder.Services.AddScoped<IStripeCustomerService, StripeCustomerService>();
builder.Services.AddScoped<IBillingServices, BillingServices>();
builder.Services.AddScoped<SubscriptionService>(sp =>
{
    var client = sp.GetRequiredService<StripeClient>();
    return new SubscriptionService(client);
});

builder.Services.AddScoped<Stripe.InvoiceService>(sp =>
{
    var client = sp.GetRequiredService<StripeClient>();
    return new Stripe.InvoiceService(client);
});

builder.Services.AddScoped<PaymentIntentService>(sp =>
{
    var client = sp.GetRequiredService<StripeClient>();
    return new PaymentIntentService(client);
});

builder.Services.AddScoped<IStripeWebhookService, StripeWebhookService>();
//builder.Services.AddScoped<IStripeCustomerService, StripeCustomerService>();
//services.AddScoped<IStripePaymentService, StripePaymentService>();
//services.AddScoped<IStripeSubscriptionService, StripeSubscriptionService>();
//services.AddScoped<IStripeInvoiceService, StripeInvoiceService>();


// ---------- Authentication / JWT ----------
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured.");
var jwtIssuer = jwtSection["Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is not configured.");
var jwtAudience = jwtSection["Audience"] ?? throw new InvalidOperationException("Jwt:Audience is not configured.");
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap["role"] = ClaimTypes.Role;
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] = ClaimTypes.Role;

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // local dev fix
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(2),
            NameClaimType = JwtRegisteredClaimNames.Sub,
            RoleClaimType = ClaimTypes.Role
        };
    });
        builder.Services.AddControllers(options =>
        {
            options.Filters.Add<ValidationFilter>();
        });
        // ---------- CORS ----------
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("FrontendCors", policy =>
            {
                var allowedOrigin = builder.Configuration["FrontendOrigin"] ?? "http://localhost:3000";

                policy.WithOrigins(allowedOrigin)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

// ---------- Controllers ----------
builder.Services.AddControllers();
var aws = builder.Configuration.GetSection("AWS");

// AWS SDK client
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonS3>();


builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();
// Register your S3 service
builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new AmazonS3Client(
        config["AWS:AccessKey"],
        config["AWS:SecretKey"],
        Amazon.RegionEndpoint.GetBySystemName(config["AWS:Region"])
    );
});
builder.Services.AddScoped<IAwsS3Service, AwsS3Service>();


var app = builder.Build();

// ---------- Middleware pipeline ----------
// ⭐ MUST be FIRST so it catches everything
app.UseMiddleware<ErrorHandlingMiddleware>();

// No DeveloperExceptionPage — it overrides your JSON middleware
// No UseExceptionHandler — also overrides your JSON middleware

//app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("FrontendCors");
//app.MapPost("/api/stripe/webhook", async context =>
//{
//    // Let MVC handle it
//}).AllowAnonymous();
//app.UseWhen(
//    ctx => ctx.Request.Path.StartsWithSegments("/api/stripe/webhook"),
//    appBuilder =>
//    {
//        // Skip authentication for Stripe webhook
//    }
//);
app.UseAuthentication();
app.UseAuthorization();
var addresses = app.Urls;
foreach (var addr in addresses)
{
    Console.WriteLine("🔥 API is listening on: " + addr);
}
app.MapControllers();

app.Run();
