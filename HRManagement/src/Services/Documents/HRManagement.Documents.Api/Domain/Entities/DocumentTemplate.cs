namespace HRManagement.Documents.Api.Domain.Entities;

public class DocumentTemplate
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public DocumentType Type { get; set; }
    public string Content { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
