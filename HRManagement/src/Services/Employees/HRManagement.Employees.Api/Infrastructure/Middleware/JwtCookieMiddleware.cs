namespace HRManagement.Employees.Api.Infrastructure.Middleware;

public class JwtCookieMiddleware
{
    private readonly RequestDelegate _next;
    private const string CookieName = "access_token";

    public JwtCookieMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.ContainsKey("Authorization"))
        {
            if (context.Request.Cookies.TryGetValue(CookieName, out var token))
            {
                context.Request.Headers.Append("Authorization", $"Bearer {token}");
            }
        }

        await _next(context);
    }
}

public static class JwtCookieMiddlewareExtensions
{
    public static IApplicationBuilder UseJwtCookieAuthentication(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<JwtCookieMiddleware>();
    }
}
