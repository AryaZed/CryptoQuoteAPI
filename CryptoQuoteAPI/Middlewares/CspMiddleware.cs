using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace CryptoQuoteAPI.Middlewares
{
    public class CspMiddleware
    {
        private readonly RequestDelegate _next;

        public CspMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var csp = "default-src 'self'; " +
                      "script-src 'self' https://cdnjs.cloudflare.com; " +
                      "style-src 'self' https://cdnjs.cloudflare.com; " +
                      "img-src 'self'; " +
                      "connect-src 'self' /cryptohub; " +
                      "font-src 'self'; " +
                      "object-src 'none'; " +
                      "frame-ancestors 'none';";

            context.Response.Headers.Add("Content-Security-Policy", csp);
            await _next(context);
        }
    }

    public static class CspMiddlewareExtensions
    {
        public static IApplicationBuilder UseCsp(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CspMiddleware>();
        }
    }
}
