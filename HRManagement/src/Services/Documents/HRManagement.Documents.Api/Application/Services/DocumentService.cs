using Microsoft.EntityFrameworkCore;
using HRManagement.Documents.Api.Domain.Entities;
using HRManagement.Documents.Api.Application.DTOs;
using HRManagement.Documents.Api.Infrastructure.Data;
using HRManagement.Shared.Common.Models;
using HRManagement.Shared.MessageBus;
using HRManagement.Shared.Contracts.Events;

namespace HRManagement.Documents.Api.Application.Services;

public interface IDocumentService
{
    Task<ApiResponse<DocumentDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<IEnumerable<DocumentDto>>> GetByEmployeeAsync(Guid employeeId);
    Task<ApiResponse<IEnumerable<DocumentDto>>> GetByTypeAsync(string type);
    Task<ApiResponse<IEnumerable<DocumentDto>>> GetPendingSignatureAsync(Guid signerId);
    Task<ApiResponse<DocumentDto>> CreateAsync(CreateDocumentDto dto);
    Task<ApiResponse<DocumentDto>> UpdateAsync(Guid id, UpdateDocumentDto dto);
    Task<ApiResponse<bool>> DeleteAsync(Guid id);
    Task<ApiResponse<DocumentDto>> AddSignerAsync(Guid documentId, AddSignerDto dto);
    Task<ApiResponse<DocumentDto>> SubmitForSignatureAsync(Guid id);
    Task<ApiResponse<DocumentDto>> SignAsync(Guid documentId, SignDocumentDto dto);
    Task<ApiResponse<DocumentDto>> RejectAsync(Guid documentId, RejectSignatureDto dto);
    Task<ApiResponse<DocumentDto>> ArchiveAsync(Guid id);
}

public class DocumentService : IDocumentService
{
    private readonly DocumentsDbContext _context;
    private readonly IEventBus _eventBus;

    public DocumentService(DocumentsDbContext context, IEventBus eventBus)
    {
        _context = context;
        _eventBus = eventBus;
    }

    public async Task<ApiResponse<DocumentDto>> GetByIdAsync(Guid id)
    {
        var document = await _context.Documents
            .Include(d => d.Signatures.OrderBy(s => s.SignOrder))
            .FirstOrDefaultAsync(d => d.Id == id);

        if (document == null)
            return ApiResponse<DocumentDto>.FailureResponse("Документ не найден");

        return ApiResponse<DocumentDto>.SuccessResponse(MapToDto(document));
    }

