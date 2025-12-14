namespace HRManagement.Shared.Contracts.Events;

public record DocumentCreatedEvent(
    Guid DocumentId,
    Guid EmployeeId,
    string DocumentType,
    string DocumentNumber,
    DateTime CreatedAt);

public record DocumentSignedEvent(
    Guid DocumentId,
    Guid SignedById,
    DateTime SignedAt);
