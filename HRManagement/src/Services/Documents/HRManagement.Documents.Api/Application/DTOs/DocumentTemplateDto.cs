namespace HRManagement.Documents.Api.Application.DTOs;

public record DocumentTemplateDto(
    Guid Id,
    string Name,
    string Type,
    string Content,
    string? Description,
    bool IsActive);

public record CreateDocumentTemplateDto(
    string Name,
    string Type,
    string Content,
    string? Description);

public record UpdateDocumentTemplateDto(
    string? Name,
    string? Content,
    string? Description,
    bool? IsActive);
