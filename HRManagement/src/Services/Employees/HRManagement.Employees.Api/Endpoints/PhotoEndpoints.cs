using HRManagement.Employees.Api.Application.Services;
using HRManagement.Employees.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Employees.Api.Endpoints;

public static class PhotoEndpoints
{
    public static IEndpointRouteBuilder MapPhotoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/photos")
            .WithTags("Фотографии")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapPost("/employee/{employeeId:guid}", UploadEmployeePhoto)
            .WithName("UploadEmployeePhoto")
            .WithDescription("Загрузить фото сотрудника")
            .DisableAntiforgery()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/employee/{employeeId:guid}", GetEmployeePhoto)
            .WithName("GetEmployeePhoto")
            .WithDescription("Получить фото сотрудника")
            .AllowAnonymous()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/employee/{employeeId:guid}", DeleteEmployeePhoto)
            .WithName("DeleteEmployeePhoto")
            .WithDescription("Удалить фото сотрудника")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> UploadEmployeePhoto(
        Guid employeeId,
        IFormFile photo,
        IFileStorageService fileService,
        EmployeesDbContext context,
        CancellationToken ct)
    {
        var employee = await context.Employees.FindAsync(new object[] { employeeId }, ct);
        if (employee == null)
            return Results.NotFound(new { Success = false, Message = "Сотрудник не найден" });

        try
        {
            if (!string.IsNullOrEmpty(employee.PhotoPath))
                await fileService.DeleteFileAsync(employee.PhotoPath, ct);

            var filePath = await fileService.SaveFileAsync(photo, "employees", ct);
            employee.PhotoPath = filePath;
            employee.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(ct);

            return Results.Ok(new { Success = true, Message = "Фото загружено", PhotoUrl = fileService.GetFileUrl(filePath) });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    private static async Task<IResult> GetEmployeePhoto(
        Guid employeeId,
        IFileStorageService fileService,
        EmployeesDbContext context,
        CancellationToken ct)
    {
        var employee = await context.Employees.FindAsync(new object[] { employeeId }, ct);
        if (employee == null || string.IsNullOrEmpty(employee.PhotoPath))
            return Results.NotFound(new { Success = false, Message = "Фото не найдено" });

        var fileBytes = await fileService.GetFileAsync(employee.PhotoPath, ct);
        if (fileBytes == null)
            return Results.NotFound(new { Success = false, Message = "Файл не найден" });

        var extension = Path.GetExtension(employee.PhotoPath).ToLowerInvariant();
        var contentType = extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };

        return Results.File(fileBytes, contentType);
    }

    private static async Task<IResult> DeleteEmployeePhoto(
        Guid employeeId,
        IFileStorageService fileService,
        EmployeesDbContext context,
        CancellationToken ct)
    {
        var employee = await context.Employees.FindAsync(new object[] { employeeId }, ct);
        if (employee == null)
            return Results.NotFound(new { Success = false, Message = "Сотрудник не найден" });

        if (string.IsNullOrEmpty(employee.PhotoPath))
            return Results.NotFound(new { Success = false, Message = "Фото не найдено" });

        await fileService.DeleteFileAsync(employee.PhotoPath, ct);
        employee.PhotoPath = null;
        employee.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);

        return Results.Ok(new { Success = true, Message = "Фото удалено" });
    }
}
