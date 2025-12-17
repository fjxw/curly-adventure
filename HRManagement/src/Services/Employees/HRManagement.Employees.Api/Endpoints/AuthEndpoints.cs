using HRManagement.Employees.Api.Application.DTOs;
using HRManagement.Employees.Api.Application.Services;

namespace HRManagement.Employees.Api.Endpoints;

public static class AuthEndpoints
{
    private const string CookieName = "access_token";

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Аутентификация")
            .WithOpenApi();

        group.MapPost("/register", Register)
            .WithName("Register")
            .WithDescription("Регистрация нового пользователя")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/login", Login)
            .WithName("Login")
            .WithDescription("Вход в систему")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/logout", Logout)
            .WithName("Logout")
            .WithDescription("Выход из системы")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/me", GetCurrentUser)
            .WithName("GetCurrentUser")
            .WithDescription("Получить информацию о текущем пользователе")
            .RequireAuthorization()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        return app;
    }

    private static async Task<IResult> Register(
        RegisterRequest request,
        IAuthService authService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await authService.RegisterAsync(request, cancellationToken);
        
        if (result.Success && result.Data != null)
        {
            SetTokenCookie(httpContext, result.Data.Token, result.Data.ExpiresAt);
        }
        
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> Login(
        LoginRequest request,
        IAuthService authService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);
        
        if (result.Success && result.Data != null)
        {
            SetTokenCookie(httpContext, result.Data.Token, result.Data.ExpiresAt);
        }
        
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static IResult Logout(HttpContext httpContext)
    {
        httpContext.Response.Cookies.Delete(CookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/"
        });

        return Results.Ok(new { message = "Выход выполнен успешно" });
    }

    private static IResult GetCurrentUser(HttpContext httpContext)
    {
        var user = httpContext.User;
        
        if (user.Identity?.IsAuthenticated != true)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new
        {
            id = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            email = user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
            name = user.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
        });
    }

    private static void SetTokenCookie(HttpContext httpContext, string token, DateTime expiresAt)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = expiresAt,
            Path = "/"
        };

        httpContext.Response.Cookies.Append(CookieName, token, cookieOptions);
    }
}
