using Microsoft.AspNetCore.Builder;

namespace TradePlatform.Api.Middleware
{
    public static class ErrorHandlingExtensions
    {
        public static IApplicationBuilder UseGlobalErrorHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
}
