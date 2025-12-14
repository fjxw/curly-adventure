using HRManagement.Documents.Api.Application.DTOs;
using HRManagement.Documents.Api.Application.Services;

namespace HRManagement.Documents.Api.Endpoints;

public static class DocumentTemplateEndpoints
{
    public static void MapDocumentTemplateEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/templates")
            .WithTags("Шаблоны документов")
            ;

        group.MapGet("/{id:guid}", async (Guid id, IDocumentTemplateService service) =>
        {
            var result = await service.GetByIdAsync(id);
            return result.Success ? Results.Ok(result) : Results.NotFound(result);
        })
        .WithName("GetTemplate")
        .WithDescription("Получить шаблон по ID");

        group.MapGet("/", async (IDocumentTemplateService service) =>
        {
            var result = await service.GetAllAsync();
            return Results.Ok(result);
        })
        .WithName("GetAllTemplates")
        .WithDescription("Получить все шаблоны");

        group.MapGet("/type/{type}", async (string type, IDocumentTemplateService service) =>
        {
            var result = await service.GetByTypeAsync(type);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithName("GetTemplatesByType")
        .WithDescription("Получить шаблоны по типу документа");

        group.MapPost("/", async (CreateDocumentTemplateDto dto, IDocumentTemplateService service) =>
        {
            var result = await service.CreateAsync(dto);
            return result.Success ? Results.Created($"/api/templates/{result.Data?.Id}", result) : Results.BadRequest(result);
        })
        .WithName("CreateTemplate")
        .WithDescription("Создать шаблон");

        group.MapPut("/{id:guid}", async (Guid id, UpdateDocumentTemplateDto dto, IDocumentTemplateService service) =>
        {
            var result = await service.UpdateAsync(id, dto);
            return result.Success ? Results.Ok(result) : Results.NotFound(result);
        })
        .WithName("UpdateTemplate")
        .WithDescription("Обновить шаблон");

        group.MapDelete("/{id:guid}", async (Guid id, IDocumentTemplateService service) =>
        {
            var result = await service.DeleteAsync(id);
            return result.Success ? Results.Ok(result) : Results.NotFound(result);
        })
        .WithName("DeleteTemplate")
        .WithDescription("Удалить шаблон");
    }
}