    public async Task<ApiResponse<IEnumerable<DocumentDto>>> GetByEmployeeAsync(Guid employeeId)
    {
        var documents = await _context.Documents
            .Include(d => d.Signatures.OrderBy(s => s.SignOrder))
            .Where(d => d.EmployeeId == employeeId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
        return ApiResponse<IEnumerable<DocumentDto>>.SuccessResponse(documents.Select(MapToDto));
    }

    public async Task<ApiResponse<IEnumerable<DocumentDto>>> GetByTypeAsync(string type)
    {
        if (!Enum.TryParse<DocumentType>(type, out var docType))
            return ApiResponse<IEnumerable<DocumentDto>>.FailureResponse("Неверный тип документа");

        var documents = await _context.Documents
            .Include(d => d.Signatures.OrderBy(s => s.SignOrder))
            .Where(d => d.Type == docType)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
        return ApiResponse<IEnumerable<DocumentDto>>.SuccessResponse(documents.Select(MapToDto));
    }

    public async Task<ApiResponse<IEnumerable<DocumentDto>>> GetPendingSignatureAsync(Guid signerId)
    {
        var documents = await _context.Documents
            .Include(d => d.Signatures.OrderBy(s => s.SignOrder))
            .Where(d => d.Status == DocumentStatus.PendingSignature &&
                        d.Signatures.Any(s => s.SignerId == signerId && s.Status == SignatureStatus.Pending))
            .OrderBy(d => d.CreatedAt)
            .ToListAsync();
        return ApiResponse<IEnumerable<DocumentDto>>.SuccessResponse(documents.Select(MapToDto));
    }

    public async Task<ApiResponse<DocumentDto>> CreateAsync(CreateDocumentDto dto)
    {
        if (!Enum.TryParse<DocumentType>(dto.Type, out var docType))
            return ApiResponse<DocumentDto>.FailureResponse("Неверный тип документа");

        var documentNumber = GenerateDocumentNumber(docType);

        var document = new Document
        {
            Id = Guid.NewGuid(),
            EmployeeId = dto.EmployeeId,
            DocumentNumber = documentNumber,
            Type = docType,
            Title = dto.Title,
            Description = dto.Description,
            Content = dto.Content,
            Status = DocumentStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            CreatedById = dto.CreatedById,
            EffectiveDate = dto.EffectiveDate,
            ExpirationDate = dto.ExpirationDate
        };

        _context.Documents.Add(document);
        await _context.SaveChangesAsync();

        await _eventBus.PublishAsync(new DocumentCreatedEvent(
            document.Id,
            document.EmployeeId,
            document.Type.ToString(),
            document.DocumentNumber,
            document.CreatedAt));

        return ApiResponse<DocumentDto>.SuccessResponse(MapToDto(document), "Документ создан");
    }

    public async Task<ApiResponse<DocumentDto>> UpdateAsync(Guid id, UpdateDocumentDto dto)
    {
        var document = await _context.Documents
            .Include(d => d.Signatures)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (document == null)
            return ApiResponse<DocumentDto>.FailureResponse("Документ не найден");

        if (document.Status != DocumentStatus.Draft)
            return ApiResponse<DocumentDto>.FailureResponse("Можно редактировать только черновики");

        if (dto.Title != null) document.Title = dto.Title;
        if (dto.Description != null) document.Description = dto.Description;
        if (dto.Content != null) document.Content = dto.Content;
        if (dto.EffectiveDate.HasValue) document.EffectiveDate = dto.EffectiveDate.Value;
        if (dto.ExpirationDate.HasValue) document.ExpirationDate = dto.ExpirationDate.Value;

        await _context.SaveChangesAsync();
        return ApiResponse<DocumentDto>.SuccessResponse(MapToDto(document), "Документ обновлен");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        var document = await _context.Documents.FindAsync(id);
        if (document == null)
            return ApiResponse<bool>.FailureResponse("Документ не найден");

        if (document.Status != DocumentStatus.Draft)
            return ApiResponse<bool>.FailureResponse("Можно удалять только черновики");

        _context.Documents.Remove(document);
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResponse(true, "Документ удален");
    }

    public async Task<ApiResponse<DocumentDto>> AddSignerAsync(Guid documentId, AddSignerDto dto)
    {
        var document = await _context.Documents
            .Include(d => d.Signatures)
            .FirstOrDefaultAsync(d => d.Id == documentId);

        if (document == null)
            return ApiResponse<DocumentDto>.FailureResponse("Документ не найден");

        if (document.Status != DocumentStatus.Draft)
            return ApiResponse<DocumentDto>.FailureResponse("Подписанты добавляются только к черновикам");

        var signature = new DocumentSignature
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            SignerId = dto.SignerId,
            SignerName = dto.SignerName,
            SignerPosition = dto.SignerPosition,
            SignOrder = dto.SignOrder,
            Status = SignatureStatus.Pending
        };

        document.Signatures.Add(signature);
        await _context.SaveChangesAsync();

        return ApiResponse<DocumentDto>.SuccessResponse(MapToDto(document), "Подписант добавлен");
    }

    public async Task<ApiResponse<DocumentDto>> SubmitForSignatureAsync(Guid id)
    {
        var document = await _context.Documents
            .Include(d => d.Signatures)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (document == null)
            return ApiResponse<DocumentDto>.FailureResponse("Документ не найден");

        if (document.Status != DocumentStatus.Draft)
            return ApiResponse<DocumentDto>.FailureResponse("Можно отправить на подпись только черновик");

        if (!document.Signatures.Any())
            return ApiResponse<DocumentDto>.FailureResponse("Добавьте хотя бы одного подписанта");

        document.Status = DocumentStatus.PendingSignature;
        await _context.SaveChangesAsync();

        return ApiResponse<DocumentDto>.SuccessResponse(MapToDto(document), "Документ отправлен на подпись");
    }

