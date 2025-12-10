using HRManagement.Shared.Common.Models;

namespace HRManagement.Recruitment.Api.Domain.Entities;

public class TrainingParticipant : BaseEntity
{
    public Guid TrainingId { get; set; }
    public Training Training { get; set; } = null!;
    
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    
    public ParticipationStatus Status { get; set; } = ParticipationStatus.Registered;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    
    public string? CertificateNumber { get; set; }
    public int? Score { get; set; }
    public string? Feedback { get; set; }
}

public enum ParticipationStatus
{
    Registered,
    Confirmed,
    InProgress,
    Completed,
    Failed,
    Cancelled
}
