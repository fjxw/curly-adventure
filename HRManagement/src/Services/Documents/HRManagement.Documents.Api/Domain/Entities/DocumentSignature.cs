namespace HRManagement.Documents.Api.Domain.Entities;

public class DocumentSignature
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public Document Document { get; set; } = null!;
    public Guid SignerId { get; set; }
    public string SignerName { get; set; } = null!;
    public string SignerPosition { get; set; } = null!;
    public SignatureStatus Status { get; set; }
    public int SignOrder { get; set; }
    public DateTime? SignedAt { get; set; }
    public string? Comment { get; set; }
}

public enum SignatureStatus
{
    Pending,
    Signed,
    Rejected
}
