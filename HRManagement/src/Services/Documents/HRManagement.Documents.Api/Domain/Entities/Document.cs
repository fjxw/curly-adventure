namespace HRManagement.Documents.Api.Domain.Entities;

public class Document
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string DocumentNumber { get; set; } = null!;
    public DocumentType Type { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? Content { get; set; }
    public string? FilePath { get; set; }
    public DocumentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedById { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public ICollection<DocumentSignature> Signatures { get; set; } = new List<DocumentSignature>();
}

public enum DocumentType
{
    HiringOrder,
    TerminationOrder,
    TransferOrder,
    VacationOrder,
    BusinessTripOrder,
    BonusOrder,
    DisciplinaryOrder,
    EmploymentContract,
    Amendment,
    Application,
    Certificate,
    Memo,
    Report,
    Policy,
    Other
}

public enum DocumentStatus
{
    Draft,
    PendingSignature,
    Signed,
    Rejected,
    Archived
}