    public async Task<ApiResponse<DocumentDto>> SignAsync(Guid documentId, SignDocumentDto dto)
    {
        var document = await _context.Documents
            .Include(d => d.Signatures.OrderBy(s => s.SignOrder))
            .FirstOrDefaultAsync(d => d.Id == documentId);

        if (document == null)
            return ApiResponse<DocumentDto>.FailureResponse("Документ не найден");

        if (document.Status != DocumentStatus.PendingSignature)
            return ApiResponse<DocumentDto>.FailureResponse("Документ не ожидает подписи");

        var signature = document.Signatures.FirstOrDefault(s => s.SignerId == dto.SignerId && s.Status == SignatureStatus.Pending);
        if (signature == null)
            return ApiResponse<DocumentDto>.FailureResponse("Подпись не найдена или уже проставлена");

        var previousSignatures = document.Signatures
            .Where(s => s.SignOrder < signature.SignOrder && s.Status == SignatureStatus.Pending);

        if (previousSignatures.Any())
            return ApiResponse<DocumentDto>.FailureResponse("Дождитесь подписи предыдущих подписантов");

        signature.Status = SignatureStatus.Signed;
        signature.SignedAt = DateTime.UtcNow;
        signature.Comment = dto.Comment;

        if (document.Signatures.All(s => s.Status == SignatureStatus.Signed))
            document.Status = DocumentStatus.Signed;

        await _context.SaveChangesAsync();

        await _eventBus.PublishAsync(new DocumentSignedEvent(
            document.Id,
            dto.SignerId,
            DateTime.UtcNow));

        var message = document.Status == DocumentStatus.Signed
            ? "Документ полностью подписан"
            : "Подпись проставлена";

        return ApiResponse<DocumentDto>.SuccessResponse(MapToDto(document), message);
    }

    public async Task<ApiResponse<DocumentDto>> RejectAsync(Guid documentId, RejectSignatureDto dto)
    {
        var document = await _context.Documents
            .Include(d => d.Signatures)
            .FirstOrDefaultAsync(d => d.Id == documentId);

        if (document == null)
            return ApiResponse<DocumentDto>.FailureResponse("Документ не найден");

        if (document.Status != DocumentStatus.PendingSignature)
            return ApiResponse<DocumentDto>.FailureResponse("Документ не ожидает подписи");

        var signature = document.Signatures.FirstOrDefault(s => s.SignerId == dto.SignerId && s.Status == SignatureStatus.Pending);
        if (signature == null)
            return ApiResponse<DocumentDto>.FailureResponse("Подпись не найдена");

        signature.Status = SignatureStatus.Rejected;
        signature.Comment = dto.Comment;
        signature.SignedAt = DateTime.UtcNow;

        document.Status = DocumentStatus.Rejected;
        await _context.SaveChangesAsync();

        return ApiResponse<DocumentDto>.SuccessResponse(MapToDto(document), "Документ отклонен");
    }

    public async Task<ApiResponse<DocumentDto>> ArchiveAsync(Guid id)
    {
        var document = await _context.Documents
            .Include(d => d.Signatures)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (document == null)
            return ApiResponse<DocumentDto>.FailureResponse("Документ не найден");

        document.Status = DocumentStatus.Archived;
        await _context.SaveChangesAsync();

        return ApiResponse<DocumentDto>.SuccessResponse(MapToDto(document), "Документ архивирован");
    }

    private static string GenerateDocumentNumber(DocumentType type)
    {
        var prefix = type switch
        {
            DocumentType.HiringOrder => "ПР-Н",
            DocumentType.TerminationOrder => "ПР-У",
            DocumentType.TransferOrder => "ПР-П",
            DocumentType.VacationOrder => "ПР-О",
            DocumentType.BusinessTripOrder => "ПР-К",
            DocumentType.BonusOrder => "ПР-Б",
            DocumentType.DisciplinaryOrder => "ПР-Д",
            DocumentType.EmploymentContract => "ТД",
            DocumentType.Amendment => "ДС",
            DocumentType.Application => "ЗАЯ",
            DocumentType.Certificate => "СПР",
            DocumentType.Memo => "СЗ",
            DocumentType.Report => "ОТЧ",
            DocumentType.Policy => "ПОЛ",
            _ => "ДОК"
        };
        return $"{prefix}-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }

    private static DocumentDto MapToDto(Document doc) => new(
        doc.Id,
        doc.EmployeeId,
        doc.DocumentNumber,
        doc.Type.ToString(),
        doc.Title,
        doc.Description,
        doc.Content,
        doc.FilePath,
        doc.Status.ToString(),
        doc.CreatedAt,
        doc.CreatedById,
        doc.EffectiveDate,
        doc.ExpirationDate,
        doc.Signatures.Select(s => new DocumentSignatureDto(
            s.Id, s.SignerId, s.SignerName, s.SignerPosition,
            s.Status.ToString(), s.SignOrder, s.SignedAt, s.Comment)));
}
