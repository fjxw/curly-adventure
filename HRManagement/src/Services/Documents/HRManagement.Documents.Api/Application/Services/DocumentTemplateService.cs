using Microsoft.EntityFrameworkCore;
using HRManagement.Documents.Api.Domain.Entities;
using HRManagement.Documents.Api.Application.DTOs;
using HRManagement.Documents.Api.Infrastructure.Data;
using HRManagement.Shared.Common.Models;

namespace HRManagement.Documents.Api.Application.Services;

public interface IDocumentTemplateService
{
    Task<ApiResponse<DocumentTemplateDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<IEnumerable<DocumentTemplateDto>>> GetAllAsync();
    Task<ApiResponse<IEnumerable<DocumentTemplateDto>>> GetByTypeAsync(string type);
    Task<ApiResponse<DocumentTemplateDto>> CreateAsync(CreateDocumentTemplateDto dto);
    Task<ApiResponse<DocumentTemplateDto>> UpdateAsync(Guid id, UpdateDocumentTemplateDto dto);
    Task<ApiResponse<bool>> DeleteAsync(Guid id);
}

public class DocumentTemplateService : IDocumentTemplateService
{
    private readonly DocumentsDbContext _context;

    public DocumentTemplateService(DocumentsDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<DocumentTemplateDto>> GetByIdAsync(Guid id)
    {
        var template = await _context.DocumentTemplates.FindAsync(id);
        if (template == null)
            return ApiResponse<DocumentTemplateDto>.FailureResponse("Шаблон не найден");

        return ApiResponse<DocumentTemplateDto>.SuccessResponse(MapToDto(template));
    }

    public async Task<ApiResponse<IEnumerable<DocumentTemplateDto>>> GetAllAsync()
    {
        var templates = await _context.DocumentTemplates
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync();
        return ApiResponse<IEnumerable<DocumentTemplateDto>>.SuccessResponse(templates.Select(MapToDto));
    }

    public async Task<ApiResponse<IEnumerable<DocumentTemplateDto>>> GetByTypeAsync(string type)
    {
        if (!Enum.TryParse<DocumentType>(type, out var docType))
            return ApiResponse<IEnumerable<DocumentTemplateDto>>.FailureResponse("Неверный тип документа");

        var templates = await _context.DocumentTemplates
            .Where(t => t.Type == docType && t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync();
        return ApiResponse<IEnumerable<DocumentTemplateDto>>.SuccessResponse(templates.Select(MapToDto));
    }

    public async Task<ApiResponse<DocumentTemplateDto>> CreateAsync(CreateDocumentTemplateDto dto)
    {
        if (!Enum.TryParse<DocumentType>(dto.Type, out var docType))
            return ApiResponse<DocumentTemplateDto>.FailureResponse("Неверный тип документа");

        var template = new DocumentTemplate
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Type = docType,
            Content = dto.Content,
            Description = dto.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.DocumentTemplates.Add(template);
        await _context.SaveChangesAsync();

        return ApiResponse<DocumentTemplateDto>.SuccessResponse(MapToDto(template), "Шаблон создан");
    }

    public async Task<ApiResponse<DocumentTemplateDto>> UpdateAsync(Guid id, UpdateDocumentTemplateDto dto)
    {
        var template = await _context.DocumentTemplates.FindAsync(id);
        if (template == null)
            return ApiResponse<DocumentTemplateDto>.FailureResponse("Шаблон не найден");

        if (dto.Name != null) template.Name = dto.Name;
        if (dto.Content != null) template.Content = dto.Content;
        if (dto.Description != null) template.Description = dto.Description;
        if (dto.IsActive.HasValue) template.IsActive = dto.IsActive.Value;

        template.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse<DocumentTemplateDto>.SuccessResponse(MapToDto(template), "Шаблон обновлен");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        var template = await _context.DocumentTemplates.FindAsync(id);
        if (template == null)
            return ApiResponse<bool>.FailureResponse("Шаблон не найден");

        _context.DocumentTemplates.Remove(template);
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResponse(true, "Шаблон удален");
    }

    private static DocumentTemplateDto MapToDto(DocumentTemplate t) => new(
        t.Id, t.Name, t.Type.ToString(), t.Content, t.Description, t.IsActive);
}
