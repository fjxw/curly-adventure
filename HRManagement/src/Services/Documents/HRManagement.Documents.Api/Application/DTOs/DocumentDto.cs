namespace HRManagement.Documents.Api.Application.DTOs;

public record DocumentDto(
    Guid Id,
    Guid EmployeeId,
    string DocumentNumber,
    string Type,
    string Title,
    string? Description,
    string? Content,
    string? FilePath,
    string Status,
    DateTime CreatedAt,
    Guid CreatedById,
    DateTime? EffectiveDate,
    DateTime? ExpirationDate,
    IEnumerable<DocumentSignatureDto> Signatures);

public record CreateDocumentDto(
    Guid EmployeeId,
    string Type,
    string Title,
    string? Description,
    string? Content,
    DateTime? EffectiveDate,
    DateTime? ExpirationDate,
    Guid CreatedById);

public record UpdateDocumentDto(
    string? Title,
    string? Description,
    string? Content,
    DateTime? EffectiveDate,
    DateTime? ExpirationDate);

public record DocumentSignatureDto(
    Guid Id,
    Guid SignerId,
    string SignerName,
    string SignerPosition,
    string Status,
    int SignOrder,
    DateTime? SignedAt,
    string? Comment);

public record AddSignerDto(
    Guid SignerId,
    string SignerName,
    string SignerPosition,
    int SignOrder);

public record SignDocumentDto(
    Guid SignerId,
    string? Comment);

public record RejectSignatureDto(
    Guid SignerId,
    string Comment);
