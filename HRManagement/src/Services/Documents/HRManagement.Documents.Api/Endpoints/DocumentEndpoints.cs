using HRManagement.Documents.Api.Application.DTOs;
using HRManagement.Documents.Api.Application.Services;

namespace HRManagement.Documents.Api.Endpoints;

public static class DocumentEndpoints
{
    public static void MapDocumentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/documents")
            .WithTags("Документы")
            ;

        group.MapGet("/{id:guid}", async (Guid id, IDocumentService service) =>
        {
            var result = await service.GetByIdAsync(id);
            return result.Success ? Results.Ok(result) : Results.NotFound(result);
        })
        .WithName("GetDocument")
        .WithDescription("Получить документ по ID");

        group.MapGet("/employee/{employeeId:guid}", async (Guid employeeId, IDocumentService service) =>
        {
            var result = await service.GetByEmployeeAsync(employeeId);
            return Results.Ok(result);
        })
        .WithName("GetEmployeeDocuments")
        .WithDescription("Получить документы сотрудника");

        group.MapGet("/type/{type}", async (string type, IDocumentService service) =>
        {
            var result = await service.GetByTypeAsync(type);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithName("GetDocumentsByType")
        .WithDescription("Получить документы по типу");

        group.MapGet("/pending/{signerId:guid}", async (Guid signerId, IDocumentService service) =>
        {
            var result = await service.GetPendingSignatureAsync(signerId);
            return Results.Ok(result);
        })
        .WithName("GetPendingDocuments")
        .WithDescription("Получить документы на подпись");

        group.MapPost("/", async (CreateDocumentDto dto, IDocumentService service) =>
        {
            var result = await service.CreateAsync(dto);
            return result.Success ? Results.Created($"/api/documents/{result.Data?.Id}", result) : Results.BadRequest(result);
        })
        .WithName("CreateDocument")
        .WithDescription("Создать документ");

        group.MapPut("/{id:guid}", async (Guid id, UpdateDocumentDto dto, IDocumentService service) =>
        {
            var result = await service.UpdateAsync(id, dto);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithName("UpdateDocument")
        .WithDescription("Обновить документ");

        group.MapDelete("/{id:guid}", async (Guid id, IDocumentService service) =>
        {
            var result = await service.DeleteAsync(id);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithName("DeleteDocument")
        .WithDescription("Удалить документ");

        group.MapPost("/{id:guid}/signers", async (Guid id, AddSignerDto dto, IDocumentService service) =>
        {
            var result = await service.AddSignerAsync(id, dto);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithName("AddSigner")
        .WithDescription("Добавить подписанта");

        group.MapPost("/{id:guid}/submit", async (Guid id, IDocumentService service) =>
        {
            var result = await service.SubmitForSignatureAsync(id);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithName("SubmitForSignature")
        .WithDescription("Отправить на подпись");

        group.MapPost("/{id:guid}/sign", async (Guid id, SignDocumentDto dto, IDocumentService service) =>
        {
            var result = await service.SignAsync(id, dto);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithName("SignDocument")
        .WithDescription("Подписать документ");

        group.MapPost("/{id:guid}/reject", async (Guid id, RejectSignatureDto dto, IDocumentService service) =>
        {
            var result = await service.RejectAsync(id, dto);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithName("RejectDocument")
        .WithDescription("Отклонить документ");

        group.MapPost("/{id:guid}/archive", async (Guid id, IDocumentService service) =>
        {
            var result = await service.ArchiveAsync(id);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithName("ArchiveDocument")
        .WithDescription("Архивировать документ");
    }
}
