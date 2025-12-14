using HRManagement.Employees.Api.Application.DTOs;
using HRManagement.Employees.Api.Application.Services;

namespace HRManagement.Employees.Api.Endpoints;

public static class AuthEndpoints
{
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

        return app;
    }

    private static async Task<IResult> Register(
        RegisterRequest request,
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        var result = await authService.RegisterAsync(request, cancellationToken);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> Login(
        LoginRequest request,
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }
}
