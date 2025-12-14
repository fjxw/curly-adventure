using HRManagement.Recruitment.Api.Application.Services;
using HRManagement.Recruitment.Api.Infrastructure.Data;

namespace HRManagement.Recruitment.Api.Endpoints;

public static class CandidateFileEndpoints
{
    private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private static readonly string[] ResumeExtensions = { ".pdf", ".doc", ".docx", ".rtf", ".odt" };

    public static IEndpointRouteBuilder MapCandidateFileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/candidates")
            .WithTags("Файлы кандидатов")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapPost("/{candidateId:guid}/photo", UploadPhoto)
            .WithName("UploadCandidatePhoto")
            .WithDescription("Загрузить фото кандидата")
            .DisableAntiforgery()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/{candidateId:guid}/photo", GetPhoto)
            .WithName("GetCandidatePhoto")
            .WithDescription("Получить фото кандидата")
            .AllowAnonymous()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{candidateId:guid}/resume", UploadResume)
            .WithName("UploadCandidateResume")
            .WithDescription("Загрузить резюме кандидата")
            .DisableAntiforgery()
            .AllowAnonymous()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/{candidateId:guid}/resume", GetResume)
            .WithName("GetCandidateResume")
            .WithDescription("Скачать резюме кандидата")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> UploadPhoto(
        Guid candidateId,
        IFormFile photo,
        IFileStorageService fileService,
        RecruitmentDbContext context,
        CancellationToken ct)
    {
        var candidate = await context.Candidates.FindAsync(new object[] { candidateId }, ct);
        if (candidate == null)
            return Results.NotFound(new { Success = false, Message = "Кандидат не найден" });

        try
        {
            if (!string.IsNullOrEmpty(candidate.PhotoPath))
                await fileService.DeleteFileAsync(candidate.PhotoPath, ct);

            var filePath = await fileService.SaveFileAsync(photo, "candidates/photos", ImageExtensions, ct);
            candidate.PhotoPath = filePath;
            candidate.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(ct);

            return Results.Ok(new { Success = true, Message = "Фото загружено", PhotoUrl = fileService.GetFileUrl(filePath) });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    private static async Task<IResult> GetPhoto(
        Guid candidateId,
        IFileStorageService fileService,
        RecruitmentDbContext context,
        CancellationToken ct)
    {
        var candidate = await context.Candidates.FindAsync(new object[] { candidateId }, ct);
        if (candidate == null || string.IsNullOrEmpty(candidate.PhotoPath))
            return Results.NotFound(new { Success = false, Message = "Фото не найдено" });

        var fileBytes = await fileService.GetFileAsync(candidate.PhotoPath, ct);
        if (fileBytes == null)
            return Results.NotFound(new { Success = false, Message = "Файл не найден" });

        var extension = Path.GetExtension(candidate.PhotoPath).ToLowerInvariant();
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

    private static async Task<IResult> UploadResume(
        Guid candidateId,
        IFormFile resume,
        IFileStorageService fileService,
        RecruitmentDbContext context,
        CancellationToken ct)
    {
        var candidate = await context.Candidates.FindAsync(new object[] { candidateId }, ct);
        if (candidate == null)
            return Results.NotFound(new { Success = false, Message = "Кандидат не найден" });

        try
        {
            if (!string.IsNullOrEmpty(candidate.ResumePath))
                await fileService.DeleteFileAsync(candidate.ResumePath, ct);

            var filePath = await fileService.SaveFileAsync(resume, "candidates/resumes", ResumeExtensions, ct);
            candidate.ResumePath = filePath;
            candidate.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(ct);

            return Results.Ok(new { Success = true, Message = "Резюме загружено", ResumeUrl = fileService.GetFileUrl(filePath) });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    private static async Task<IResult> GetResume(
        Guid candidateId,
        IFileStorageService fileService,
        RecruitmentDbContext context,
        CancellationToken ct)
    {
        var candidate = await context.Candidates.FindAsync(new object[] { candidateId }, ct);
        if (candidate == null || string.IsNullOrEmpty(candidate.ResumePath))
            return Results.NotFound(new { Success = false, Message = "Резюме не найдено" });

        var fileBytes = await fileService.GetFileAsync(candidate.ResumePath, ct);
        if (fileBytes == null)
            return Results.NotFound(new { Success = false, Message = "Файл не найден" });

        var extension = Path.GetExtension(candidate.ResumePath).ToLowerInvariant();
        var contentType = extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".rtf" => "application/rtf",
            ".odt" => "application/vnd.oasis.opendocument.text",
            _ => "application/octet-stream"
        };

        var fileName = $"{candidate.LastName}_{candidate.FirstName}_resume{extension}";
        return Results.File(fileBytes, contentType, fileName);
    }
}
