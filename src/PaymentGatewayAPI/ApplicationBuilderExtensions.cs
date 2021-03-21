using Microsoft.AspNetCore.Builder;

namespace WebAPI
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandler>();
        }
    }
}